using Colipu.Common.ActiveMQ.ActiveMqEnum;

namespace Colipu.Common.ActiveMQ
{
    public class MqMessageModel<T>
    {
        public MqMessageModel()
        {
        }

        /// <summary>
        /// 优先级
        /// </summary>
        public MsgPriorityType Priority { set; get; }

        /// <summary>
        /// 延迟发送时间,ms
        /// </summary>
        public long? DelayConsume { set; get; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public T Body { set; get; }
    }
}