using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Confluent.Kafka;

namespace loth.kafka
{
    public class KafkaProducerPool
    {
        public KafkaProducerPool(string uri, int poolSize, int checkSleepSecs = 3)
        {
            _poolSize = poolSize;
            _checkSleepSecs = checkSleepSecs;
            _readWriteLock = new ReaderWriterLockSlim();
            config = new ProducerConfig
            {
                BootstrapServers = uri
            };

            ThreadPool.QueueUserWorkItem(item =>
            {
                try
                {
                    ConnectCheck();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("数据库心跳检测异常：" + ex.ToString());
                }
            });

            try
            {
                KafkaAdminClient kafkaAdminClient = new KafkaAdminClient(uri);
                if (kafkaAdminClient.GetPartitionCount(TestConnect) <= 0)
                {
                    kafkaAdminClient.CreateTopic(TestConnect, 1, 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("基础监控连接Topic创建出错：" + ex.ToString());
            }
        }

        public KafkaProducer GetProducer()
        {
            _readWriteLock.EnterUpgradeableReadLock();
            KafkaProducer kafkaProducer = null;
            try
            {
                if (_cList.Count() < _poolSize)
                {
                    try
                    {
                        _readWriteLock.EnterWriteLock();
                        kafkaProducer = new KafkaProducer(new ProducerBuilder<int, string>(config).Build());
                        _cList.Add(kafkaProducer);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        _readWriteLock.ExitWriteLock();
                    }
                }
                return (from p in _cList
                        where p.IsOk
                        orderby Guid.NewGuid() descending
                        select p).FirstOrDefault();
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.ToString());
            }
            finally
            {
                _readWriteLock.ExitUpgradeableReadLock();
            }
            return kafkaProducer;
        }

        private void ConnectCheck()
        {
            while (true)
            {
                Thread.Sleep(1000 * _checkSleepSecs);
                _readWriteLock.EnterReadLock();
                List<KafkaProducer> list = new List<KafkaProducer>();
                try
                {
                    for (int i = 0; i < _cList.Count; i++)
                    {
                        KafkaProducer kafkaProducer = _cList[i];
                        try
                        {
                            kafkaProducer.Producer.Produce("_TestConnect_", new Message<int, string>
                            {
                                Key = 1,
                                Value = "1"
                            }, null);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("kafka connectcheck 异常，加入到异常连接池:" + ex.ToString());
                            try
                            {
                                kafkaProducer.IsOk = false;
                                kafkaProducer.Producer.Dispose();
                            }
                            catch (Exception)
                            {
                            }
                            list.Add(kafkaProducer);
                        }
                    }
                }
                finally
                {
                    _readWriteLock.ExitReadLock();
                }
                if (list.Count > 0)
                {
                    _readWriteLock.EnterWriteLock();
                    try
                    {
                        foreach (KafkaProducer item in list)
                        {
                            Console.WriteLine("kafka connectcheck 异常，移除链接:" + _cList.Remove(item).ToString());
                        }
                    }
                    finally
                    {
                        _readWriteLock.ExitWriteLock();
                    }
                }
            }
           
        }

        private List<KafkaProducer> _cList = new List<KafkaProducer>();

        private ProducerConfig config;

        private int _poolSize;

        private int _checkSleepSecs;

        private ReaderWriterLockSlim _readWriteLock;

        private const string TestConnect = "_TestConnect_";
    }
}
