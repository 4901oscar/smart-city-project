<<<<<<< HEAD
using NJsonSchema;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json.Linq;

public class EventValidatorService
=======
//Valida el Envelope y el payload segun su tipo

using NJsonSchema;
using Microsoft.Extensions.Configuration;
using System.IO;
using backend.Interfaces;
using Newtonsoft.Json.Linq;

public class EventValidatorService : IEventValidatorService
>>>>>>> 596a44f (Servicios e interfaces)
{
    private readonly JsonSchema _envelopeSchema;
    private readonly Dictionary<string, JsonSchema> _payloadSchemas;

    public EventValidatorService(IConfiguration config)
    {
        _envelopeSchema = JsonSchema.FromFileAsync(Path.Combine("Schemas", "event-envelope-schema.json")).Result;
        _payloadSchemas = new Dictionary<string, JsonSchema>
        {
            ["panic.button"] = JsonSchema.FromFileAsync(Path.Combine("Schemas", "panic-button-schema.json")).Result,
            ["sensor.lpr"] = JsonSchema.FromFileAsync(Path.Combine("Schemas", "lpr-camera-schema.json")).Result,
            ["sensor.speed"] = JsonSchema.FromFileAsync(Path.Combine("Schemas", "speed-motion-schema.json")).Result,
            ["sensor.acoustic"] = JsonSchema.FromFileAsync(Path.Combine("Schemas", "acoustic-ambient-schema.json")).Result,
            ["citizen.report"] = JsonSchema.FromFileAsync(Path.Combine("Schemas", "citizen-report-schema.json")).Result
        };
    }

    public bool Validate(JObject payload)
    {
        var envelopeErrors = _envelopeSchema.Validate(payload);
        if (envelopeErrors.Any()) return false;

        string eventType = payload["event_type"]?.ToString();
        if (string.IsNullOrEmpty(eventType) || !_payloadSchemas.ContainsKey(eventType)) return false;

        JObject payloadData = payload["payload"] as JObject;
        if (payloadData == null) return false;

        var payloadErrors = _payloadSchemas[eventType].Validate(payloadData);
        return !payloadErrors.Any();
    }
}