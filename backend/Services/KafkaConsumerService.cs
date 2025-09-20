//consume eventos y los guarda estructuradamente

using backend.Interfaces;
using Confluent.Kafka;
using Newtonsoft.Json.Linq;

namespace backend.Services;


public class KafkaConsumerService : IkafkaConsumerService
{
    private readonly ConsumerConfig _config = new ConsumerConfig
    {
        BootstrapServers = "localhost:9092",
        GroupId = "event-consumer-group",
        AutoOffsetReset = AutoOffsetReset.Earliest
    };

    private readonly EventValidatorService _validator;
    private readonly EventEnrichmentService _enricher;
    private readonly EventRepository _repository;

    public KafkaConsumerService(EventValidatorService validator, EventEnrichmentService enricher, EventRepository repository)
    {
        _validator = validator;
        _enricher = enricher;
        _repository = repository;
    }

    public void Start()
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
        consumer.Subscribe("events-topic");

        while (true)
        {
            var result = consumer.Consume();
            var json = JObject.Parse(result.Message.Value);

            if (!_validator.Validate(json)) continue;

            var enriched = _enricher.Enrich(json);
            _repository.Save(enriched);
        }
    }
}