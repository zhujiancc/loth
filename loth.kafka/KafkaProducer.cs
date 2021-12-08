using System;
using Confluent.Kafka;

namespace loth.kafka
{
	public class KafkaProducer
	{
		public KafkaProducer(IProducer<int, string> producer)
		{
			this._producer = producer;
		}

		public IProducer<int, string> Producer
		{
			get
			{
				return this._producer;
			}
		}

		private IProducer<int, string> _producer;

		public bool IsOk = true;
	}
}
