using System;
using NLog;

namespace loth.rmq
{
	public class RmqLogHelper
	{
		static RmqLogHelper()
		{
			RmqLogHelper.logger4mq = RmqLogManager.Instance.GetLogger("Rmq");
		}

		public static void WriteInfo(string info)
		{
			RmqLogHelper.logger4mq.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" + info);
		}

		public static void WriteError(string error)
		{
			RmqLogHelper.logger4mq.Error(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" + error);
		}

		private static Logger logger4mq = null;
	}
}
