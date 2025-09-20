namespace backend.Interfaces;

public interface IKafkaProducerService
{
    Task Publish(string topic, string message);
}