using System;
using RabbitMQ.Client;

namespace loth.rmq
{
    public class RmqConnect
    {
        public RmqConnect(IConnection connect)
        {
            this._connect = connect;
        }

        public IModel GetChannel()
        {
            return this._connect.CreateModel();
        }

        public IConnection GetRealConnectInfo()
        {
            return this._connect;
        }

        private IConnection _connect;

    }
}
