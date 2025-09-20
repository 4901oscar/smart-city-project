using backend.Interfaces;
using StackExchange.Redis;
using Newtonsoft.Json.Linq;

namespace backend.Services;


public class RedisCorrelationService : IRedisCorrelationService
{
    private readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect("localhost");
    private readonly IDatabase _db;

    public RedisCorrelationService()
    {
        _db = _redis.GetDatabase();
    }

    public void Process(JObject eventData)
    {
        string zone = eventData["geo"]?["zone"]?.ToString() ?? "Zona 10";
        string key = $"event:{zone}:{eventData["event_type"]}";
        _db.StringSet(key, eventData.ToString(), TimeSpan.FromMinutes(5));

        // Ejemplo de correlación simple
        if (_db.KeyExists($"event:{zone}:panic.button") &&
            _db.KeyExists($"event:{zone}:sensor.speed") &&
            _db.KeyExists($"event:{zone}:sensor.lpr"))
        {
            Console.WriteLine($"🚨 Posible robo detectado en {zone}");
        }
    }
}