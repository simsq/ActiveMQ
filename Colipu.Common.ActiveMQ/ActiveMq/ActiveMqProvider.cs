using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Colipu.Common.ActiveMQ.ActiveMq
{
    /// <summary>
    /// ActiveMqProvider
    /// </summary>
    internal sealed class ActiveMqProvider
    {
        /// <summary>
        /// 请求超时时间 单位秒
        /// </summary>
        internal static readonly int RequestTimeout = 3;

        /// <summary>
        /// 默认json序列化配置
        /// </summary>
        internal static readonly JsonSerializerSettings JsonDefaultSettings = new JsonSerializerSettings();

        /// <summary>
        /// 连接字符串
        /// </summary>
        internal static string ConnectionUrl { set; get; }

        /// <summary>
        /// 全局连接
        /// </summary>
        internal static IConnection Connection;

        /// <summary>
        /// _isConnected
        /// </summary>
        private static bool? _isConnected;

        /// <summary>
        /// 是否连接成功
        /// </summary>
        internal static bool IsConnected
        {
            get
            {
                while (!_isConnected.HasValue)
                    Thread.Sleep(1000);
                return _isConnected.Value;
            }
        }

        /// <summary>
        /// 生产者s
        /// </summary>
        internal static IDictionary<string, IMessageProducer> Producers = new Dictionary<string, IMessageProducer>();

        /// <summary>
        /// 消费者
        /// </summary>
        internal static readonly List<IMessageConsumer> _consumers = new List<IMessageConsumer>();

        /// <summary>
        /// CreateConsumerForWaitActions
        /// </summary>
        internal static readonly Dictionary<long, Action> CreateConsumerForWaitActions = new Dictionary<long, Action>();

        /// <summary>
        /// 当连接重新连上时
        /// </summary>
        internal static Action OnConnectionResumed;

        /// <summary>
        /// 创建连接
        /// </summary>
        internal static void CreateConnection()
        {
            IConnection connection = null;
            Task task = new Task(() =>
            {
                try
                {
                    _isConnected = false;
                    Debug.WriteLine("CreateConnection:ActiveMq.ConnectionUrl:{ConnectionUrl},正在与服务器连接....");
                    IConnectionFactory factory = new ConnectionFactory(ConnectionUrl);

                    connection = factory.CreateConnection();
                    //设置请求超时时间 2S
                    connection.RequestTimeout = new TimeSpan(0, 0, RequestTimeout);
                    connection.ConnectionInterruptedListener += () =>
                    {
                        _isConnected = false;
                        var log = $"ActiveMqProvider 与服务器连接已断开,{ConnectionUrl}";
                        Debug.WriteLine(log);
                    };
                    connection.ConnectionResumedListener += () =>
                    {
                        Connection = connection;
                        _isConnected = true;
                        var log = $"ActiveMqProvider 与服务器连接已重新连接,{ConnectionUrl}";
                        Debug.WriteLine(log);
                        //创建未成功监听的消费者
                        CreateConsumerForWaitActions.Values.ToList().ForEach(item =>
                    {
                        item.BeginInvoke(null, null);
                    });
                        CreateConsumerForWaitActions.Clear();
                        //执行重连成功后的事件
                        if (OnConnectionResumed != null)
                            OnConnectionResumed.BeginInvoke(null, null);
                        new Thread(() =>
                    {
                        Thread.Sleep(1000);
                        var session = Connection.CreateSession();
                        var activeMQ_DLQ = session.CreateProducer(session.GetQueue("ActiveMQ.DLQ"));
                        Trace.WriteLine(activeMQ_DLQ);
                        var brokerInfo = ((Connection)connection).BrokerInfo;
                        Trace.WriteLine(brokerInfo);
                        if (brokerInfo != null)
                        {
                            log = string.Format("BrokerInfo:{0}", JsonConvert.SerializeObject(new
                            {
                                brokerInfo.BrokerName,
                                brokerInfo.BrokerUploadUrl,
                                brokerInfo.BrokerURL
                            }));
                            Debug.WriteLine(log);
                        }
                    }).Start();
                    };

                    connection.Start();
                    Connection = connection;
                    _isConnected = true;
                    Debug.WriteLine($"CreateConnection:ActiveMq.ConnectionUrl:{ConnectionUrl},与服务器第一次连接成功!");
                }
                catch (Exception ex)
                {
                    _isConnected = false;

                    Debug.WriteLine(ex);
                }
            });

            task.Start();
            //task.Wait(RequestTimeout * 1000);
            if (_isConnected != true)
            {
                Debug.WriteLine("CreateConnection:ActiveMq.ConnectionUrl:{ConnectionUrl},{RequestTimeout}还未连接上服务器!");
            }
        }
    }
}