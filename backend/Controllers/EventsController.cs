using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
    private readonly EventValidatorService _validator;
    private readonly KafkaProducerService _producer;

    public EventsController(EventValidatorService validator, KafkaProducerService producer)
    {
        _validator = validator;
        _producer = producer;
    }

    [HttpPost]
    public IActionResult Post([FromBody] JObject eventData)
    {
        if (!_validator.Validate(eventData)) return BadRequest("Invalid schema or payload");

        // Enriquecer con metadatos (Zona 10 por defecto si no está en geo)
        eventData["timestamp"] = eventData["timestamp"] ?? DateTime.UtcNow.ToString("o");
        var geo = eventData["geo"] as JObject ?? new JObject();
        geo["zone"] = geo["zone"]?.ToString() ?? "Zona 10";
        geo["lat"] = geo["lat"] ?? 14.6091;
        geo["lon"] = geo["lon"] ?? -90.5252;
        eventData["geo"] = geo;

        _producer.Publish("events-topic", eventData.ToString());
        return Ok("Evento enviado y enriquecido para alerta en Zona 10");
    }

    [HttpGet("schema")]
    public IActionResult GetSchema()
    {
        return File("Schemas/panic-button-schema.json", "application/json");
    }
    //test
}