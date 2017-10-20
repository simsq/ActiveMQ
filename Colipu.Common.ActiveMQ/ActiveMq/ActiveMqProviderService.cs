using Apache.NMS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Colipu.Common.ActiveMQ.ActiveMq
{
    /// <summary>
    /// ActiveMq供应者
    /// </summary>
    internal class ActiveMqProvider<T>
    {
        /// <summary>
        /// 待绑定的消费者监听
        /// </summary>
        private static readonly List<Func<T, bool>> _waitListenerFuncs = new List<Func<T, bool>>();

        /// <summary>
        /// 队列消息类型
        /// </summary>
        private string _queueTopicType;

        /// <summary>
        /// 当前队列是否为P2P类型的,不为P2P就为P2M
        /// </summary>
        private bool _isP2P;

        /// <summary>
        /// 存活哦时长
        /// </summary>
        public TimeSpan TimeToLive { private set; get; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="connectionUrl">连接地址</param>
        /// <param name="queueTopicType">队列消息类型</param>
        public ActiveMqProvider(string queueTopicType, TimeSpan? timeToLive = null)
        {
            _queueTopicType = queueTopicType;
            TimeToLive = timeToLive ?? new TimeSpan(0);
            var type = _queueTopicType.ToUpper();
            if (type.StartsWith("P2P_"))
                _isP2P = true;
            else if (type.StartsWith("P2M_"))
                _isP2P = false;
            else
                throw new Exception($"QueueTopicType枚举属性:{queueTopicType.ToString()}必须以P2P_或者P2M_开头");
        }

        #region 私有方法

        /// <summary>
        /// CreateDestination=   _isP2P ? Queue : Topic
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        private IDestination CreateDestination(ISession session)
        {
            if (_isP2P)
                return session.GetQueue(_queueTopicType);
            else
                return session.GetTopic(_queueTopicType);
        }

        /// <summary>
        /// 创建生产者
        /// </summary>
        /// <returns></returns>
        private IMessageProducer CreateProducer()
        {
            if (!ActiveMqProvider.IsConnected)
                throw new NMSConnectionException($"ActiveMq.ConnectionUrl:{ActiveMqProvider.Connection},{_queueTopicType},与服务器连接已断开或者尚未连接成功!");
            if (ActiveMqProvider.Producers.ContainsKey(_queueTopicType))
                return ActiveMqProvider.Producers[_queueTopicType];
            var session = ActiveMqProvider.Connection.CreateSession();
            IDestination destination = CreateDestination(session);
            var producer = session.CreateProducer(destination);
            ActiveMqProvider.Producers[_queueTopicType] = producer;
            return producer;
        }

        /// <summary>
        ///
        /// </summary>
        protected void CreateConsumerForWait()
        {
            try
            {
                lock (_waitListenerFuncs)
                {
                    _waitListenerFuncs.ForEach(item =>
                    {
                        CreateConsumer(item);
                    });
                    _waitListenerFuncs.Clear();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        #endregion 私有方法

        #region 公共方法

        /// <summary>
        /// 生产者发送消息到队列中
        /// </summary>
        /// <param name="data"></param>
        /// <param name="priority"></param>
        /// <param name="delayConsume"></param>
        /// <param name="isNeedJsonSerializeObject"></param>
        public bool SendMsg(MqMessageModel<T> message, bool isNeedJsonSerializeObject = true)
        {
            if (!ActiveMqProvider.IsConnected)
                return false;
            //生产者
            var producer = CreateProducer();
            var d = isNeedJsonSerializeObject ? JsonConvert.SerializeObject(message.Body) : (message.Body == null ? null : message.Body.ToString());
            var msg = producer.CreateTextMessage(d);
            Debug.WriteLine(string.Format("Send:{0}", msg.Text));
            if (message.DelayConsume.HasValue) //延时消费时间
                msg.Properties.SetLong(ScheduledType.AMQ_SCHEDULED_DELAY, message.DelayConsume.Value);
            var cPriority = (MsgPriority)Enum.Parse(typeof(MsgPriority), message.Priority.ToString());
            //持久化到硬盘,消费后删除      优先级      最短时间内删除
            producer.Send(msg, MsgDeliveryMode.Persistent, cPriority, TimeToLive);
            return true;
        }

        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <param name="listenerFunc"></param>
        public void CreateConsumer(Func<T, bool> listenerFunc)
        {
            if (listenerFunc == null)
                throw new NullReferenceException("参数listenerFunc,监听消费方法不能为空!");
            if (!ActiveMqProvider.IsConnected)
            {
                lock (_waitListenerFuncs)
                {
                    _waitListenerFuncs.Add(listenerFunc);
                }
                if (!ActiveMqProvider.CreateConsumerForWaitActions.ContainsKey(GetHashCode()))
                    ActiveMqProvider.CreateConsumerForWaitActions.Add(GetHashCode(), CreateConsumerForWait);
                return;
            }
            //创建事务性确认模式的Session
            var session = ActiveMqProvider.Connection.CreateSession(AcknowledgementMode.Transactional);
            IDestination destination = CreateDestination(session);
            var consumer = session.CreateConsumer(destination);
            consumer.Listener += (message) =>
            {
                try
                {
                    var data = ((ITextMessage)message).Text;
                    var m = JsonConvert.DeserializeObject<T>(data);
                    var result = listenerFunc.Invoke(m);
                    if (result)
                        session.Commit();
                    else
                        session.Rollback();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                    session.Rollback();
                }
            };
            ActiveMqProvider._consumers.Add(consumer);
        }

        #endregion 公共方法
    }
}