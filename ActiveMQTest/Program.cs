using Colipu.Common.ActiveMQ;
using Colipu.Common.ActiveMQ.ActiveMqEnum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ActiveMQTest
{
    internal class Program
    {
        public readonly static IMqProvider<MessageModel> myProvider = MqFactory.CreateMQProvider<MessageModel>(QueueTopicType.P2P_推送商品);
        public readonly static Queue<int> Queue = new Queue<int>();
        public static bool IsRun = false;

        private static void Main(string[] args)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                TestProducer();
            });
            //启动生产这

            //启动消费
            Task t2 = Task.Factory.StartNew(() =>
            {
                TestConsumer();
            });

            Console.WriteLine(Queue.Count);
            Console.ReadKey();
        }

        /// <summary>
        /// 消费者
        /// </summary>
        public static void TestConsumer()
        {
            var index = 0;
            for (var i = 0; i < 3; i++)
            {
                var _i = i;
                Func<MessageModel, bool> receiveMessage = (message) =>
                {
                    //模拟回滚条件
                    var isRollback = index % 5 == 0;
                    index++;

                    #region 模拟处理消息

                    var log = $"接收到{_i}:{JsonConvert.SerializeObject(message)},\n,\t\t--IsRollback:{isRollback}";
                    Debug.Write(log);
                    Thread.Sleep(10);

                    #endregion 模拟处理消息

                    return isRollback;
                };
                myProvider.ListenerReceive(receiveMessage);
            }
        }

        public static void TestProducer()
        {
            var index = 0;
            while (true)
            {
                Console.WriteLine(index);
                try
                {
                    var model = new MessageModel
                    {
                        DateTime = DateTime.Now,
                        Index = index++,
                        Body = "于千万人之中遇见你所遇见你的于千万年之中时间的无涯的荒野里 房哥哥哥哥哥"
                    };
                    //正常消息入列
                    myProvider.Send(model);
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public class MessageModel
        {
            public int Index { set; get; }

            public DateTime DateTime { set; get; }
            public string Body { get; set; }
        }
    }
}