using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RabbitMQ.Client.Exceptions;

namespace EventBusRabbitMQ
{
    public class RabbitMQConnection : IRabbitMQConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private bool _dispoesed;

        public RabbitMQConnection(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            if (!IsConnected)
            {
                TryConnect();
            }
        }

        public bool IsConnected => _connection?.IsOpen == true && !_dispoesed;

        public IModel CreateModel()
        {
            if (!IsConnected) throw new InvalidOperationException("No rabbit connection");

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_dispoesed) return;

            try
            {
                _connection.Dispose();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public bool TryConnect()
        {
            try
            {
                _connection = _connectionFactory.CreateConnection();
            }
            catch (BrokerUnreachableException e)
            {
                Thread.Sleep(2000);
                _connection = _connectionFactory.CreateConnection();
            }
            return IsConnected;
        }
    }
}
