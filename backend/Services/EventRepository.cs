using backend.Interfaces;
using Npgsql;
using Newtonsoft.Json.Linq;


namespace backend.Services;

public class EventRepository : IEventRepository
{
    private readonly string _connectionString = "Host=localhost;Username=postgres;Password=tu_clave;Database=eventos";

    public void Save(JObject eventData)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("INSERT INTO eventos (event_id, event_type, timestamp, zone, severity, payload) VALUES (@id, @type, @ts, @zone, @sev, @payload)", conn);
        cmd.Parameters.AddWithValue("id", eventData["event_id"]?.ToString());
        cmd.Parameters.AddWithValue("type", eventData["event_type"]?.ToString());
        cmd.Parameters.AddWithValue("ts", DateTime.Parse(eventData["timestamp"]?.ToString() ?? DateTime.UtcNow.ToString()));
        cmd.Parameters.AddWithValue("zone", eventData["geo"]?["zone"]?.ToString() ?? "Zona 10");
        cmd.Parameters.AddWithValue("sev", eventData["severity"]?.ToString());
        cmd.Parameters.AddWithValue("payload", eventData["payload"]?.ToString());

        cmd.ExecuteNonQuery();
    }
}