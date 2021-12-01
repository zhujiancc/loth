using System;
using System.Collections.Generic;

namespace loth.rmq
{
	public class RmqConnectPoolFactory
	{
		public static RmqConnectPool GetClientPool(string connect, int poolSize = 5, int checkSleepSecs = 3)
		{
			bool flag = !RmqConnectPoolFactory._poolDic.ContainsKey(connect);
			if (flag)
			{
				object obj = RmqConnectPoolFactory.lock_obj;
				lock (obj)
				{
					bool flag3 = !RmqConnectPoolFactory._poolDic.ContainsKey(connect);
					if (flag3)
					{
						RmqConnectPoolFactory._poolDic[connect] = new RmqConnectPool(connect, poolSize, checkSleepSecs);
						RmqConnectPoolFactory._poolDic[connect].GetConnect();
					}
				}
			}
			return RmqConnectPoolFactory._poolDic[connect];
		}

		public static RmqConnectPool GetClientPool(RmqUrlEnum urlEnum, int poolSize = 5, int checkSleepSecs = 3)
		{
			string connect = string.Empty;
			switch (urlEnum)
			{
				case RmqUrlEnum.Douyin:
					connect = "amqp://xigua:hI9hmLFOY4@innerdspmq.xiguaji.com:5672/Xigua";
					goto IL_81;
				case RmqUrlEnum.XiGuaWx:
					connect = "amqp://xigua:hI9hmLFOY4@innermq.xiguaji.com:5672/Xigua";
					goto IL_81;
				case RmqUrlEnum.WeiBo:
					connect = "amqp://xigua:Ckma43vwTQpVxAcE@innerwbmq.xiguaji.com:5672/Xigua";
					goto IL_81;
				case RmqUrlEnum.XiaoHongShu:
					connect = "amqp://xigua:Ckma43vwTQpVxAcE@innerwbmq.xiguaji.com:5672/Xigua";
					goto IL_81;
				case RmqUrlEnum.KuaiShou:
					connect = "amqp://xigua:f9LhMFdNjeWgWq6a@innerksmq.xiguaji.com:5672/Xigua";
					goto IL_81;
				case RmqUrlEnum.ZhiGua:
					connect = "amqp://xigua:hI9hmLFOY4@innerdspmq.xiguaji.com:5672/Xigua";
					goto IL_81;
				case RmqUrlEnum.BStation:
					connect = "amqp://xigua:Ckma43vwTQpVxAcE@innerwbmq.xiguaji.com:5672/Xigua";
					goto IL_81;
				case RmqUrlEnum.ZhiGua_New:
					connect = "amqp://xigua:hI9hmLFOY4@zhiguamq.xiguaji.com:5672/Xigua";
					goto IL_81;
			}
			connect = "amqp://xigua:Ckma43vwTQpVxAcE@innerwbmq.xiguaji.com:5672/Xigua";
		IL_81:
			return RmqConnectPoolFactory.GetClientPool(connect, poolSize, checkSleepSecs);
		}

		private static Dictionary<string, RmqConnectPool> _poolDic = new Dictionary<string, RmqConnectPool>();

		private static object lock_obj = new object();
	}
}
