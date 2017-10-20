namespace Colipu.Common.ActiveMQ.ActiveMqEnum
{
    /// <summary>
    /// 注意：类型必须以P2P或者P2M开头；
    /// 消息主题（队列类型）
    /// </summary>
    public enum QueueTopicType
    {
        /// <summary>
        /// 队列类型消息 添加商品
        /// </summary>
        P2P_推送商品 = 10000,

        /// <summary>
        /// 队列类型消息添加
        /// </summary>
        P2P_推送价格 = 10001,

        /// <summary>
        /// 队列类型消息添加
        /// </summary>
        P2M_推送商品 = 10002,
    }
}