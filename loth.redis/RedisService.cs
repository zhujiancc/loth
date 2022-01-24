using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using ServiceStack.Redis.Pipeline;

namespace loth.redis
{
    public class RedisService
    {
        public RedisService(PooledRedisClientManager clientMan)
        {
            this._clientManager = clientMan;
        }

        public IRedisClient GetClient()
        {
            return this._clientManager.GetClient();
        }

        public long Increment(string key, uint amount)
        {
            long result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = client.Increment(key, amount);
            }
            return result;
        }

        public static TimeSpan GetTimeSpan(int timeout)
        {
            bool flag = timeout <= 0;
            if (flag)
            {
                timeout = RedisService.DefaultTimeOut;
            }
            return new TimeSpan((long)timeout * 10000000L);
        }

        public void SetExpire(string key, int timeout = 0)
        {
            using (IRedisClient client = this._clientManager.GetClient())
            {
                client.ExpireEntryIn(key, RedisService.GetTimeSpan(timeout));
            }
        }

        public bool Set<T>(string key, T t, int timeout = 0)
        {
            bool result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                bool flag = timeout == -1;
                if (flag)
                {
                    result = client.Set<T>(key, t);
                }
                else
                {
                    result = client.Set<T>(key, t, RedisService.GetTimeSpan(timeout));
                }
            }
            return result;
        }

        public T Get<T>(string key)
        {
            T result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = client.Get<T>(key);
            }
            return result;
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            IDictionary<string, T> all;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                all = client.GetAll<T>(keys);
            }
            return all;
        }

        public bool Remove(string key)
        {
            bool result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = client.Remove(key);
            }
            return result;
        }

        public void List_Add<T>(string key, T t)
        {
            using (IRedisClient client = this._clientManager.GetClient())
            {
                IRedisTypedClient<T> typedClient = client.As<T>();
                typedClient.AddItemToList(typedClient.Lists[key], t);
            }
        }

        public bool List_Remove<T>(string key, T t)
        {
            bool result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                IRedisTypedClient<T> typedClient = client.As<T>();
                result = (typedClient.RemoveItemFromList(typedClient.Lists[key], t) > 0);
            }
            return result;
        }

        public void List_RemoveAll<T>(string key)
        {
            using (IRedisClient client = this._clientManager.GetClient())
            {
                IRedisTypedClient<T> typedClient = client.As<T>();
                typedClient.Lists[key].RemoveAll();
            }
        }

        public long List_Count(string key)
        {
            long result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = (long)client.GetListCount(key);
            }
            return result;
        }

        public List<T> List_GetRange<T>(string key, int start, int count)
        {
            List<T> range;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                IRedisTypedClient<T> typedClient = client.As<T>();
                range = typedClient.Lists[key].GetRange(start, start + count - 1);
            }
            return range;
        }

        public List<T> List_GetList<T>(string key)
        {
            List<T> range;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                IRedisTypedClient<T> typedClient = client.As<T>();
                range = typedClient.Lists[key].GetRange(0, typedClient.Lists[key].Count);
            }
            return range;
        }

        public List<T> List_GetList<T>(string key, int pageIndex, int pageSize)
        {
            int start = pageSize * (pageIndex - 1);
            return this.List_GetRange<T>(key, start, pageSize);
        }

        public void PushItemToList<T>(string listId, T value)
        {
            using (IRedisClient client = this._clientManager.GetClient())
            {
                IRedisTypedClient<T> typedClient = client.As<T>();
                typedClient.PushItemToList(typedClient.Lists[listId], value);
            }
        }

        public T PopItemFromList<T>(string listId)
        {
            T result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                IRedisTypedClient<T> typedClient = client.As<T>();
                result = typedClient.PopItemFromList(typedClient.Lists[listId]);
            }
            return result;
        }

        public List<T> PopRangeFromList<T>(string listId, int num)
        {
            List<T> result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                IRedisTypedClient<T> typedClient = client.As<T>();
                IRedisList<T> redisList = typedClient.Lists[listId];
                List<T> range = redisList.GetRange(0, num - 1);
                bool flag = range.Any<T>();
                if (flag)
                {
                    typedClient.TrimList(redisList, range.Count, -1);
                }
                result = range;
            }
            return result;
        }

        public List<T> GetHastSetAll<T>(string key)
        {
            List<T> result2;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                List<T> result = new List<T>();
                List<string> hashValues = client.GetHashValues(key);
                bool flag = hashValues != null && hashValues.Count > 0;
                if (flag)
                {
                    hashValues.ForEach(delegate (string x)
                    {
                        T item = JsonConvert.DeserializeObject<T>(x);
                        result.Add(item);
                    });
                }
                result2 = result;
            }
            return result2;
        }

        public T Hs_Get<T>(string hashId, string key)
        {
            T result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                string valueFromHash = client.GetValueFromHash(hashId, key);
                bool flag = !string.IsNullOrEmpty(valueFromHash);
                if (flag)
                {
                    result = JsonConvert.DeserializeObject<T>(valueFromHash);
                }
                else
                {
                    result = default(T);
                }
            }
            return result;
        }

        public bool Hs_Exist<T>(string hashId, string key)
        {
            bool result = false;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = client.HashContainsEntry(hashId, key);
            }
            return result;
        }

        public bool Hs_Set<T>(string hashId, string key, T t)
        {
            bool result = false;
            try
            {
                using (IRedisClient client = this._clientManager.GetClient())
                {
                    string text = JsonConvert.SerializeObject(t);
                    result = client.SetEntryInHash(hashId, key, text);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public bool Hs_Remove(string hashId, string key)
        {
            bool result = false;
            try
            {
                using (IRedisClient client = this._clientManager.GetClient())
                {
                    result = client.RemoveEntryFromHash(hashId, key);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public long Hs_Increment(string hashId, string key, int incNum = 1)
        {
            long result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = (long)client.IncrementValueInHash(hashId, key, incNum);
            }
            return result;
        }

        public bool Hs_SetIfNotExists<T>(string hashId, string key, T t)
        {
            bool result = false;
            try
            {
                using (IRedisClient client = this._clientManager.GetClient())
                {
                    string text = JsonConvert.SerializeObject(t);
                    result = client.SetEntryInHashIfNotExists(hashId, key, text);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public bool SetIfNotExists(string key, string value, int timeout = 0)
        {
            bool result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                bool flag = client.SetValueIfNotExists(key, value);

                bool flag2 = flag && timeout >= 0;
                if (flag2)
                {
                    client.ExpireEntryIn(key, RedisService.GetTimeSpan(timeout));
                }
                result = flag;
            }
            return result;
        }

        public bool ContainsKey(string key)
        {
            bool result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = client.ContainsKey(key);
            }
            return result;
        }

        public List<string> SearchKeys(string pattern)
        {
            List<string> result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = client.SearchKeys(pattern);
            }
            return result;
        }

        public bool AddItemToSortedSet(string setId, string value, long score)
        {
            bool result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = client.AddItemToSortedSet(setId, value, (double)score);
            }
            return result;
        }

        public List<string> GetRangeFromSortedSetByLowestScore(string setId, long fromScore, long toScore)
        {
            List<string> rangeFromSortedSetByLowestScore;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                rangeFromSortedSetByLowestScore = client.GetRangeFromSortedSetByLowestScore(setId, fromScore, toScore);
            }
            return rangeFromSortedSetByLowestScore;
        }

        public void RemoveItemFromSortedSet(string setId, string itemValue)
        {
            using (IRedisClient client = this._clientManager.GetClient())
            {
                client.RemoveItemFromSortedSet(setId, itemValue);
            }
        }

        public long RemoveRangeFromSortedSet(string setId, int minRank, int maxRank)
        {
            long result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = client.RemoveRangeFromSortedSet(setId, minRank, maxRank);
            }
            return result;
        }

        public string PopItemWithLowestScoreFromSortedSet(string setId)
        {
            string result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = client.PopItemWithLowestScoreFromSortedSet(setId);
            }
            return result;
        }

        public long GetSortedSetCount(string setId)
        {
            long sortedSetCount;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                sortedSetCount = client.GetSortedSetCount(setId);
            }
            return sortedSetCount;
        }

        public double GetItemScoreInSortedSet(string setId, string value)
        {
            double itemScoreInSortedSet;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                itemScoreInSortedSet = client.GetItemScoreInSortedSet(setId, value);
            }
            return itemScoreInSortedSet;
        }

        public long Decrement(string key, uint amount)
        {
            long result;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                result = client.Decrement(key, amount);
            }
            return result;
        }

        public Dictionary<string, string> GetAllEntitiesFromHash(string hashId)
        {
            Dictionary<string, string> allEntriesFromHash;
            using (IRedisClient client = this._clientManager.GetClient())
            {
                allEntriesFromHash = client.GetAllEntriesFromHash(hashId);
            }
            return allEntriesFromHash;
        }

        public void SetRangeInHash(string hashId, Dictionary<string, string> keyValues)
        {
            using (IRedisClient client = this._clientManager.GetClient())
            {
                client.SetRangeInHash(hashId, keyValues);
            }
        }

        public void PipelineCommand(params Action<IRedisClient>[] actions)
        {
            using (IRedisClient client = this.GetClient())
            {
                IRedisPipeline redisPipeline = client.CreatePipeline();
                bool flag = actions.Any<Action<IRedisClient>>();
                if (flag)
                {
                    foreach (Action<IRedisClient> action in actions)
                    {
                        redisPipeline.QueueCommand(action);
                    }
                }
                redisPipeline.Flush();
            }
        }

        public T PipelineCommand<T>(Func<IRedisClient, T> afterActionsFunc, params Action<IRedisClient>[] actions)
        {
            T result;
            using (IRedisClient client = this.GetClient())
            {
                IRedisPipeline redisPipeline = client.CreatePipeline();
                bool flag = actions.Any<Action<IRedisClient>>();
                if (flag)
                {
                    foreach (Action<IRedisClient> action in actions)
                    {
                        redisPipeline.QueueCommand(action);
                    }
                }
                redisPipeline.Flush();
                result = afterActionsFunc(client);
            }
            return result;
        }

        private PooledRedisClientManager _clientManager;

        public static int DefaultTimeOut = 600;
    }
}
