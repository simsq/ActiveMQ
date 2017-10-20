using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipu.Common.ActiveMQ.ActiveMqEnum
{
    /// <summary>
    /// 消息的优先级
    /// </summary>
    public enum MsgPriorityType
    {
        /// <summary>
        /// 最低
        /// </summary>
        Lowest = 0,

        /// <summary>
        /// 极低
        /// </summary>
        VeryLow = 1,

        /// <summary>
        /// 低
        /// </summary>
        Low = 2,

        /// <summary>
        /// 高于低
        /// </summary>
        AboveLow = 3,

        /// <summary>
        /// 正常以下
        /// </summary>
        BelowNormal = 4,

        /// <summary>
        /// 正常
        /// </summary>
        Normal = 5,

        /// <summary>
        /// 高于正常
        /// </summary>
        AboveNormal = 6,

        /// <summary>
        /// 高
        /// </summary>
        High = 7,

        /// <summary>
        /// 极高
        /// </summary>
        VeryHigh = 8,

        /// <summary>
        /// 最高
        /// </summary>
        Highest = 9
    }
}