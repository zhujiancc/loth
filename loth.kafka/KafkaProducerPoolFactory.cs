using System;
using System.Collections.Generic;

namespace loth.kafka
{
	public class KafkaProducerPoolFactory
	{
		public static KafkaProducerPool GetClientPool(string connectKey, int poolSize = 10, int checkSleepSecs = 3)
		{
			if (!_poolDic.ContainsKey(connectKey))
			{
				lock (lock_obj)
				{
					if (!_poolDic.ContainsKey(connectKey))
					{
						_poolDic[connectKey] = new KafkaProducerPool(connectKey, poolSize, checkSleepSecs);
						_poolDic[connectKey].GetProducer();
					}
				}
			}
			return _poolDic[connectKey];
		}

		private static Dictionary<string, KafkaProducerPool> _poolDic = new Dictionary<string, KafkaProducerPool>();

		private static object lock_obj = new object();
	}
}
