using System;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;

namespace SmartCityBackend.Services
{
    public class ElasticsearchService
    {
        private readonly ElasticClient _client;

        public ElasticsearchService()
        {
            // URL de tu Elasticsearch (sin https, ya que desactivamos SSL)
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("events-000001")
                .PrettyJson()
                .ThrowExceptions()
                .DisableDirectStreaming();

            _client = new ElasticClient(settings);
        }

        // Método para indexar un evento genérico
        public async Task IndexEventAsync(object eventData, string indexName = "events-000001")
        {
            var response = await _client.IndexAsync(eventData, i => i
                .Index(indexName)
                .Pipeline("geo_make_location")  // usa el pipeline que ya creamos
            );

            if (!response.IsValid)
            {
                Console.WriteLine("Error al indexar documento:");
                Console.WriteLine(response.DebugInformation);
            }
            else
            {
                Console.WriteLine($"Documento indexado en '{indexName}' con ID: {response.Id}");
            }
        }
    }
}
