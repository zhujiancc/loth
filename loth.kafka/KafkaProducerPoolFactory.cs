using System;
using System.Collections.Generic;

namespace loth.kafka
{
	public class KafkaProducerPoolFactory
	{
		public static KafkaProducerPool GetClientPool(string connectKey, int poolSize = 10, int checkSleepSecs = 3)
		{
			if (!KafkaProducerPoolFactory._poolDic.ContainsKey(connectKey))
			{
				object obj = KafkaProducerPoolFactory.lock_obj;
				lock (obj)
				{
					if (!KafkaProducerPoolFactory._poolDic.ContainsKey(connectKey))
					{
						KafkaProducerPoolFactory._poolDic[connectKey] = new KafkaProducerPool(connectKey, poolSize, checkSleepSecs);
						KafkaProducerPoolFactory._poolDic[connectKey].GetProducer();
					}
				}
			}
			return KafkaProducerPoolFactory._poolDic[connectKey];
		}

		private static Dictionary<string, KafkaProducerPool> _poolDic = new Dictionary<string, KafkaProducerPool>();

		private static object lock_obj = new object();
	}
}
