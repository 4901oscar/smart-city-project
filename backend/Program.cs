using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SmartCityBackend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Agrega servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<EventValidatorService>();
builder.Services.AddSingleton<KafkaProducerService>();

// Configura Swagger para documentación y pruebas de APIs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Smart City Events API", 
        Version = "v1",
        Description = "API para el sistema de eventos de Smart City. Permite recibir y procesar diferentes tipos de eventos urbanos.",
        Contact = new OpenApiContact
        {
            Name = "Smart City Team",
            Email = "dev@smartcity.com"
        }
    });
    
    // Habilitar comentarios XML si existe el archivo
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configura el pipeline de solicitud HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart City API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Verificar conexión a la base de datos
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventDbContext>();
    try
    {
        // Verificar si podemos conectarnos a la base de datos
        var canConnect = context.Database.CanConnect();
        if (canConnect)
        {
            Console.WriteLine("[INFO] ✓ Conexión a la base de datos establecida correctamente");
            
            // Contar eventos existentes
            var eventCount = context.Events.Count();
            Console.WriteLine($"[INFO] Total de eventos en la base de datos: {eventCount}");
        }
        else
        {
            Console.WriteLine("[WARN] ⚠ No se pudo conectar a la base de datos");
            Console.WriteLine("[WARN] Verifica tu conexión a Neon o ejecuta: .\\init-database.ps1");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] ✗ Error conectando a la base de datos: {ex.Message}");
        Console.WriteLine("[INFO] Asegúrate de que las tablas estén creadas en Neon");
        Console.WriteLine("[INFO] Ejecuta el script: .\\init-database.ps1");
    }
}

app.Run();