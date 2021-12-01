using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace loth.rmq
{
    public class RmqPullMessageService
    {
        public static List<T> GetMessagesBulk<T>(int count, string queueName, RmqUrlEnum uri)
        {
            List<T> list = new List<T>();
            int num = 0;
            RmqConnect connect = RmqConnectPoolFactory.GetClientPool(uri, 5, 3).GetConnect();
            using (IModel channel = connect.GetChannel())
            {
                for (; ; )
                {
                    bool flag = num >= count;
                    if (flag)
                    {
                        break;
                    }
                    string value = string.Empty;
                    try
                    {
                        BasicGetResult basicGetResult = channel.BasicGet(queueName, true);
                        bool flag2 = basicGetResult == null;
                        if (flag2)
                        {
                            break;
                        }
                        value = Encoding.UTF8.GetString(basicGetResult.Body.ToArray());
                        T item = JsonConvert.DeserializeObject<T>(value);
                        list.Add(item);
                    }
                    catch (Exception ex)
                    {
                        RmqLogHelper.WriteError("拉取消息异常：" + ex.ToString());
                    }
                    num++;
                }
            }
            return list;
        }

        public static void Clear(RmqUrlEnum uri, string queueName)
        {
            RmqConnect connect = RmqConnectPoolFactory.GetClientPool(uri, 5, 3).GetConnect();
            IModel channel = connect.GetChannel();
            try
            {
                for (; ; )
                {
                    try
                    {
                        BasicGetResult basicGetResult = channel.BasicGet(queueName, true);
                    }
                    catch (Exception ex)
                    {
                        RmqLogHelper.WriteError("拉取消息异常：" + ex.ToString());
                    }
                }
            }
            finally
            {
                if (channel != null)
                {
                    channel.Dispose();
                    goto IL_52;
                }
                goto IL_52;
            IL_52:;
            }
        }

        public static void DeleteQueue(RmqUrlEnum uri, string queueName)
        {
            RmqConnect connect = RmqConnectPoolFactory.GetClientPool(uri, 5, 3).GetConnect();
            using (IModel channel = connect.GetChannel())
            {
                try
                {
                    uint num = channel.QueueDelete(queueName, true, false);
                    Console.WriteLine(queueName);
                }
                catch (Exception ex)
                {
                    RmqLogHelper.WriteError("删除队列异常：" + queueName);
                }
            }
        }
    }
}
