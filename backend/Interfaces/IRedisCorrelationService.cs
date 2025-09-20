namespace backend.Interfaces;

using Newtonsoft.Json.Linq;

public interface IRedisCorrelationService
{
    void Process(JObject eventData);
}