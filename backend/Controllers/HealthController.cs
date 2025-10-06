using Microsoft.AspNetCore.Mvc;

namespace SmartCityBackend.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Endpoint de liveness/readiness para health checks
    /// </summary>
    /// <remarks>
    /// Retorna el estado del servicio. Puede ser usado por Kubernetes, Docker Compose u orquestadores.
    /// </remarks>
    /// <returns>Estado del servicio</returns>
    /// <response code="200">Servicio operativo</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new { 
            status = "healthy",
            service = "smart-city-events-api",
            version = "1.0",
            timestamp = DateTime.UtcNow
        });
    }
}
