using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartCityBackend.Models;

/// <summary>
/// Envelope que contiene todos los eventos del sistema Smart City
/// </summary>
public class EventEnvelope
{
    /// <summary>
    /// Versión del evento (debe ser "1.0")
    /// </summary>
    [Required]
    [JsonPropertyName("event_version")]
    public string EventVersion { get; set; } = "1.0";

    /// <summary>
    /// Tipo de evento que se está enviando
    /// </summary>
    [Required]
    [JsonPropertyName("event_type")]
    public EventType EventType { get; set; }

    /// <summary>
    /// Identificador único del evento (UUID)
    /// </summary>
    [Required]
    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Productor del evento
    /// </summary>
    [Required]
    [JsonPropertyName("producer")]
    public EventProducer Producer { get; set; }

    /// <summary>
    /// Fuente del evento (debe ser "simulated")
    /// </summary>
    [Required]
    [JsonPropertyName("source")]
    public string Source { get; set; } = "simulated";

    /// <summary>
    /// ID de correlación para trazabilidad (UUID)
    /// </summary>
    [Required]
    [JsonPropertyName("correlation_id")]
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// ID de trazabilidad (UUID)
    /// </summary>
    [Required]
    [JsonPropertyName("trace_id")]
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp del evento en formato ISO 8601
    /// </summary>
    [Required]
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Clave de partición para Kafka
    /// </summary>
    [Required]
    [JsonPropertyName("partition_key")]
    public string PartitionKey { get; set; } = string.Empty;

    /// <summary>
    /// Información geográfica del evento
    /// </summary>
    [Required]
    [JsonPropertyName("geo")]
    public GeoLocation Geo { get; set; } = new();

    /// <summary>
    /// Severidad del evento
    /// </summary>
    [Required]
    [JsonPropertyName("severity")]
    public EventSeverity Severity { get; set; }

    /// <summary>
    /// Payload específico según el tipo de evento
    /// </summary>
    [Required]
    [JsonPropertyName("payload")]
    public object Payload { get; set; } = new();
}

/// <summary>
/// Tipos de eventos disponibles en el sistema
/// </summary>
public enum EventType
{
    [JsonPropertyName("panic.button")]
    PanicButton,
    
    [JsonPropertyName("sensor.lpr")]
    SensorLpr,
    
    [JsonPropertyName("sensor.speed")]
    SensorSpeed,
    
    [JsonPropertyName("sensor.acoustic")]
    SensorAcoustic,
    
    [JsonPropertyName("citizen.report")]
    CitizenReport
}

/// <summary>
/// Productores de eventos disponibles
/// </summary>
public enum EventProducer
{
    [JsonPropertyName("artillery")]
    Artillery,
    
    [JsonPropertyName("python-sim")]
    PythonSim,
    
    [JsonPropertyName("kafka-cli")]
    KafkaCli
}

/// <summary>
/// Niveles de severidad del evento
/// </summary>
public enum EventSeverity
{
    [JsonPropertyName("info")]
    Info,
    
    [JsonPropertyName("warning")]
    Warning,
    
    [JsonPropertyName("critical")]
    Critical
}

/// <summary>
/// Información de ubicación geográfica
/// </summary>
public class GeoLocation
{
    /// <summary>
    /// Zona geográfica (ej: "Zona 10")
    /// </summary>
    [Required]
    [JsonPropertyName("zone")]
    public string Zone { get; set; } = string.Empty;

    /// <summary>
    /// Latitud en grados decimales
    /// </summary>
    [Required]
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    /// <summary>
    /// Longitud en grados decimales
    /// </summary>
    [Required]
    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}