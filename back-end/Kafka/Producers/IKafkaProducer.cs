namespace back_end.Kafka.Producers
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<T>(string topic, T message);
    }
}
