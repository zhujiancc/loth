using System;

namespace loth.redis
{
	public class RedisServiceFactory
	{
		public static RedisService GetService(RedisConfigEnum configEnum, RedisProjectDB db)
		{
			return RedisServiceFactory.GetService(configEnum, (int)db);
		}

		public static RedisService GetService(RedisConfigEnum configEnum, int db)
		{
			string text = string.Empty;
			switch (configEnum)
			{
				case RedisConfigEnum.产品web:
					text = "VlifunRedis2016@innercache_redis.xiguaji.com:6379";
					return new RedisService(RedisPoolService.GetClientMan(text, text, db, 200, 200));
				case RedisConfigEnum.采集专用:
					text = "VlifunRedis2016@tokeninnercache_redis.xiguaji.com:6379";
					return new RedisService(RedisPoolService.GetClientMan(text, text, db, 200, 200));
				case RedisConfigEnum.基础服务:
					text = "VlifunRedis2016@innercache_basicredis.xiguaji.com:6379";
					return new RedisService(RedisPoolService.GetClientMan(text, text, db, 200, 200));
				case RedisConfigEnum.抖音web:
					text = "VlifunRedis2016@innercache_dyredis.xiguaji.com:6379";
					return new RedisService(RedisPoolService.GetClientMan(text, text, db, 200, 200));
				case RedisConfigEnum.Tiktok:
					text = "redisoversea.7sd4fp.ng.0001.use2.cache.amazonaws.com:6379";
					return new RedisService(RedisPoolService.GetClientMan(text, text, db, 200, 200));
			}
			return null;
		}

		public static RedisService GetServiceByUdf(string writeUrl, string readUrlList, int db)
		{
			return new RedisService(RedisPoolService.GetClientMan(writeUrl, readUrlList, db, 200, 200));
		}
	}
}
