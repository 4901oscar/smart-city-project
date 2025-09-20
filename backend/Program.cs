<<<<<<< HEAD
=======
using backend.Interfaces;
using backend.Services;

>>>>>>> 596a44f (Servicios e interfaces)
var builder = WebApplication.CreateBuilder(args);

// Agrega servicios al contenedor
builder.Services.AddControllers();  // Habilita los controladores de API
builder.Services.AddSingleton<EventValidatorService>();
builder.Services.AddSingleton<KafkaProducerService>();
<<<<<<< HEAD
=======
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IkafkaConsumerService, KafkaConsumerService>();
builder.Services.AddSingleton<IEventValidatorService, EventValidatorService>();
builder.Services.AddSingleton<IEventEnrichmentService, EventEnrichmentService>();
builder.Services.AddSingleton<IEventRepository, EventRepository>();
builder.Services.AddSingleton<IRedisCorrelationService, RedisCorrelationService>();
builder.Services.AddSingleton<IElasticIndexerService, ElasticIndexerService>();
builder.Services.AddSingleton<IAirflowTasksService, AirflowTasksService>();
>>>>>>> 596a44f (Servicios e interfaces)

// Configura Swagger (clave para activarlo)
builder.Services.AddEndpointsApiExplorer();  // Explora los endpoints
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Smart City API", Version = "v1.0" });  // Define docs
});

var app = builder.Build();

// Configura el pipeline de solicitud
if (app.Environment.IsDevelopment())  // Solo en desarrollo (como dev profile en Spring)
{
    app.UseSwagger();  // Habilita Swagger JSON
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart City API v1.0");  // URL de Swagger
        c.RoutePrefix = string.Empty;  // Accede directamente en raíz (e.g., http://localhost:5000/)
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();;