using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SmartCityBackend.Controllers;

[ApiController]
[Route("dispatch")]
public class DispatchController : ControllerBase
{
    private readonly ILogger<DispatchController> _logger;

    public DispatchController(ILogger<DispatchController> logger)
    {
        _logger = logger;
    }

    [HttpPost("policia-transito")]
    public IActionResult DispatchToPoliciaTransito([FromBody] JsonElement alertData)
    {
        _logger.LogInformation("📋 DESPACHO A POLICÍA DE TRÁNSITO");
        _logger.LogInformation($"Alert ID: {alertData.GetProperty("alert_id")}");
        _logger.LogInformation($"Zona: {alertData.GetProperty("zone")}");
        
        return Ok(new { 
            entity = "Policia de Transito",
            status = "received",
            message = "Unidad despachada",
            timestamp = DateTime.UtcNow 
        });
    }

    [HttpPost("bomberos")]
    public IActionResult DispatchToBomberos([FromBody] JsonElement alertData)
    {
        _logger.LogInformation("🚒 DESPACHO A BOMBEROS");
        _logger.LogInformation($"Alert ID: {alertData.GetProperty("alert_id")}");
        _logger.LogInformation($"Zona: {alertData.GetProperty("zone")}");
        
        return Ok(new { 
            entity = "Bomberos",
            status = "received",
            message = "Estacion de bomberos notificada",
            timestamp = DateTime.UtcNow 
        });
    }

    [HttpPost("bomberos-voluntarios")]
    public IActionResult DispatchToBomberosVoluntarios([FromBody] JsonElement alertData)
    {
        _logger.LogInformation("🚑 DESPACHO A BOMBEROS VOLUNTARIOS");
        _logger.LogInformation($"Alert ID: {alertData.GetProperty("alert_id")}");
        _logger.LogInformation($"Zona: {alertData.GetProperty("zone")}");
        
        return Ok(new { 
            entity = "Bomberos Voluntarios",
            status = "received",
            message = "Ambulancia despachada",
            timestamp = DateTime.UtcNow 
        });
    }

    [HttpPost("policia-nacional")]
    public IActionResult DispatchToPoliciaNacional([FromBody] JsonElement alertData)
    {
        _logger.LogInformation("👮 DESPACHO A POLICÍA NACIONAL");
        _logger.LogInformation($"Alert ID: {alertData.GetProperty("alert_id")}");
        _logger.LogInformation($"Zona: {alertData.GetProperty("zone")}");
        
        return Ok(new { 
            entity = "Policia Nacional",
            status = "received",
            message = "Patrulla despachada",
            timestamp = DateTime.UtcNow 
        });
    }

    [HttpPost("cruz-roja")]
    public IActionResult DispatchToCruzRoja([FromBody] JsonElement alertData)
    {
        _logger.LogInformation("🏥 DESPACHO A CRUZ ROJA");
        _logger.LogInformation($"Alert ID: {alertData.GetProperty("alert_id")}");
        _logger.LogInformation($"Zona: {alertData.GetProperty("zone")}");
        
        return Ok(new { 
            entity = "Cruz Roja",
            status = "received",
            message = "Ambulancia medica despachada",
            timestamp = DateTime.UtcNow 
        });
    }

    [HttpPost("policia-municipal")]
    public IActionResult DispatchToPoliciaMunicipal([FromBody] JsonElement alertData)
    {
        _logger.LogInformation("🚓 DESPACHO A POLICÍA MUNICIPAL");
        _logger.LogInformation($"Alert ID: {alertData.GetProperty("alert_id")}");
        _logger.LogInformation($"Zona: {alertData.GetProperty("zone")}");
        
        return Ok(new { 
            entity = "Policia Municipal",
            status = "received",
            message = "Oficial municipal notificado",
            timestamp = DateTime.UtcNow 
        });
    }

    [HttpGet("status")]
    public IActionResult GetDispatchStatus()
    {
        return Ok(new { 
            service = "Smart City Dispatch System",
            status = "operational",
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
