using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace loth.rmq
{
    public abstract class RmqBatchComsumerWorkBase<T>
    {
        protected string QueueName 
        {
            get;
            set;
        }

        protected int BatchSize
        {
            get;
            set;
        } = 100;


        public TimeSpan Interval
        {
            get;
            set;
        } = TimeSpan.FromMinutes(3.0);


        protected RmqUrlEnum RmqUrl
        {
            get;
            set;
        }

        public void Run()
        {
            while (true)
            {
                Start();
                Console.WriteLine("运行了一轮数据!");
            }
        }

        public void Start()
        {
            List<T> list = new List<T>();
            DateTime intervalTime = DateTime.Now.Add(Interval);
            try
            {
                RmqConnect connect = RmqConnectPoolFactory.GetClientPool(RmqUrl).GetConnect();
                using (IModel model = connect.GetChannel())
                {
                    while (list.Count < BatchSize && !(DateTime.Now > intervalTime))
                    {
                        BasicGetResult basicGetResult = model.BasicGet(QueueName, autoAck: true);
                        if (basicGetResult == null)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        ulong deliveryTag = basicGetResult.DeliveryTag;
                        string @string = Encoding.UTF8.GetString(basicGetResult.Body.ToArray());
                        T item = JsonConvert.DeserializeObject<T>(@string);
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                RmqLogHelper.Error("RmqBatchComsumerWorkBase 异常：" + ex.ToString());
            }

            if (list.Count > 0)
            {
                InternalHandleComingMessage(list);
            }
        }

        private bool InternalHandleComingMessage(List<T> messageDTO)
        {
            try
            {
                return HandleComingMessage(messageDTO);
            }
            catch (Exception ex)
            {
                RmqLogHelper.Error("InternalHandleComingMessage 消费队列异常：" + ex.Message);
                return false;
            }
        }

        protected abstract bool HandleComingMessage(List<T> messageDTO);
    }
}