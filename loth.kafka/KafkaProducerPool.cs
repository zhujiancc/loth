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
			this._connect = uri;
			this._poolSize = poolSize;
			this._checkSleepSecs = checkSleepSecs;
			this._rd = new Random();
			this._readWriteLock = new ReaderWriterLockSlim();
			this.config = new ProducerConfig
			{
				BootstrapServers = uri
			};
			new Thread(delegate ()
			{
				try
				{
					this.ConnectCheck();
				}
				catch (Exception ex2)
				{
					Console.WriteLine("数据库心跳检测异常：" + ex2.ToString());
				}
			}).Start();
			try
			{
				KafkaAdminClient kafkaAdminClient = new KafkaAdminClient(uri);
				if (kafkaAdminClient.GetPartitionCount("_TestConnect_") <= 0)
				{
					kafkaAdminClient.CreateTopic("_TestConnect_", 1, 1);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("基础监控连接Topic创建出错：" + ex.ToString());
			}
		}

		public KafkaProducer GetProducer()
		{
			this._readWriteLock.EnterUpgradeableReadLock();
			KafkaProducer kafkaProducer = null;
			try
			{
				if (this._cList.Count<KafkaProducer>() < this._poolSize)
				{
					try
					{
						this._readWriteLock.EnterWriteLock();
						kafkaProducer = new KafkaProducer(new ProducerBuilder<int, string>(this.config).Build());
						this._cList.Add(kafkaProducer);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}
					finally
					{
						this._readWriteLock.ExitWriteLock();
					}
				}
				return (from p in this._cList
						where p.IsOk
						orderby Guid.NewGuid() descending
						select p).FirstOrDefault<KafkaProducer>();
			}
			catch (Exception ex2)
			{
				Console.WriteLine(ex2.ToString());
			}
			finally
			{
				this._readWriteLock.ExitUpgradeableReadLock();
			}
			return kafkaProducer;
		}

		private void ConnectCheck()
		{
			for (; ; )
			{
				Thread.Sleep(1000 * this._checkSleepSecs);
				this._readWriteLock.EnterReadLock();
				List<KafkaProducer> list = new List<KafkaProducer>();
				try
				{
					for (int i = 0; i < this._cList.Count; i++)
					{
						KafkaProducer kafkaProducer = this._cList[i];
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
					this._readWriteLock.ExitReadLock();
				}
				if (list.Count > 0)
				{
					this._readWriteLock.EnterWriteLock();
					try
					{
						foreach (KafkaProducer item in list)
						{
							Console.WriteLine("kafka connectcheck 异常，移除链接:" + this._cList.Remove(item).ToString());
						}
					}
					finally
					{
						this._readWriteLock.ExitWriteLock();
					}
				}
			}
		}

		private List<KafkaProducer> _cList = new List<KafkaProducer>();

		private ProducerConfig config;

		private object _lock = new object();

		private string _connect;

		private int _poolSize;

		private int _checkSleepSecs;

		private Random _rd;

		private ReaderWriterLockSlim _readWriteLock;

		private const string TestConnect = "_TestConnect_";
	}
}
