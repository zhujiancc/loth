using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace loth.rmq
{
    public abstract class RmqComsumerWorkerBase<T>
    {
        protected int ConsumerCount
        {
            get;
            set;
        } = 1;


        protected RmqUrlEnum RmqUrl
        {
            get;
            set;
        }

        protected string QueueName
        {
            get;
            set;
        } = "";


        protected bool AutoAckOk
        {
            get;
            set;
        } = false;


        protected bool FailEnqueue
        {
            get;
            set;
        } = false;


        protected bool NeedLog
        {
            get;
            set;
        } = false;


        protected abstract Task<bool> HandleComingMessage(T messageDTO);

        public void Start()
        {
            int runningCount = 0;
            RmqFailService fs = null;
            if (FailEnqueue)
            {
                fs = new RmqFailService(QueueName, RmqUrl);
            }

            IModel channel;
            while (true)
            {
                if (runningCount < ConsumerCount)
                {
                    try
                    {
                        RmqConnect connect = RmqConnectPoolFactory.GetClientPool(RmqUrl).GetConnect();
                        channel = connect.GetChannel();
                        channel.BasicQos(0u, 1, global: false);
                        EventingBasicConsumer eventingBasicConsumer = new EventingBasicConsumer(channel);
                        eventingBasicConsumer.Shutdown += delegate
                        {
                            Thread.Sleep(3000);
                            if (NeedLog)
                            {
                                RmqLogHelper.Error("rmq 网络连接断开了，重连操作");
                            }

                            Interlocked.Decrement(ref runningCount);
                        };
                        eventingBasicConsumer.Received += async (object ch, BasicDeliverEventArgs ea) =>
                        {
                            if (NeedLog && new Random(30).Next() == 10)
                            {
                                RmqLogHelper.Info("rmq 消费中");
                            }

                            bool flag = true;
                            T val = default(T);
                            try
                            {
                                byte[] bytes = ea.Body.ToArray();
                                val = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
                                flag =await HandleComingMessage(val);
                            }
                            catch (Exception ex2)
                            {
                                RmqLogHelper.Error("rmq 接收消息异常：" + ex2.ToString());
                                flag = false;
                            }
                            finally
                            {
                                if (!AutoAckOk)
                                {
                                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                                }

                                if (!flag && FailEnqueue)
                                {
                                    fs.FailSend(val);
                                }
                            }
                        };
                        channel.BasicConsume(QueueName, AutoAckOk, eventingBasicConsumer);
                        Interlocked.Increment(ref runningCount);
                    }
                    catch (Exception ex)
                    {
                        RmqLogHelper.Error("rmq  RmqComsumerWorkerBase消费异常：" + ex.ToString());
                    }
                }

                Thread.Sleep(500);
            }
        }
    }
}