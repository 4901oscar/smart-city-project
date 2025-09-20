namespace backend.Interfaces;

using Newtonsoft.Json.Linq;

public interface IEventEnrichmentService
{
    JObject Enrich(JObject eventData);
}