using System;
using System.Collections.Generic;
using ServiceStack.Redis;

namespace loth.redis
{
	public class RedisPoolService
	{
		public static PooledRedisClientManager GetClientMan(string connect_read, string connect_write, int db = 0, int readPoolSize = 200, int writePoolSize = 200)
		{
			RedisConnectConfig redisConnectConfig = new RedisConnectConfig(connect_read, connect_write, db);
			bool flag = !RedisPoolService._poolDic.ContainsKey(redisConnectConfig.ConnectKey);
			if (flag)
			{
				object obj = RedisPoolService.lock_obj;
				lock (obj)
				{
					bool flag3 = !RedisPoolService._poolDic.ContainsKey(redisConnectConfig.ConnectKey);
					if (flag3)
					{
						PooledRedisClientManager pooledRedisClientManager = new PooledRedisClientManager(redisConnectConfig.Connect_Write.Split(new char[]
						{
							';'
						}), redisConnectConfig.Connect_Read.Split(new char[]
						{
							';'
						}), new RedisClientManagerConfig
						{
							AutoStart = true,
							DefaultDb = new int?(db),
							MaxReadPoolSize = readPoolSize,
							MaxWritePoolSize = writePoolSize
						});
						pooledRedisClientManager.ConnectTimeout = new int?(10000);
						pooledRedisClientManager.PoolTimeout = new int?(10000);
						RedisPoolService._poolDic[redisConnectConfig.ConnectKey] = pooledRedisClientManager;
					}
				}
			}
			return RedisPoolService._poolDic[redisConnectConfig.ConnectKey];
		}

		private static Dictionary<string, PooledRedisClientManager> _poolDic = new Dictionary<string, PooledRedisClientManager>();

		private static object lock_obj = new object();
	}
}
