using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SmartCityBackend.Services;

namespace SmartCityBackend.Controllers;

[ApiController]
[Route("alerts")]
public class AlertsController : ControllerBase
{
    private readonly EventDbContext _context;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(EventDbContext context, ILogger<AlertsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint para recibir y guardar alertas correlacionadas
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Post()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            var alertData = JObject.Parse(json);

            // Extraer datos de la alerta
            var alertId = alertData["alert_id"]?.ToString() ?? Guid.NewGuid().ToString();
            var correlationId = alertData["correlation_id"]?.ToString();
            
            var sourceEventId = alertData["source_event_id"]?.ToString();
            var eventType = alertData["event_type"]?.ToString();
            var zone = alertData["zone"]?.ToString();
            var timestamp = DateTime.TryParse(alertData["timestamp"]?.ToString(), out var ts) 
                ? DateTime.SpecifyKind(ts, DateTimeKind.Utc) 
                : DateTime.UtcNow;

            // Obtener alertas del array
            var alertsArray = alertData["alerts"] as JArray ?? new JArray();
            
            if (alertsArray.Count == 0)
            {
                return BadRequest(new { message = "No alerts provided in the request" });
            }

            // Procesar cada alerta del array
            var savedCount = 0;
            foreach (var alertItem in alertsArray)
            {
                var level = alertItem["level"]?.ToString();
                var type = alertItem["type"]?.ToString();
                var message = alertItem["message"]?.ToString();
                var details = alertItem["details"]?.ToString();

                // Calcular score basado en el nivel
                decimal score = level switch
                {
                    "CRÍTICO" => 100m,
                    "ALTO" => 75m,
                    "MEDIO" => 50m,
                    "INFO" => 25m,
                    _ => 0m
                };

                // Construir evidencia JSONB
                var evidence = new JObject
                {
                    ["source_event_id"] = sourceEventId,
                    ["event_type"] = eventType,
                    ["level"] = level,
                    ["message"] = message,
                    ["details"] = details,
                    ["timestamp"] = timestamp.ToString("o")
                };

                // Crear modelo de alerta
                var alert = new Alert
                {
                    AlertId = Guid.NewGuid().ToString(), // Generar nuevo ID para cada alerta del array
                    CorrelationId = correlationId,
                    Type = type ?? "UNKNOWN",
                    Score = score,
                    Zone = zone,
                    WindowStart = timestamp.AddMinutes(-5), // Ventana de 5 minutos antes
                    WindowEnd = timestamp,
                    Evidence = evidence.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Alerts.Add(alert);
                savedCount++;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Guardadas {Count} alertas para evento {EventId} en zona {Zone}", 
                savedCount, sourceEventId?.Substring(0, 8), zone);

            return Ok(new 
            { 
                message = $"{savedCount} alert(s) saved successfully",
                alert_id = alertId,
                correlation_id = correlationId,
                zone = zone,
                count = savedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando alertas: {Message}", ex.Message);
            return StatusCode(500, new { message = "Error saving alerts", detail = ex.Message });
        }
    }

    /// <summary>
    /// Obtener alertas recientes
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int take = 20, [FromQuery] string? zone = null)
    {
        try
        {
            var query = _context.Alerts.AsQueryable();

            if (!string.IsNullOrEmpty(zone))
            {
                query = query.Where(a => a.Zone == zone);
            }

            var alerts = await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(take)
                .Select(a => new
                {
                    a.AlertId,
                    a.CorrelationId,
                    a.Type,
                    a.Score,
                    a.Zone,
                    a.WindowStart,
                    a.WindowEnd,
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo alertas: {Message}", ex.Message);
            return StatusCode(500, new { message = "Error retrieving alerts", detail = ex.Message });
        }
    }

    /// <summary>
    /// Obtener estadísticas de alertas
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] string? zone = null)
    {
        try
        {
            var query = _context.Alerts.AsQueryable();

            if (!string.IsNullOrEmpty(zone))
            {
                query = query.Where(a => a.Zone == zone);
            }

            var total = await query.CountAsync();
            var last24h = await query
                .Where(a => a.CreatedAt >= DateTime.UtcNow.AddHours(-24))
                .CountAsync();

            var byType = await query
                .GroupBy(a => a.Type)
                .Select(g => new { type = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToListAsync();

            var byZone = await query
                .Where(a => a.Zone != null)
                .GroupBy(a => a.Zone)
                .Select(g => new { zone = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            var avgScore = await query
                .Where(a => a.Score != null)
                .AverageAsync(a => a.Score);

            return Ok(new
            {
                total,
                last_24h = last24h,
                by_type = byType,
                by_zone = byZone,
                avg_score = avgScore
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo estadísticas: {Message}", ex.Message);
            return StatusCode(500, new { message = "Error retrieving stats", detail = ex.Message });
        }
    }
}
