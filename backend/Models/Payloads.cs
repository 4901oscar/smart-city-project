using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartCityBackend.Models.Payloads;

public class PanicButtonPayload
{
    [Required]
    [JsonPropertyName("tipo_alerta")]
    public string TipoDeAlerta { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("device_id")]
    public string IdentificadorDispositivo { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("user_context")]
    public string UserContext { get; set; } = string.Empty;
}

public class CitizenReportPayload
{
    [Required]
    [JsonPropertyName("tipo_evento")]
    public string TipoEvento { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("mensaje")]
    public string MensajeDescriptivo { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("ubicacion")]
    public string UbicacionAproximada { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("origen")]
    public string Origen { get; set; } = string.Empty;
}

public class LprCameraPayload
{
    [Required]
    [JsonPropertyName("placa")]
    public string PlacaVehicular { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    [JsonPropertyName("velocidad")]
    public double VelocidadEstimada { get; set; }

    [Required]
    [JsonPropertyName("modelo")]
    public string ModeloVehiculo { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("color")]
    public string ColorVehiculo { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("sensor_ubicacion")]
    public string UbicacionSensor { get; set; } = string.Empty;
}

public class SpeedMotionPayload
{
    [Required]
    [Range(0, double.MaxValue)]
    [JsonPropertyName("velocidad")]
    public double VelocidadDetectada { get; set; }

    [Required]
    [JsonPropertyName("sensor_id")]
    public string SensorId { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("direccion")]
    public string Direccion { get; set; } = string.Empty;
}

public class AcousticAmbientPayload
{
    [Required]
    [JsonPropertyName("tipo_sonido")]
    public string TipoSonidoDetectado { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    [JsonPropertyName("decibeles")]
    public double NivelDecibeles { get; set; }

    [Required]
    [Range(0, 1)]
    [JsonPropertyName("probabilidad_critica")]
    public double ProbabilidadEventoCritico { get; set; }
}