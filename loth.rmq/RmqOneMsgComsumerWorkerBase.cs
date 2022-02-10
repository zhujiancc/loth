using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace loth.rmq
{
    public abstract class RmqOneMsgComsumerWorkerBase<T>
    {
        protected RmqUrlEnum RmqUrl { get; set; }

        protected string QueueName { get; set; } = "";

        protected bool FailEnqueue { get; set; } = false;

        public void HandleOneMessage()
        {
            bool flag = true;
            T t = default(T);
            try
            {
                RmqConnect connect = RmqConnectPoolFactory.GetClientPool(this.RmqUrl, 5, 3).GetConnect();
                try
                {
                    BasicGetResult basicGetResult = null;
                    using (IModel channel = connect.GetChannel())
                    {
                        basicGetResult = channel.BasicGet(this.QueueName, true);
                    }
                    bool flag2 = basicGetResult != null;
                    if (flag2)
                    {
                        string @string = Encoding.UTF8.GetString(basicGetResult.Body.ToArray());
                        t = JsonConvert.DeserializeObject<T>(@string);
                        flag = this.HandleComingMessage(t);
                    }
                }
                catch (Exception ex)
                {
                    flag = false;
                    RmqLogHelper.Error("RmqOneMsgComsumerWorkerBase消费队列异常(内部)：" + ex.Message);
                }
                finally
                {
                    bool flag3 = this.FailEnqueue && !flag;
                    if (flag3)
                    {
                        RmqFailService rmqFailService = new RmqFailService(this.QueueName, this.RmqUrl);
                        rmqFailService.FailSend<T>(t);
                    }
                }
            }
            catch (Exception ex2)
            {
                RmqLogHelper.Error("RmqOneMsgComsumerWorkerBase消费队列异常：" + ex2.Message);
            }
        }

        protected abstract bool HandleComingMessage(T messageDTO);
    }
}
