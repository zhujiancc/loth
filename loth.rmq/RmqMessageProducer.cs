using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace loth.rmq
{
	public class RmqMessageProducer
	{
		public static void MessageSend<T>(RmqUrlEnum rmqUrlEnum, string exchange, string routeKey, T message, RmqMessageProps props = null)
		{
			RmqConnect connect = RmqConnectPoolFactory.GetClientPool(rmqUrlEnum, 5, 3).GetConnect();
			using (IModel channel = connect.GetChannel())
			{
				IBasicProperties basicProperties = channel.CreateBasicProperties();
				RmqMessageProducer.SetProps(basicProperties, props);
				string s = JsonConvert.SerializeObject(message);
				channel.BasicPublish(exchange, routeKey, false, basicProperties, Encoding.UTF8.GetBytes(s));
			}
		}

		private static void SetProps(IBasicProperties props, RmqMessageProps messageProps)
		{
			bool flag = messageProps == null;
			if (flag)
			{
				messageProps = new RmqMessageProps();
			}
			bool flag2 = !string.IsNullOrEmpty(messageProps.CorrelationId);
			if (flag2)
			{
				props.CorrelationId = messageProps.CorrelationId;
			}
			bool flag3 = !string.IsNullOrEmpty(messageProps.UserId);
			if (flag3)
			{
				props.UserId = messageProps.UserId;
			}
			bool flag4 = !string.IsNullOrEmpty(messageProps.MessageId);
			if (flag4)
			{
				props.MessageId = messageProps.MessageId;
			}
			bool flag5 = messageProps.Headers != null;
			if (flag5)
			{
				props.Headers = messageProps.Headers;
			}
			bool flag6 = messageProps.Expiration != null;
			if (flag6)
			{
				props.Expiration = messageProps.Expiration.ToString();
			}
			bool flag7 = messageProps.DeliveryMode != null;
			if (flag7)
			{
				props.DeliveryMode = Convert.ToByte(messageProps.DeliveryMode);
			}
			props.Priority = (byte)messageProps.Priority;
		}
	}
}
