using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace loth.kafka
{
    public class KafkaAdminClient
    {
        private string kafkaServers;

        public KafkaAdminClient()
            : this(KafkaConfig.DefaultServers)
        {
        }

        public KafkaAdminClient(KafkaServerType kafkaServerType)
            : this(KafkaConfig.GetServers(kafkaServerType))
        {
        }

        public KafkaAdminClient(string kafkaServers)
        {
            this.kafkaServers = kafkaServers;
        }

        public void CreateTopic(string topicName, int numPartitions, int replicationFactor = 0)
        {
            CreateTopicAsync(topicName, numPartitions, replicationFactor).Wait();
        }

        public async Task CreateTopicAsync(string topicName, int numPartitions, int replicationFactor = 0)
        {
            AdminClientConfig config = new AdminClientConfig
            {
                BootstrapServers = kafkaServers
            };
            using (IAdminClient adminClient = new AdminClientBuilder(config).Build())
            {
                try
                {
                    if (replicationFactor == 0)
                    {
                        Metadata metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(20.0));
                        if (metadata.Brokers != null)
                        {
                            replicationFactor = metadata.Brokers.Count;
                        }
                    }

                    await adminClient.CreateTopicsAsync(new TopicSpecification[1]
                    {
                        new TopicSpecification
                        {
                            Name = topicName,
                            ReplicationFactor = (short)replicationFactor,
                            NumPartitions = numPartitions
                        }
                    });
                }
                catch (CreateTopicsException ex)
                {
                    Console.WriteLine("创建主题失败 " + ex.Results[0].Topic + ": " + ex.Results[0].Error.Reason);
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("创建主题失败 " + ex2.Message);
                }
            }
        }

        public void CreatePartitions(string topicName, int increaseTo)
        {
            CreatePartitionsAsync(topicName, increaseTo).Wait();
        }

        public async Task CreatePartitionsAsync(string topicName, int increaseTo)
        {
            AdminClientConfig config = new AdminClientConfig
            {
                BootstrapServers = kafkaServers
            };
            using (IAdminClient adminClient = new AdminClientBuilder(config).Build())
            {
                try
                {
                    await adminClient.CreatePartitionsAsync(new List<PartitionsSpecification>
                    {
                        new PartitionsSpecification
                        {
                            Topic = topicName,
                            IncreaseTo = increaseTo
                        }
                    });
                }
                catch (CreatePartitionsException ex)
                {
                    Console.WriteLine("创建分区失败 " + ex.Results[0].Topic + ": " + ex.Results[0].Error.Reason);
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("创建分区失败 " + ex2.Message);
                }
            }
        }

        public void DeleteTopic(string topicName)
        {
            DeleteTopicsAsync(new List<string>
            {
                topicName
            }).Wait();
        }

        public async Task DeleteTopicAsync(string topicName)
        {
            await DeleteTopicsAsync(new List<string>
            {
                topicName
            });
        }

        public void DeleteTopics(List<string> topics)
        {
            DeleteTopicsAsync(topics).Wait();
        }

        public async Task DeleteTopicsAsync(List<string> topics)
        {
            AdminClientConfig config = new AdminClientConfig
            {
                BootstrapServers = kafkaServers
            };
            using (IAdminClient adminClient = new AdminClientBuilder(config).Build())
            {
                try
                {
                    await adminClient.DeleteTopicsAsync(topics);
                }
                catch (DeleteTopicsException ex)
                {
                    Console.WriteLine("删除主题失败 " + ex.Results[0].Topic + ": " + ex.Results[0].Error.Reason);
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("删除主题失败 " + ex2.Message);
                }
            }
        }

        public int GetBrokerCount()
        {
            using (IAdminClient adminClient = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = kafkaServers
            }).Build())
            {
                try
                {
                    Metadata metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(20.0));
                    if (metadata.Brokers != null)
                    {
                        return metadata.Brokers.Count;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取实例个数失败 " + ex.Message);
                }
            }

            return 0;
        }

        public int GetPartitionCount(string topic)
        {
            using (IAdminClient adminClient = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = kafkaServers
            }).Build())
            {
                try
                {
                    Metadata metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(20.0));
                    if (metadata.Topics != null && metadata.Topics.Count >= 1)
                    {
                        return metadata.Topics[0].Partitions.Count;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取分区数失败，topic=" + topic + "，" + ex.Message);
                }
            }

            return 0;
        }

        public long GetMessageCount(string topic, string groupId = "", int partition = -1)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                Console.WriteLine("队列名称不可为空");
                return -1L;
            }

            int partitionCount = GetPartitionCount(topic);
            if (partitionCount == 0)
            {
                Console.WriteLine("该队列的分区数为0");
                return -2L;
            }

            long num = 0L;
            using (IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
            {
                GroupId = (string.IsNullOrWhiteSpace(groupId) ? "__default_group" : groupId),
                BootstrapServers = kafkaServers,
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnableAutoCommit = false,
                AllowAutoCreateTopics = false,
                EnablePartitionEof = true
            }).Build())
            {
                for (int i = 0; i < partitionCount; i++)
                {
                    if (partition < 0 || partition == i)
                    {
                        consumer.Assign(new TopicPartitionOffset(topic, i, Offset.Stored));
                        ConsumeResult<Ignore, string> consumeResult = consumer.Consume();
                        Offset offset = consumer.GetWatermarkOffsets(new TopicPartition(topic, i)).High;
                        if (offset < 0L)
                        {
                            offset = 0L;
                        }

                        long num2 = (long)offset - (long)consumeResult.Offset;
                        if (num2 < 0)
                        {
                            num2 = 0L;
                        }

                        num += (string.IsNullOrWhiteSpace(groupId) ? offset.Value : num2);
                    }
                }

                return num;
            }
        }
    }
}
