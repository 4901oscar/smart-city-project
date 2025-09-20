namespace backend.Interfaces;

using Newtonsoft.Json.Linq;

public interface IEventRepository
{
    void Save(JObject eventData);
}
