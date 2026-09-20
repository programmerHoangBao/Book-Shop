
using back_end.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Confluent.Kafka;

namespace back_end.Kafka.Producers.Implements
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly KafkaSetting _setting;
        public KafkaProducer(IOptions<KafkaSetting> options)
        {
            _setting = options.Value;
        }
        public async Task ProduceAsync<T>(string topic, T message)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _setting.BootstrapServers
            };

            using var producer =
                new ProducerBuilder<Null, string>(config).Build();

            var json = JsonSerializer.Serialize(message);

            await producer.ProduceAsync(
                topic,
                new Message<Null, string>
                {
                    Value = json
                });
        }
    }
}
