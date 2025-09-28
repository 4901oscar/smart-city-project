using NJsonSchema;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace SmartCityBackend.Services;

public class EventValidatorService
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

    public bool Validate(JObject? payload) => ValidateDetailed(payload).isValid;

    public (bool isValid, List<string> errors) ValidateDetailed(JObject? payload)
    {
        var errors = new List<string>();
        if (payload == null)
        {
            errors.Add("Payload is null");
            return (false, errors);
        }

        var envelopeErrors = _envelopeSchema.Validate(payload);
        if (envelopeErrors.Any())
        {
            errors.AddRange(envelopeErrors.Select(e => $"Envelope: {e.Path} -> {e.Kind} ({e})"));
        }

        string? eventType = payload["event_type"]?.ToString();
        if (string.IsNullOrEmpty(eventType))
        {
            errors.Add("event_type missing or empty");
        }
        else if (!_payloadSchemas.ContainsKey(eventType))
        {
            errors.Add($"No payload schema for event_type '{eventType}'");
        }

        JObject? payloadData = payload["payload"] as JObject;
        if (payloadData == null)
        {
            errors.Add("payload object missing or not an object");
        }
        else if (!string.IsNullOrEmpty(eventType) && _payloadSchemas.ContainsKey(eventType))
        {
            var payloadErrors = _payloadSchemas[eventType].Validate(payloadData);
            errors.AddRange(payloadErrors.Select(e => $"Payload: {e.Path} -> {e.Kind} ({e})"));
        }

        var ok = !errors.Any();
        Console.WriteLine(ok ? "Validation successful" : $"Validation failed: {string.Join(" | ", errors)}");
        return (ok, errors);
    }
}