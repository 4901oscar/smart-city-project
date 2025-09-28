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

    public EventsController(EventValidatorService validator, KafkaProducerService producer, EventDbContext context)
    {
        _validator = validator;
        _producer = producer;
        _context = context;
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
                return BadRequest(new { message = "Invalid schema or payload", errors = validationResult.errors });
            }

        // Enriquecer (Zona 10 por defecto)
        eventData["timestamp"] = eventData["timestamp"] ?? DateTime.UtcNow.ToString("o");
        var geo = eventData["geo"] as JObject ?? new JObject();
        geo["zone"] = geo["zone"]?.ToString() ?? "Zona 10";
        geo["lat"] = geo["lat"] ?? 14.6091;
        geo["lon"] = geo["lon"] ?? -90.5252;
        eventData["geo"] = geo;

        // Publicar en Kafka
        try
        {
            await _producer.Publish("events-topic", eventData.ToString());
        }
        catch (Exception kex)
        {
            return StatusCode(502, new { message = "Error enviando a Kafka", detail = kex.Message });
        }

        // Guardar en Postgres
        var newEvent = new Event
        {
            EventType = eventData["event_type"]?.ToString() ?? "unknown",
            Payload = eventData.ToString(),
            Timestamp = DateTime.UtcNow
        };
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

            return Ok("Evento enviado, enriquecido y persistido para alerta en Zona 10");
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
                .OrderByDescending(e => e.Id)
                .Take(take)
                .Select(e => new {
                    e.Id,
                    e.EventType,
                    e.Timestamp
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

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok("Backend saludable - Listo para simulación en Zona 10");
    }

    [HttpGet("schema")]
    public async Task<IActionResult> GetSchema()
    {
        try
        {
            var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", "event-envelope-schema.json");
            var schemaContent = await System.IO.File.ReadAllTextAsync(schemaPath);
            return Ok(JObject.Parse(schemaContent));
        }
        catch (Exception ex)
        {
            return BadRequest($"Error reading schema: {ex.Message}");
        }
    }
}