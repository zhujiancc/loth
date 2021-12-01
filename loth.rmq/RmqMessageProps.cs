using System;
using System.Collections.Generic;

namespace loth.rmq
{
	public class RmqMessageProps
	{
		public int? DeliveryMode { get; set; } = new int?(1);

		public int? Expiration { get; set; }

		public Dictionary<string, object> Headers { get; set; }

		public RmqMessagePriorityLevel Priority { get; set; } = (RmqMessagePriorityLevel)0;

		public string MessageId { get; set; }

		public string CorrelationId { get; set; }

		public string UserId { get; set; }
	}
}
