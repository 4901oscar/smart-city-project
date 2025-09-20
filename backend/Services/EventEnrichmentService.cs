//agregara metadata faltante (zona, longitud y latitud)

using backend.Interfaces;
using Newtonsoft.Json.Linq;


namespace backend.Services;

public class EventEnrichmentService : IEventEnrichmentService
{

    public JObject Enrich(JObject eventData)
    {
        eventData["timestamp"] ??= DateTime.UtcNow.ToString("o");
        var geo = eventData["geo"] as JObject ?? new JObject();
        geo["zone"] ??= "Zona 10";
        geo["lat"] ??= 14.6091;
        geo["lon"] ??= -90.5252;
        eventData["geo"] = geo;

        return eventData;
    }

}