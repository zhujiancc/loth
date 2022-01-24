using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace loth.kafka
{
    public class KafkaProducerService
    {
        private KafkaServerType serverType;

        private static IsoDateTimeConverter timeFormat = new IsoDateTimeConverter
        {
            DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
        };

        private static Random random = new Random((int)(DateTime.Now.Ticks % 100000));

        private string KafkaServers
        {
            get;
            set;
        }

        public event EventHandler<ProduceException<Null, string>> OnProduceException;

        public KafkaProducerService()
            : this(KafkaConfig.DefaultServers)
        {
        }

        public KafkaProducerService(KafkaServerType kafkaServerType)
            : this(KafkaConfig.GetServers(kafkaServerType))
        {
            serverType = kafkaServerType;
        }

        public KafkaProducerService(string kafkaServers)
        {
            KafkaServers = kafkaServers;
        }

        public void Produce<T>(string topic, T messageDto, int partition = -1) where T : class
        {
            if (messageDto != null && !string.IsNullOrWhiteSpace(topic))
            {
                new ProducerConfig().BootstrapServers = KafkaServers;
                try
                {
                    KafkaProducerPoolFactory.GetClientPool(KafkaServers).GetProducer().Producer.Produce(new TopicPartition(topic, partition), new Message<int, string>
                    {
                        Key = Math.Abs(Guid.NewGuid().GetHashCode()),
                        Value = JsonConvert.SerializeObject(messageDto, timeFormat)
                    });
                }
                catch (ProduceException<Null, string> e)
                {
                    this.OnProduceException?.Invoke(this, e);
                }
            }
        }

        public async Task<TopicPartitionOffset> ProduceAsync<T>(string topic, T messageDto, int partition = -1) where T : class
        {
            if (messageDto == null || string.IsNullOrWhiteSpace(topic))
            {
                return new TopicPartitionOffset(topic, -1, -1L);
            }

            new ProducerConfig().BootstrapServers = KafkaServers;
            try
            {
                return (await KafkaProducerPoolFactory.GetClientPool(KafkaServers).GetProducer().Producer.ProduceAsync(new TopicPartition(topic, partition), new Message<int, string>
                {
                    Key = Math.Abs(Guid.NewGuid().GetHashCode()),
                    Value = JsonConvert.SerializeObject(messageDto, timeFormat)
                })).TopicPartitionOffset;
            }
            catch (ProduceException<Null, string> e)
            {
                this.OnProduceException?.Invoke(this, e);
                return new TopicPartitionOffset(topic, -1, -1L);
            }
        }
    }
}
