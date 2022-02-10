using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace loth.rmq
{
    public abstract class RmqBatchComsumerMultThreadWorkBase<T> : RmqBatchComsumerWorkBase<T>
    {
        public int ThreadCount
        {
            get;
            set;
        } = 1;


        protected abstract bool HandleOneMessage(T messageDTO);

        protected override bool HandleComingMessage(List<T> messages)
        {
            try
            {
                Parallel.ForEach(messages, new ParallelOptions
                {
                    MaxDegreeOfParallelism = ThreadCount
                }, delegate (T p)
                {
                    try
                    {
                        HandleOneMessage(p);
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine(ex2.ToString());
                    }
                });
                return true;
            }
            catch (Exception ex)
            {
                RmqLogHelper.Error("InternalHandleComingMessage 消费队列异常：" + ex.Message);
                return false;
            }
        }
    }
}