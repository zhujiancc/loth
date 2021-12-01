using System;

namespace loth.rmq
{
	public class RmqQueueCommonOperate<T> where T : class
	{
		private RmqMessageProducer _service { get; set; }

		private string _queueName { get; set; }

		private RmqHttpApiService _rmqHttpApi { get; set; }

		private string _exchange { get; set; }

		private RmqMessageProducer _producer { get; set; }

		public RmqQueueCommonOperate(RmqUrlEnum urlEnum, string exchange, string queueName)
		{
			this._rmqUrlEnum = urlEnum;
			this._queueName = queueName;
			this._exchange = exchange;
			this._rmqHttpApi = new RmqHttpApiService(urlEnum, 15355);
		}

		public void Enqueue(T t, string routeKey = "", RmqMessagePriorityLevel priorityLevel = RmqMessagePriorityLevel.Normal, int deliveryMode = 1)
		{
			bool flag = string.IsNullOrEmpty(routeKey);
			if (flag)
			{
				routeKey = this._exchange + "." + this._queueName;
			}
			RmqMessageProducer.MessageSend<T>(this._rmqUrlEnum, this._exchange, routeKey, t, new RmqMessageProps
			{
				Priority = priorityLevel,
				DeliveryMode = new int?(deliveryMode)
			});
		}

		public int GetCount()
		{
			return this._rmqHttpApi.GetMessageCount(this._queueName);
		}

		public void StartConsumer(RmqQueueCommonOperate<T>.ConsumerWorker worker, int threadCount = 1)
		{
			RmqQueueCommonOperate<T>.Consumer consumer = new RmqQueueCommonOperate<T>.Consumer(this._rmqUrlEnum, this._queueName, worker, threadCount, false, false);
			consumer.Start();
		}

		private RmqUrlEnum _rmqUrlEnum;

		public delegate bool ConsumerWorker(T t);

		private class Consumer : RmqComsumerWorkerBase<T>
		{
			public Consumer(RmqUrlEnum urlEnum, string queueName, RmqQueueCommonOperate<T>.ConsumerWorker worker, int threadCount = 1, bool autoAckOk = false, bool failEnqueue = false)
			{
				base.QueueName = queueName;
				base.ConsumerCount = threadCount;
				this.Worker = worker;
				base.AutoAckOk = autoAckOk;
				base.RmqUrl = urlEnum;
				base.FailEnqueue = failEnqueue;
			}

			protected override bool HandleComingMessage(T messageDto)
			{
				try
				{
					return this.Worker(messageDto);
				}
				catch (Exception ex)
				{
					string str = "消费队列异常:";
					Exception ex2 = ex;
					RmqLogHelper.WriteError(str + ((ex2 != null) ? ex2.ToString() : null));
				}
				return true;
			}

			private RmqQueueCommonOperate<T>.ConsumerWorker Worker;
		}
	}
}
