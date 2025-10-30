using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SmartCityBackend.Services;

namespace SmartCityBackend.Controllers;

[ApiController]
[Route("dispatch")]
public class DispatchController : ControllerBase
{
    private readonly ILogger<DispatchController> _logger;
    private readonly EmailService _emailService;
    private readonly IConfiguration _configuration;

    public DispatchController(
        ILogger<DispatchController> logger, 
        EmailService emailService,
        IConfiguration configuration)
    {
        _logger = logger;
        _emailService = emailService;
        _configuration = configuration;
    }

    private async Task<IActionResult> ProcessDispatch(JsonElement alertData, string entityName, string emoji)
    {
        _logger.LogInformation($"{emoji} DESPACHO A {entityName.ToUpper()}");
        
        try
        {
            // Extraer datos de la alerta
            var alertId = alertData.TryGetProperty("alert_id", out var id) ? id.ToString() : "N/A";
            var correlationId = alertData.TryGetProperty("correlation_id", out var corrId) ? corrId.ToString() : "N/A";
            var alertType = alertData.TryGetProperty("type", out var type) ? type.ToString() : "DESCONOCIDO";
            var zone = alertData.TryGetProperty("zone", out var z) ? z.ToString() : "N/A";
            var timestamp = alertData.TryGetProperty("window_start", out var ts) 
                ? DateTime.Parse(ts.ToString()) 
                : DateTime.UtcNow;
            
            // Extraer geolocalización directamente del payload
            double? geoLat = null;
            double? geoLon = null;
            
            if (alertData.TryGetProperty("geo_lat", out var lat))
            {
                if (lat.ValueKind == JsonValueKind.Number)
                    geoLat = lat.GetDouble();
            }
                
            if (alertData.TryGetProperty("geo_lon", out var lon))
            {
                if (lon.ValueKind == JsonValueKind.Number)
                    geoLon = lon.GetDouble();
            }
            
            // Obtener details como string
            var details = alertData.TryGetProperty("details", out var detailsObj) 
                ? detailsObj.ToString() 
                : "";

            _logger.LogInformation($"Alert ID: {alertId} | Zona: {zone} | Tipo: {alertType} | Coords: ({geoLat}, {geoLon})");

            // Enviar email
            var recipientEmail = _configuration["Email:AlertRecipient"] ?? "oscarrivera4901@gmail.com";
            
            var emailSent = await _emailService.SendAlertEmailAsync(
                toEmail: recipientEmail,
                alertType: alertType,
                zone: zone ?? "Desconocida",
                timestamp: timestamp,
                geoLat: geoLat,
                geoLon: geoLon,
                targetEntity: entityName,
                alertDetails: details
            );

            if (emailSent)
            {
                _logger.LogInformation($"✓ Email de alerta enviado a {recipientEmail}");
            }

            return Ok(new { 
                entity = entityName,
                status = "received",
                message = $"Unidad despachada - Email enviado a {recipientEmail}",
                timestamp = DateTime.UtcNow,
                emailSent = emailSent
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error procesando dispatch: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("policia-transito")]
    public async Task<IActionResult> DispatchToPoliciaTransito([FromBody] JsonElement alertData)
    {
        return await ProcessDispatch(alertData, "Policía de Tránsito", "📋");
    }

    [HttpPost("bomberos")]
    public async Task<IActionResult> DispatchToBomberos([FromBody] JsonElement alertData)
    {
        return await ProcessDispatch(alertData, "Bomberos", "🚒");
    }

    [HttpPost("bomberos-voluntarios")]
    public async Task<IActionResult> DispatchToBomberosVoluntarios([FromBody] JsonElement alertData)
    {
        return await ProcessDispatch(alertData, "Bomberos Voluntarios", "🚑");
    }

    [HttpPost("policia-nacional")]
    public async Task<IActionResult> DispatchToPoliciaNacional([FromBody] JsonElement alertData)
    {
        return await ProcessDispatch(alertData, "Policía Nacional (PNC)", "👮");
    }

    [HttpPost("cruz-roja")]
    public async Task<IActionResult> DispatchToCruzRoja([FromBody] JsonElement alertData)
    {
        return await ProcessDispatch(alertData, "Cruz Roja", "🏥");
    }

    [HttpPost("policia-municipal")]
    public async Task<IActionResult> DispatchToPoliciaMunicipal([FromBody] JsonElement alertData)
    {
        return await ProcessDispatch(alertData, "Policía Municipal", "🚓");
    }

    [HttpGet("status")]
    public IActionResult GetDispatchStatus()
    {
        return Ok(new { 
            service = "Smart City Dispatch System",
            status = "operational",
            emailRecipient = _configuration["Email:AlertRecipient"],
            entities = new[] {
                "policia-transito",
                "bomberos",
                "bomberos-voluntarios",
                "policia-nacional",
                "cruz-roja",
                "policia-municipal"
            }
        });
    }
}
