using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Colipu.Common.ActiveMQ.ActiveMq;
using Colipu.Common.ActiveMQ.ActiveMqEnum;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Colipu.Common.ActiveMQ
{
    internal class MqProviderService<T> : IMqProvider<T>
    {
        /// <summary>
        /// amq
        /// </summary>
        private ActiveMqProvider<T> _amq;

        /// <summary>
        /// ConnectionUrl
        /// </summary>
        private string _connectionUrl = ActiveMqProvider.ConnectionUrl;

        /// <summary>
        /// 当前队列类型
        /// </summary>
        private QueueTopicType _queueTopicType;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="queueTopicType">队列消息类型</param>
        public MqProviderService(QueueTopicType queueTopicType, TimeSpan? timeToLive = null)
        {
            _queueTopicType = queueTopicType;
            _amq = new ActiveMqProvider<T>(queueTopicType.ToString(), timeToLive);
        }

        #region 私有方法

        /// <summary>
        /// 生产者发送消息到队列中
        /// </summary>
        /// <param name="message"></param>
        private void SendMsg(MqMessageModel<T> message)
        {
            try
            {
                var result = _amq.SendMsg(message);
                if (!result)
                {
                    var log = JsonConvert.SerializeObject(new
                    {
                        ActiveMqProvider.ConnectionUrl,
                        MqMessage = message,
                        Message = "与服务器连接已断开或者尚未连接成功,数据将发送到Redis灾备"
                    });
                    Debug.WriteLine(log);

                    //灾备到redis
                    //MqProvider.EnqueuRedis(_queueTopicType, message, _amq.TimeToLive);
                }
            }
            catch (Exception ex)
            {
                var log = JsonConvert.SerializeObject(new
                {
                    ActiveMqProvider.ConnectionUrl,
                    MqMessage = message,
                    Exception = ex
                });
                Debug.WriteLine(log);

                Debug.WriteLine(ex);
                var eType = ex.GetType();
                if (eType == typeof(RequestTimedOutException) ||
                    eType == typeof(ConnectionClosedException) ||
                    eType == typeof(BrokerException) ||
                    eType == typeof(NMSConnectionException))
                {
                    //灾备到redis
                    //MqProvider.EnqueuRedis(_queueTopicType, message, _amq.TimeToLive);
                }
                else
                    throw ex;
            }
        }

        #endregion 私有方法

        #region 接口实现

        /// <summary>
        /// 连接字符串
        /// </summary>
        string IMqProvider<T>.ConnectionUrl { get { return _connectionUrl; } }

        /// <summary>
        /// 队列类型
        /// </summary>
        QueueTopicType IMqProvider<T>.QueueTopicType { get { return _queueTopicType; } }

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="body">要发送的数据</param>
        public void Send(T body)
        {
            SendMsg(new MqMessageModel<T> { Body = body, DelayConsume = null, Priority = MsgPriorityType.Normal });
        }

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="body">要发送的数据</param>
        /// <param name="priority">优先级,默认为正常</param>
        public void Send(T body, MsgPriorityType priority)
        {
            SendMsg(new MqMessageModel<T> { Body = body, DelayConsume = null, Priority = priority });
        }

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="body">要发送的数据</param>
        /// <param name="delayConsume">延迟消费时间,单位毫秒,默认为null</param>
        public void Send(T body, long delayConsume)
        {
            SendMsg(new MqMessageModel<T> { Body = body, DelayConsume = delayConsume, Priority = MsgPriorityType.Normal });
        }

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="body">要发送的数据</param>
        /// <param name="priority">优先级,默认为正常</param>
        /// <param name="delayConsume">延迟消费时间,单位毫秒,默认为null</param>
        public void Send(T body, MsgPriorityType priority, long delayConsume)
        {
            SendMsg(new MqMessageModel<T> { Body = body, DelayConsume = delayConsume, Priority = priority });
        }

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="body">要发送的数据</param>
        /// <param name="priority">优先级,默认为正常</param>
        /// <param name="delayConsume">延迟消费时间,单位毫秒,默认为null</param>
        public void Send(T body, MsgPriorityType priority = MsgPriorityType.Normal, long? delayConsume = null)
        {
            SendMsg(new MqMessageModel<T> { Body = body, DelayConsume = delayConsume, Priority = priority });
        }

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="message">mq消息实体</param>
        public void Send(MqMessageModel<T> message)
        {
            SendMsg(message);
        }

        /// <summary>
        /// 监听接收消息,
        /// 当监听方法返回true时该条消息将被消费成功,
        /// 返回false 或者调用时出现异常该条消息将被回滚;
        /// 简单来说该监听处理是在一个消息事务来完成的.
        /// 如果该方法被调用多次,就会产生多个消费者
        /// </summary>
        /// <param name="listenerFunc">监听方法</param>
        public void ListenerReceive(Func<T, bool> listenerFunc)
        {
            _amq.CreateConsumer(listenerFunc);
        }

        #endregion 接口实现
    }
}