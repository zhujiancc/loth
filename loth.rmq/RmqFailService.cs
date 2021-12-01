using System;
using RabbitMQ.Client;

namespace loth.rmq
{
	public class RmqFailService
	{
		public string QueueName { get; set; }

		public RmqUrlEnum RmqUrl { get; set; }

		public RmqFailService(string queueName, RmqUrlEnum rmqUrl)
		{
			this.QueueName = queueName;
			this.RmqUrl = rmqUrl;
			try
			{
				RmqConnect connect = RmqConnectPoolFactory.GetClientPool(this.RmqUrl, 5, 3).GetConnect();
				using (IModel channel = connect.GetChannel())
				{
					channel.QueueDeclare(this.FailQueueName, true, false, false, null);
					channel.QueueBind(this.FailQueueName, RmqBasicInfoConfig.BaseExchange, this.FailQueueName, null);
				}
			}
			catch (Exception ex)
			{
				RmqLogHelper.WriteError("FailQueueInit 初始化错误队列异常：" + ex.ToString());
			}
		}

		public void FailSend<T>(T t)
		{
			try
			{
				RmqMessageProducer.MessageSend<T>(this.RmqUrl, RmqBasicInfoConfig.BaseExchange, this.FailQueueName, t, null);
			}
			catch (Exception ex)
			{
				RmqLogHelper.WriteError("FailSend处理错误消息异常：" + ex.ToString());
			}
		}

		protected string FailQueueName
		{
			get
			{
				return RmqBasicInfoConfig.BaseFailQueuePrefix + this.QueueName;
			}
		}
	}
}
