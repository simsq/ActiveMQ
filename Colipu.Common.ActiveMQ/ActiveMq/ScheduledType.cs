using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipu.Common.ActiveMQ.ActiveMq
{
    public static class ScheduledType
    {
        /// <summary>
        /// The time in milliseconds that a message will wait before being scheduled to be delivered by the broker,
        ///  延迟消费
        /// type:long
        /// </summary>
        public readonly static string AMQ_SCHEDULED_DELAY = "AMQ_SCHEDULED_DELAY";

        /// <summary>
        /// The time in milliseconds to wait after the start time to wait before scheduling the message again
        /// type:long
        /// </summary>
        public readonly static string AMQ_SCHEDULED_PERIOD = "AMQ_SCHEDULED_PERIOD";

        /// <summary>
        /// The number of times to repeat scheduling a message for delivery
        /// type:int
        /// </summary>
        public readonly static string AMQ_SCHEDULED_REPEAT = "AMQ_SCHEDULED_REPEAT";

        /// <summary>
        /// Use a Cron entry to set the schedule, 表达式
        /// type:string
        /// message.setStringProperty(ScheduledMessage.AMQ_SCHEDULED_CRON, "0 * * * *");
        /// </summary>
        public readonly static string AMQ_SCHEDULED_CRON = "AMQ_SCHEDULED_CRON";
    }
}