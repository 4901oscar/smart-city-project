namespace backend.Interfaces;

using Newtonsoft.Json.Linq;

public interface IElasticIndexerService
{
    void Index(JObject eventData);
}