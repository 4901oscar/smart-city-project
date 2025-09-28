using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class KafkaProducerService {
    private readonly ProducerConfig _config;
    private readonly IProducer<Null, string> _producer; // Reusar instancia (registrado como singleton)
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger) {
        _logger = logger;

        var servers = config["Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(servers)) {
            servers = "kafka:29092"; // fallback interno dentro del cluster docker
            _logger.LogWarning("Kafka:BootstrapServers no configurado. Usando fallback {servers}", servers);
        } else {
            _logger.LogInformation("Kafka BootstrapServers configurado: {servers}", servers);
        }

        _config = new ProducerConfig {
            BootstrapServers = servers,
            Acks = Acks.All,
            MessageTimeoutMs = 5000
        };

        _producer = new ProducerBuilder<Null, string>(_config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
            .Build();
    }

    public async Task Publish(string topic, string message) {
        try {
            var dr = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
            if (dr.Status == PersistenceStatus.NotPersisted) {
                throw new Exception($"Mensaje no persistido en Kafka (Topic={topic})");
            }
            _logger.LogInformation("Mensaje publicado en {Topic} @ Partition {Partition} Offset {Offset}", dr.Topic, dr.Partition, dr.Offset);
        } catch (ProduceException<Null, string> ex) {
            _logger.LogError(ex, "Error produciendo a Kafka: {Reason}", ex.Error.Reason);
            throw;
        }
    }
}