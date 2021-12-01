RMQ 组件

2、生产者发送消息代码案例：
`RmqMessageProducer.MessageSend<string>(RmqUrlEnum.WeiBo, "Xigua.Weibo", "xiguatest",DateTime.Now.Ticks.ToString(), new RmqMessageProps()
  {
      DeliveryMode = 1
  });`

 静态方法MessageSend几个参数如下：

 1、rmqUrlEnum 枚举，表示使用什么rmq集群

 2、exchange 交换机名称

 3、routekey 路由键

 4、消息实体对象

 5、消息的属性 （过期时间，是否持久化 等）
    `/// <summary>
    /// 消息是否持久化：1 非持久化，2 持久化
    /// </summary>
    public int? DeliveryMode { get; set; } = 1;
    /// <summary>
    /// 消息自动过期毫秒数,若不设置则不过期
    /// </summary>
    public int? Expiration { get; set; }

    /// <summary>
    /// 消息的Headers 属性
    /// </summary>
    public Dictionary<string, object> Headers {get;set;}

    /// <summary>
    /// 消息的优先级(0-9)
    /// </summary>
    public RmqMessagePriorityLevel Priority { get; set; } = 0;

    /// <summary>
    /// 业务级别的消息Id
    /// </summary>
    public string MessageId { get; set; }

    /// <summary>
    /// 消息的唯一Id
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    /// 发送消息者的唯一Id
    /// </summary>
    public string UserId { get; set; }`

3、消费者消费消息案例：
    实现继承自 RmqComsumerWorkerBase 对象类，并赋值几个核心参数

    1、 ConsumerCount 消费者数目（可以理解为多少线程）

    2、 RmqUrl 枚举表示使用什么集群
    
    3、QueueName  消费的目标队列
    
    4、AutoAckOk 是否消息接收后，就通知rmq 消息已经处理ok，默认是true

    5、FailEnqueue （默认false）设置为true，表示消息处理失败时即 handle 异常或者返回false 之后，加入到失败队列（队列名称_hardbone_{queuename}）

    6、实现 HandleComingMessage 这个方法

    `public class XiGuaConsume : RmqComsumerWorkerBase<string>
    {
        public XiGuaConsume()
        {
            this.TotalAckOk = true;
            this.QueueName = "xiguatest";
            this.RmqUrl = RmqUrlEnum.Douyin;
            this.ConsumerCount = 2;
        }

        protected override bool HandleComingMessage(string messageDTO)
        {
            Console.WriteLine(messageDTO.ToString());
            return true;
        }
    }`

   //消费者运行,start 里面就是while true 循环
   new XiGuaConsume().Start();

4、主动拉取消息类
 //一次性从队列中拉取100条消息出来
 `var list = RmqPullMessageService.GetMessagesBulk<string>(100, "test", RmqUrlEnum.WeiBo);`

5、rmqApi 接口封装
    `//查询队列长度
     var count = new RmqHttpApiService(RmqUrlEnum.WeiBo).GetMessageCount("test");`

6、队列混合操作封装
`RmqQueueCommonOperate<string> commonoperate = new RmqQueueCommonOperate<string>(RmqUrlEnum.XiGuaWx, "xigua.Spider", "abc");
        commonoperate.Enqueue("adsf", "routekey", RmqMessagePriorityLevel.Lower, 1);
        ///队列长度大于0 开始消费
        if (commonoperate.GetCount() > 0) {
            // 10 个线程去消费
            commonoperate.StartConsumer(
                a => { Console.WriteLine(a); return true; }, 10);
        }

`
7、批量主动拉取消息并消费的对象
public class BatchConsume : RmqBatchComsumerWorkBase<string>
    {
        public BatchConsume()
        {
            this.Interval = TimeSpan.FromSeconds(30); 当消息不足BatchSize时，但时间超过30秒就先返回
            this.RmqUrl = RmqUrlEnum.Douyin;
            this.BatchSize = 100;
            this.QueueName = ""; 
        } 
        //这里单线程的原因是，多条消息内部做其他操作（例如去重聚合等），若想多线程处理批量消息，参考下面7.1多线程操作
        protected override bool HandleComingMessage(List<string> messageDTO)
        {
            //批量处理这个任务
            return true;
        }
    }
BatchConsume.Run() //里面就while ture 运行

7.1、批量主动拉取消息并多线程消费的对象
  public class PullmessageConsume : RmqBatchComsumerMultThreadWorkBase<string>
    {
        public  PullmessageConsume() 
        {
           this.Interval = TimeSpan.FromSeconds(30); 当消息不足BatchSize时，但时间超过30秒就先返回
            this.QueueName = "";
            this.RmqUrl = RmqUrlEnum.Douyin;
            this.BatchSize = 100;
            this.ThreadCount = 5;

        }
        protected override bool HandleOneMessage(string messageDTO)
        {
            return true;
        }
    }
PullmessageConsume.Run() //里面就while ture 运行

8、单条取单条消费

public class XiGuaConsume2 : RmqOneMsgComsumerWorkerBase<Test0bJ>
    {
        public XiGuaConsume2()
        { 
            this.QueueName = "xiguatest";
            this.RmqUrl = RmqUrlEnum.KuaiShou; 
            this.FailEnqueue = true;
        }

         protected override bool HandleComingMessage(Test0bJ messageDTO)
        {
            Console.WriteLine(DateTime.Now.ToString()+":"+ messageDTO.Ticks.ToString());
            Thread.Sleep(1000 * 1);
            return true;
        }
    }

while (true) {
                //while循环一条条取并消费
                new XiGuaConsume2().HandleOneMessage();
            }

9、其他操作：
   若想创建channel 并 使用他创建队列，exchange 等，直接如下操作，千万记住connection 不要using 也不要把他关闭 ：
    
        `var connection = RmqConnectPoolFactory.GetClientPool(RmqUrlEnum.WeiBo).GetConnect();
        using (var channle = connection.GetChannel())
        {
            //操作channel 
        }`

特别说明：rmq 若消费时间太长，我们默认5分钟就可能丢回去，消息可能会丢回原先的队列，这种情况下，请使用PullmessageConsume 对象，相当于主动从rmq 中拉消息下来，并且多线程消费
