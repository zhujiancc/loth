using System;
using NLog;

namespace loth.rmq
{
    public class RmqLogHelper
    {
        private static readonly Logger logger4mq;

        static RmqLogHelper()
        {
            logger4mq = RmqLogManager.Instance.GetLogger("Rmq");
        }

        public static void Info(string info)
        {
            logger4mq.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" + info);
        }

        public static void Error(string error)
        {
            logger4mq.Error(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" + error);
        }

    }
}
