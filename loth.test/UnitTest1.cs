using loth.rmq;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;
using Xunit;

namespace loth.test
{
    public class UnitTest1
    {
        [Fact]
        public void BaseTest()
        {
            var urlEnum = RmqUrlEnum.XiaoHongShu;
            var exchange = "loth.xhs";
            var queueName = "loth.base";
            var routingKey = "loth.test";
            var pool = RmqConnectPoolFactory.GetClientPool(urlEnum);
            var connect = pool.GetConnect();
            using (var channel = connect.GetChannel())
            {
                channel.ExchangeDeclare(exchange, ExchangeType.Topic, true, false);
                channel.QueueDeclare(queueName, true,false,false);
                channel.QueueBind(queueName, exchange, routingKey);
            }
            
            var t1 = Task.Run(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    var props = new RmqMessageProps()
                    {
                        DeliveryMode = 1
                    };
                    RmqMessageProducer.MessageSend(RmqUrlEnum.XiaoHongShu, exchange, "loth.test", DateTime.Now.Ticks.ToString(), props);
                }
            });

            var consumer = new RmqQueueCommonOperate<string>(RmqUrlEnum.XiaoHongShu, exchange, queueName);
            consumer.StartConsumer(t =>
            {
                Console.WriteLine(t);
                return true;
            });


        }
    }
}