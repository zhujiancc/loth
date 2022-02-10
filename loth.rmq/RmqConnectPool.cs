using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RabbitMQ.Client;

namespace loth.rmq
{
    public class RmqConnectPool
    {
        public ConnectionFactory GetInnerFactoryObject()
        {
            return this._factory;
        }

        public RmqConnectPool(string uri, int poolSize, int checkSleepSecs = 3)
        {
            this._connect = uri;
            this._poolSize = poolSize;
            this._checkSleepSecs = checkSleepSecs;
            this._rd = new Random();
            this._readWriteLock = new ReaderWriterLockSlim();
            this._factory = new ConnectionFactory
            {
                Uri = new Uri(uri),
                AutomaticRecoveryEnabled = false,
                RequestedHeartbeat = TimeSpan.FromSeconds(3),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(60),
                SocketWriteTimeout = TimeSpan.FromSeconds(30),
                SocketReadTimeout = TimeSpan.FromSeconds(30),
                NetworkRecoveryInterval = TimeSpan.FromSeconds(3.0)
            };
            new Thread(delegate ()
            {
                try
                {
                    this.ConnectCheck();
                }
                catch (Exception ex2)
                {
                    RmqLogHelper.Error("数据库心跳检测异常：" + ex2.ToString());
                }
            }).Start();
            try
            {
                using (IConnection connection = this._factory.CreateConnection())
                {
                    using (IModel model = connection.CreateModel())
                    {
                        model.ExchangeDeclare(RmqBasicInfoConfig.BaseExchange, "direct", true, false, null);
                    }
                }
            }
            catch (Exception ex)
            {
                RmqLogHelper.Error("基础exchange创建出错：" + ex.ToString());
            }
        }

        public RmqConnect GetConnect()
        {
            this._readWriteLock.EnterUpgradeableReadLock();
            RmqConnect result = null;
            try
            {
                bool flag = (from p in this._cList
                             where p.GetRealConnectInfo().IsOpen
                             select p).Count<RmqConnect>() < this._poolSize;
                if (flag)
                {
                    try
                    {
                        this._readWriteLock.EnterWriteLock();
                        IConnection connect = this._factory.CreateConnection();
                        RmqConnect item = new RmqConnect(connect);
                        RmqLogHelper.Info("rmq 生成链接,当前连接数:" + this._cList.Count.ToString());
                        this._cList.Add(item);
                        this._cList.RemoveAll((RmqConnect p) => !p.GetRealConnectInfo().IsOpen);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        this._readWriteLock.ExitWriteLock();
                    }
                }
                return (from p in this._cList
                        where p.GetRealConnectInfo().IsOpen
                        orderby Guid.NewGuid() descending
                        select p).FirstOrDefault<RmqConnect>();
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.ToString());
            }
            finally
            {
                this._readWriteLock.ExitUpgradeableReadLock();
            }
            return result;
        }

        private void ConnectCheck()
        {
            for (; ; )
            {
                Thread.Sleep(1000 * this._checkSleepSecs);
                this._readWriteLock.EnterReadLock();
                List<RmqConnect> list = new List<RmqConnect>();
                try
                {
                    for (int i = 0; i < this._cList.Count; i++)
                    {
                        RmqConnect rmqConnect = this._cList[i];
                        try
                        {
                            using (IModel channel = rmqConnect.GetChannel())
                            {
                                channel.ExchangeDeclare("__HeartChcekRmqPool", "fanout", false, false, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            RmqLogHelper.Info("rmq connectcheck 异常，加入到异常连接池:" + ex.ToString());
                            try
                            {
                                IConnection realConnectInfo = rmqConnect.GetRealConnectInfo();
                                bool flag = realConnectInfo != null && realConnectInfo.IsOpen;
                                if (flag)
                                {
                                    realConnectInfo.Close();
                                }
                            }
                            catch (Exception)
                            {
                            }
                            list.Add(rmqConnect);
                        }
                    }
                }
                finally
                {
                    this._readWriteLock.ExitReadLock();
                }
                bool flag2 = list.Count > 0;
                if (flag2)
                {
                    this._readWriteLock.EnterWriteLock();
                    try
                    {
                        foreach (RmqConnect item in list)
                        {
                            RmqLogHelper.Error("rmq connectcheck 异常，移除链接:" + this._cList.Remove(item).ToString());
                        }
                    }
                    finally
                    {
                        this._readWriteLock.ExitWriteLock();
                    }
                }
            }
        }

        private List<RmqConnect> _cList = new List<RmqConnect>();

        private object _lock = new object();

        private string _connect;

        private int _poolSize;

        private int _checkSleepSecs;

        private ConnectionFactory _factory;

        private Random _rd;

        private ReaderWriterLockSlim _readWriteLock;
    }
}
