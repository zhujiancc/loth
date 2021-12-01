using System;
using Xunit;

namespace loth.rmq.test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var props = new RmqMessageProps()
            {
                DeliveryMode = 1
            };
            RmqMessageProducer.MessageSend(RmqUrlEnum.WeiBo, "Xigua.Weibo", "xiguatest", DateTime.Now.Ticks.ToString(), props);
        }
    }
}