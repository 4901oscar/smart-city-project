using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace SmartCityBackend.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class SchemaController : ControllerBase
{
    /// <summary>
    /// Retorna el esquema JSON canónico actual (v1.0)
    /// </summary>
    /// <remarks>
    /// Devuelve el JSON Schema Draft 2020-12 que define el formato canónico de eventos.
    /// Los productores DEBEN validar contra este esquema antes de enviar eventos.
    /// </remarks>
    /// <returns>JSON Schema del evento canónico v1.0</returns>
    /// <response code="200">Esquema canónico retornado exitosamente</response>
    /// <response code="500">Error al leer el archivo de esquema</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get()
    {
        try
        {
            var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", "event-envelope-schema.json");
            var schemaContent = await System.IO.File.ReadAllTextAsync(schemaPath);
            return Ok(JObject.Parse(schemaContent));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                message = "Error reading canonical schema", 
                detail = ex.Message 
            });
        }
    }
}
