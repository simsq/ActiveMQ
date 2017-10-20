using Colipu.Common.ActiveMQ.ActiveMqEnum;
using System;

namespace Colipu.Common.ActiveMQ
{
    /// <summary>
    /// 消息队列提供者接口
    /// </summary>
    /// <typeparam name="T">要序列化的模型</typeparam>
    public interface IMqProvider<T>
    {
        #region 属 性

        /// <summary>
        /// 连接字符串
        /// </summary>
        string ConnectionUrl { get; }

        /// <summary>
        /// 队列类型
        /// </summary>
        QueueTopicType QueueTopicType { get; }

        #endregion 属 性

        #region 方法

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="body">要发送的数据</param>
        void Send(T body);

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="body">要发送的数据</param>
        /// <param name="priority">优先级,默认为正常</param>
        void Send(T body, MsgPriorityType priority);

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="body">要发送的数据</param>
        /// <param name="priority">优先级,默认为正常</param>
        /// <param name="delayConsume">延迟消费时间,单位毫秒,默认为null</param>
        void Send(T body, MsgPriorityType priority, long delayConsume);

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="body">要发送的数据</param>
        /// <param name="delayConsume">延迟消费时间,单位毫秒,默认为null</param>
        void Send(T body, long delayConsume);

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="body">要发送的数据</param>
        /// <param name="priority">优先级,默认为正常</param>
        /// <param name="delayConsume">延迟消费时间,单位毫秒,默认为null</param>
        void Send(T body, MsgPriorityType priority = MsgPriorityType.Normal, long? delayConsume = null);

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <param name="message">mq消息实体</param>
        void Send(MqMessageModel<T> message);

        /// <summary>
        /// 监听接收消息,
        /// 当监听方法返回true时该条消息将被消费成功,
        /// 返回false 或者调用时出现异常该条消息将被回滚;
        /// 简单来说该监听处理是在一个消息事务来完成的.
        /// 如果该方法被调用多次,就会产生多个消费者,类似于多开线程进行监听
        /// </summary>
        /// <param name="listenerFunc">监听方法</param>
        void ListenerReceive(Func<T, bool> listenerFunc);

        #endregion 方法
    }
}