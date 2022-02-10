using System;
using System.Collections.Generic;

namespace loth.rmq
{
    public class RmqConnectPoolFactory
    {
        public static RmqConnectPool GetClientPool(string connect, int poolSize = 5, int checkSleepSecs = 3)
        {
            bool flag = !_poolDic.ContainsKey(connect);
            if (flag)
            {
                object obj = lock_obj;
                lock (obj)
                {
                    bool flag3 = !_poolDic.ContainsKey(connect);
                    if (flag3)
                    {
                        _poolDic[connect] = new RmqConnectPool(connect, poolSize, checkSleepSecs);
                        _poolDic[connect].GetConnect();
                    }
                }
            }
            return _poolDic[connect];
        }

        public static RmqConnectPool GetClientPool(RmqUrlEnum urlEnum, int poolSize = 5, int checkSleepSecs = 3)
        {
            string connect;
            switch (urlEnum)
            {
                case RmqUrlEnum.Douyin:
                    connect = "amqp://xigua:hI9hmLFOY4@innerdspmq.xiguaji.com:5672/Xigua";
                    break;
                case RmqUrlEnum.XiGuaWx:
                    connect = "amqp://xigua:hI9hmLFOY4@innermq.xiguaji.com:5672/Xigua";
                    break;
                case RmqUrlEnum.WeiBo:
                    connect = "amqp://xigua:Ckma43vwTQpVxAcE@innerwbmq.xiguaji.com:5672/Xigua";
                    break;
                case RmqUrlEnum.XiaoHongShu:
                    connect = "amqp://xigua:Ckma43vwTQpVxAcE@innerwbmq.xiguaji.com:5672/Xigua";
                    break;
                case RmqUrlEnum.KuaiShou:
                    connect = "amqp://xigua:f9LhMFdNjeWgWq6a@innerksmq.xiguaji.com:5672/Xigua";
                    break;
                case RmqUrlEnum.ZhiGua:
                    connect = "amqp://xigua:hI9hmLFOY4@innerdspmq.xiguaji.com:5672/Xigua";
                    break;
                case RmqUrlEnum.BStation:
                    connect = "amqp://xigua:Ckma43vwTQpVxAcE@innerwbmq.xiguaji.com:5672/Xigua";
                    break;
                case RmqUrlEnum.ZhiGua_New:
                    connect = "amqp://xigua:hI9hmLFOY4@zhiguamq.xiguaji.com:5672/Xigua";
                    break;
                case RmqUrlEnum.Spider:
                    connect = "amqp://admin:zhujian@192.168.10.84:8672/xhs";
                    break;
                default:
                    connect = "amqp://xigua:Ckma43vwTQpVxAcE@innerwbmq.xiguaji.com:5672/Xigua";
                    break;
            }
            return GetClientPool(connect, poolSize, checkSleepSecs);
        }

        private static Dictionary<string, RmqConnectPool> _poolDic = new Dictionary<string, RmqConnectPool>();

        private static object lock_obj = new object();
    }
}
