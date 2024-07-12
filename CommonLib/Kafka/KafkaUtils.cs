using Confluent.Kafka;

namespace CommonLib.Kafka
{
    public class KafkaUtils
    {
        public KafkaUtils()
        {
        }
        public async Task ProduceMessageAsync(string servers, string topic, Message<Null, string> message)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = servers,
                EnableDeliveryReports = true,
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000,
            };
            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    var res = await producer.ProduceAsync(topic, message);
                    Console.WriteLine($"===> kafka produce async: {res.ToJson()}");
                }
                catch (ProduceException<Null, string> e)
                {
                    throw e;
                }
            }
        }
        public void ProduceMessage(string servers, string topic, Message<Null, string> message)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = servers,
                EnableDeliveryReports = true,
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000
            };
            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    producer.Produce(topic, message);
                }
                catch (ProduceException<Null, string> e)
                {
                    throw e;
                }
            }
        }

        public async Task ProduceMessageAsync(string servers, string topic, Message<Null, string> message, string userName = null, string password = null, int securityProtocol = 0, int mechanism = 0)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = servers,
                EnableDeliveryReports = true,
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000,
                SaslUsername = userName,
                SaslPassword = password,
                SecurityProtocol = (SecurityProtocol)securityProtocol,
                SaslMechanism = (SaslMechanism)mechanism,
            };
            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    var res = await producer.ProduceAsync(topic, message);
                    Console.WriteLine($"===> kafka produce async: {res.ToJson()}");
                }
                catch (ProduceException<Null, string> e)
                {
                    throw e;
                }
            }
        }
        public void ProduceMessage(string servers, string topic, Message<Null, string> message, string userName = null, string password = null, int securityProtocol = 0, int mechanism = 0)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = servers,
                EnableDeliveryReports = true,
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000,
                SaslUsername = userName,
                SaslPassword = password,
                SecurityProtocol = (SecurityProtocol)securityProtocol,
                SaslMechanism = (SaslMechanism)mechanism,
            };
            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    producer.Produce(topic, message);
                }
                catch (ProduceException<Null, string> e)
                {
                    throw e;
                }
            }
        }
    }
}

