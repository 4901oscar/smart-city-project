using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCityBackend.Models;

/// <summary>
/// Modelo para alertas correlacionadas en el sistema Smart City
/// </summary>
[Table("alerts")]
public class Alert
{
    /// <summary>
    /// Identificador único de la alerta
    /// </summary>
    [Key]
    [Column("alert_id")]
    public Guid AlertId { get; set; }

    /// <summary>
    /// ID de correlación con el evento original
    /// </summary>
    [Column("correlation_id")]
    public Guid? CorrelationId { get; set; }

    /// <summary>
    /// Tipo de alerta generada
    /// </summary>
    [Required]
    [Column("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Score o puntuación de la alerta (severidad numérica)
    /// </summary>
    [Column("score")]
    public decimal? Score { get; set; }

    /// <summary>
    /// Zona geográfica de la alerta
    /// </summary>
    [Column("zone")]
    public string? Zone { get; set; }

    /// <summary>
    /// Inicio de la ventana temporal de correlación
    /// </summary>
    [Column("window_start")]
    public DateTime? WindowStart { get; set; }

    /// <summary>
    /// Fin de la ventana temporal de correlación
    /// </summary>
    [Column("window_end")]
    public DateTime? WindowEnd { get; set; }

    /// <summary>
    /// Evidencia de la alerta (array de event_ids con contexto)
    /// Almacenado como JSONB en PostgreSQL
    /// </summary>
    [Column("evidence", TypeName = "jsonb")]
    public string? Evidence { get; set; }

    /// <summary>
    /// Fecha y hora de creación de la alerta
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
