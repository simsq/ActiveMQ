using Colipu.Common.ActiveMQ.ActiveMq;
using Colipu.Common.ActiveMQ.ActiveMqEnum;
using System;
using System.Configuration;
using System.Threading;

namespace Colipu.Common.ActiveMQ
{
    /// <summary>
    /// 创建工厂
    /// </summary>
    public static class MqFactory
    {
        private static readonly string _connectionUrl = ConfigurationManager.AppSettings["ActiveMqUrl"];

        /// <summary>
        /// 是否初始化完成
        /// </summary>
        private static bool? _isInit;

        static MqFactory()
        {
            Fire();
        }

        /// <summary>
        /// 开艹
        /// </summary>
        public static void Fire()
        {
            if (_isInit.HasValue)
                return;
            _isInit = false;
            ActiveMqProvider.ConnectionUrl = _connectionUrl;
            ActiveMqProvider.CreateConnection();
            _isInit = true;
        }

        /// <summary>
        /// 创建一个Mq提供者
        /// </summary>
        /// <typeparam name="T">消息队列中传输的数据模型,T必须为可序列化的模型</typeparam>
        /// <param name="queueTopicType">队列消息类型</param>
        /// <param name="timeToLive">消息存活时长,为空的话,消息不过期, 消息过期后将进入ActiveMQ.DLQ队列中</param>
        /// <returns></returns>
        public static IMqProvider<T> CreateMQProvider<T>(QueueTopicType queueTopicType, TimeSpan? timeToLive = null)
        {
            while (_isInit != true)
                Thread.Sleep(1000);
            return new MqProviderService<T>(queueTopicType, timeToLive);
        }
    }
}