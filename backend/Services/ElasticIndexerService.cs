using backend.Interfaces;
using Elasticsearch.Net;
using Newtonsoft.Json.Linq;
using System.Text;

namespace backend.Services;

public class ElasticIndexerService : IElasticIndexerService
{
    private readonly ElasticLowLevelClient _client;

    public ElasticIndexerService()
    {
        var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"));
        _client = new ElasticLowLevelClient(settings);
    }

    public void Index(JObject eventData)
    {
        string indexName = "eventos";
        string id = eventData["event_id"]?.ToString();
        var json = Encoding.UTF8.GetBytes(eventData.ToString());

        _client.Index<BytesResponse>(indexName, id, PostData.Bytes(json));
    }
}