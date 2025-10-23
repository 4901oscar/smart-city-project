using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SmartCityBackend.Services;  // Agrega el namespace
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace SmartCityBackend.Controllers;

[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
    private readonly EventValidatorService _validator;
    private readonly KafkaProducerService _producer;
    private readonly EventDbContext _context;
    // ElasticsearchService deshabilitado
    // private readonly ElasticsearchService _elasticsearch;


    public EventsController(EventValidatorService validator, KafkaProducerService producer, EventDbContext context)
    {
        _validator = validator;
        _producer = producer;
        _context = context;
        // _elasticsearch = elasticsearch;

    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            var eventData = JObject.Parse(json);
            
            var validationResult = _validator.ValidateDetailed(eventData);
            if (!validationResult.isValid)
            {
                // Publicar eventos inválidos a DLQ
                var dlqMessage = new JObject
                {
                    ["original_event"] = eventData,
                    ["validation_errors"] = JArray.FromObject(validationResult.errors),
                    ["timestamp"] = DateTime.UtcNow.ToString("o"),
                    ["reason"] = "SCHEMA_VALIDATION_FAILED"
                };
                
                try
                {
                    await _producer.Publish("events.dlq", dlqMessage.ToString());
                }
                catch
                {
                    // Si falla DLQ, continuar con el error
                }
                
                return BadRequest(new { message = "Invalid schema or payload", errors = validationResult.errors });
            }

        // Enriquecer (Zona 10 por defecto)
        eventData["timestamp"] = eventData["timestamp"] ?? DateTime.UtcNow.ToString("o");
        var geo = eventData["geo"] as JObject ?? new JObject();
        geo["zone"] = geo["zone"]?.ToString() ?? "Zona 10";
        geo["lat"] = geo["lat"] ?? 14.6091;
        geo["lon"] = geo["lon"] ?? -90.5252;
        eventData["geo"] = geo;

        // Publicar en Kafka → events.standardized (eventos válidos y enriquecidos)
        try
        {
            await _producer.Publish("events.standardized", eventData.ToString());
        }
        catch (Exception kex)
        {
            // En caso de error de Kafka, publicar a DLQ
            var dlqMessage = new JObject
            {
                ["original_event"] = eventData,
                ["error"] = kex.Message,
                ["timestamp"] = DateTime.UtcNow.ToString("o"),
                ["reason"] = "KAFKA_PUBLISH_ERROR"
            };
            
            try
            {
                await _producer.Publish("events.dlq", dlqMessage.ToString());
            }
            catch
            {
                // Si falla DLQ, continuar con el error
            }
            
            return StatusCode(502, new { message = "Error enviando a Kafka", detail = kex.Message });
        }

        // Guardar en Postgres con todos los campos del schema
        var newEvent = new Event
        {
            EventId = Guid.Parse(eventData["event_id"]?.ToString() ?? Guid.NewGuid().ToString()),
            EventType = eventData["event_type"]?.ToString() ?? "unknown",
            EventVersion = eventData["event_version"]?.ToString() ?? "1.0",
            Producer = eventData["producer"]?.ToString() ?? "unknown",
            Source = eventData["source"]?.ToString() ?? "simulated",
            CorrelationId = Guid.TryParse(eventData["correlation_id"]?.ToString(), out var corrId) ? corrId : null,
            TraceId = Guid.TryParse(eventData["trace_id"]?.ToString(), out var traceId) ? traceId : null,
            PartitionKey = eventData["partition_key"]?.ToString() ?? "default",
            TsUtc = DateTime.TryParse(eventData["timestamp"]?.ToString(), out var ts) 
                ? DateTime.SpecifyKind(ts, DateTimeKind.Utc) 
                : DateTime.UtcNow,
            Zone = geo["zone"]?.ToString(),
            GeoLat = geo["lat"] != null ? Convert.ToDecimal(geo["lat"]) : null,
            GeoLon = geo["lon"] != null ? Convert.ToDecimal(geo["lon"]) : null,
            Severity = eventData["severity"]?.ToString(),
            Payload = eventData["payload"]?.ToString() ?? "{}"
        };
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        
        // Elasticsearch indexing deshabilitado
        // await _elasticsearch.IndexEventAsync(eventData, "events-000001");

        var zoneName = geo["zone"]?.ToString() ?? "Zona Desconocida";
        return Ok($"Evento enviado, enriquecido y persistido para alerta en {zoneName}");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error processing event: {ex.Message}");
        }
    }

    // Listar eventos (solo verificación rápida) /events?take=20
    [HttpGet]
    public IActionResult List([FromQuery] int take = 20)
    {
        try
        {
            take = Math.Clamp(take, 1, 100);
            var data = _context.Events
                .OrderByDescending(e => e.TsUtc)
                .Take(take)
                .Select(e => new {
                    e.EventId,
                    e.EventType,
                    e.TsUtc,
                    e.Zone,
                    e.Severity
                })
                .ToList();
            return Ok(new { count = data.Count, items = data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error leyendo eventos", detail = ex.Message });
        }
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkPost([FromBody] JArray eventsData)
    {
        if (eventsData == null) return BadRequest("Invalid bulk payload");

        foreach (var eventData in eventsData)
        {
            if (eventData is JObject obj && _validator.Validate(obj))
            {
                await _producer.Publish("events-topic", obj.ToString());
            }
        }
        return Ok("Eventos masivos enviados");
    }
}
