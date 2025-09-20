namespace backend.Interfaces;

using Newtonsoft.Json.Linq;

public interface IEventValidatorService
{
    bool Validate(JObject payload);
}