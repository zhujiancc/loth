using System;
using Confluent.Kafka;

namespace loth.kafka
{
	public class KafkaComsumerManager
	{
		public static IConsumer<Ignore, string> GetConsumer(string topic, string groupId, bool autoCommit = false, AutoOffsetReset autoOffsetReset = AutoOffsetReset.Latest, KafkaServerType serverType = KafkaServerType.Douyin)
		{
			IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
			{
				GroupId = groupId,
				BootstrapServers = KafkaConfig.GetServers(serverType),
				AutoOffsetReset = new AutoOffsetReset?(autoOffsetReset),
				EnableAutoCommit = new bool?(autoCommit),
				LogConnectionClose = new bool?(false)
			}).Build();
			consumer.Subscribe(topic);
			return consumer;
		}
	}
}
