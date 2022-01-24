using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace loth.kafka
{
	public abstract class KafkaBatchComsumerWorkerBase<T>
	{
		public KafkaBatchComsumerWorkerBase(string topic, string groupId, int batchSize = 100, int timeout = 30, int consumerCount = 0, AutoOffsetReset autoOffsetReset = AutoOffsetReset.Latest, int partition = -1, int pollIntervalms = 2500000, int sessiontimeout = 30000, bool autoCommit = true) : this(KafkaConfig.DefaultServers, topic, groupId, batchSize, timeout, consumerCount, autoOffsetReset, partition, pollIntervalms, sessiontimeout, autoCommit)
		{
		}

		public KafkaBatchComsumerWorkerBase(KafkaServerType kafkaServerType, string topic, string groupId, int batchSize = 100, int timeout = 30, int consumerCount = 0, AutoOffsetReset autoOffsetReset = AutoOffsetReset.Latest, int partition = -1, int pollIntervalms = 2500000, int sessiontimeout = 30000, bool autoCommit = true) : this(KafkaConfig.GetServers(kafkaServerType), topic, groupId, batchSize, timeout, consumerCount, autoOffsetReset, partition, pollIntervalms, sessiontimeout, autoCommit)
		{
		}

		public KafkaBatchComsumerWorkerBase(string kafkaServers, string topic, string groupId, int batchSize = 100, int timeout = 30, int consumerCount = 0, AutoOffsetReset autoOffsetReset = AutoOffsetReset.Latest, int partition = -1, int pollIntervalms = 2500000, int sessiontimeout = 30000, bool autoCommit = true)
		{
			this.KafkaServers = kafkaServers;
			this.Topic = topic;
			this.GroupId = groupId;
			this.BatchSize = batchSize;
			this.Timeout = timeout;
			this.ConsumerCount = consumerCount;
			this.AutoOffsetReset = autoOffsetReset;
			this.Partition = partition;
			this.AutoCommit = autoCommit;
		}

		protected string KafkaServers { get; set; }

		protected string Topic { get; set; }

		protected int ConsumerCount { get; set; }

		protected string GroupId { get; set; }

		protected int BatchSize { get; set; } = 100;

		protected int Timeout { get; set; } = 30;

		protected bool AutoCommit { get; set; } = true;

		protected AutoOffsetReset AutoOffsetReset { get; set; }

		protected int Partition { get; set; } = -1;

		public event EventHandler<ConsumeException> OnConsumeException;

		public void Start()
		{
			this.Start(Offset.Unset);
		}

		public void Start(Offset offset)
		{
			if (string.IsNullOrWhiteSpace(this.Topic))
			{
				Console.WriteLine("主题未指定");
				return;
			}
			if (string.IsNullOrWhiteSpace(this.GroupId))
			{
				Console.WriteLine("消费组未指定");
				return;
			}
			ConsumerConfig conf = new ConsumerConfig
			{
				GroupId = this.GroupId,
				BootstrapServers = this.KafkaServers,
				AutoOffsetReset = new AutoOffsetReset?(this.AutoOffsetReset),
				LogConnectionClose = new bool?(false),
				EnableAutoCommit = new bool?(this.AutoCommit),
				SessionTimeoutMs = new int?(this.SessionTimeoutMs)
			};
			if (this.Partition >= 0)
			{
				new Thread(delegate ()
				{
					try
					{
						using (IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(conf).Build())
						{
							if (offset != Offset.Unset)
							{
								consumer.Assign(new TopicPartitionOffset(this.Topic, this.Partition, offset));
							}
							else
							{
								consumer.Assign(new TopicPartitionOffset(this.Topic, this.Partition, Offset.Stored));
							}
							this.Consume(consumer, offset);
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}
				}).Start();
			}
			else
			{
				if (this.ConsumerCount == 0)
				{
					KafkaAdminClient kafkaAdminClient = new KafkaAdminClient(this.KafkaServers);
					this.ConsumerCount = kafkaAdminClient.GetPartitionCount(this.Topic);
				}
				if (this.ConsumerCount == 0)
				{
					this.ConsumerCount = 1;
				}
				for (int i = 0; i < this.ConsumerCount; i++)
				{
					Console.WriteLine(string.Format("当前第{0}个消费者开启", i));
					this.Run(offset, conf);
				}
			}
			Console.Read();
		}

		private void Run(Offset offset, ConsumerConfig conf)
		{
			new Thread(delegate ()
			{
				try
				{
					using (IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(conf).Build())
					{
						if (offset != Offset.Unset)
						{
							int partitionCount = new KafkaAdminClient(this.KafkaServers).GetPartitionCount(this.Topic);
							if (partitionCount == 0)
							{
								Console.WriteLine("分区数为0");
								return;
							}
							List<TopicPartitionOffset> list = new List<TopicPartitionOffset>();
							for (int i = 0; i < partitionCount; i++)
							{
								list.Add(new TopicPartitionOffset(this.Topic, i, offset));
							}
							consumer.Assign(list);
						}
						else
						{
							consumer.Subscribe(this.Topic);
						}
						this.Consume(consumer, offset);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}).Start();
		}

		public void Consume(IConsumer<Ignore, string> consumer, Offset offset)
		{
			List<string> list = new List<string>();
			DateTime t = DateTime.Now.AddSeconds((double)this.Timeout);
			for (; ; )
			{
				bool flag = false;
				try
				{
					ConsumeResult<Ignore, string> consumeResult = consumer.Consume(default(CancellationToken));
					list.Add(consumeResult.Message.Value);
					if (list.Count >= this.BatchSize || DateTime.Now > t)
					{
						Console.WriteLine("开始批处理：" + list.Count.ToString() + "条");
						flag = this.HandleComingMessageBase(list);
						list.Clear();
						t = DateTime.Now.AddSeconds((double)this.Timeout);
					}
				}
				catch (ConsumeException ex)
				{
					EventHandler<ConsumeException> onConsumeException = this.OnConsumeException;
					if (onConsumeException != null)
					{
						onConsumeException(this, ex);
					}
					Thread.Sleep(500);
					Console.WriteLine(ex.ToString());
				}
				finally
				{
					try
					{
						if (!this.AutoCommit && flag)
						{
							consumer.Commit();
						}
					}
					catch (Exception ex2)
					{
						Console.WriteLine(ex2.ToString());
					}
				}
			}
		}

		public virtual bool HandleComingMessageBase(List<string> data)
		{
			bool result;
			try
			{
				result = this.HandleComingMessage((from a in data
												   select JsonConvert.DeserializeObject<T>(a)).ToList<T>());
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
				result = false;
			}
			return result;
		}

		protected abstract bool HandleComingMessage(List<T> messageDTO);

		protected int SessionTimeoutMs = 30000;
	}
}
