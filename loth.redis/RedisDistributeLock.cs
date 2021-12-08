using System;
using ServiceStack.Redis;
using ServiceStack.Redis.Support.Locking;

namespace loth.redis
{
    public class RedisDistributeLock
    {
        public long LockExpire { get; private set; }

        public RedisDistributeLock(string lockKey, RedisConfigEnum redisConfigEnum)
        {
            this._lockKey = "RedisDistributeLock:" + lockKey;
            this._distributedLock = new DistributedLock();
            this._redisservice = RedisServiceFactory.GetService(redisConfigEnum, 0);
        }

        public long Lock(int acquisitionTimeout, int lockTimeout)
        {
            long result = 0L;
            try
            {
                using (IRedisClient client = this._redisservice.GetClient())
                {
                    long lockExpire = 0L;
                    result = this._distributedLock.Lock(this._lockKey, acquisitionTimeout, lockTimeout, out lockExpire, client);
                    this.LockExpire = lockExpire;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public bool UnLock()
        {
            bool result = false;
            try
            {
                using (IRedisClient client = this._redisservice.GetClient())
                {
                    result = this._distributedLock.Unlock(this._lockKey, this.LockExpire, client);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        private readonly string _lockKey;

        private readonly DistributedLock _distributedLock;

        private RedisService _redisservice;
    }
}
