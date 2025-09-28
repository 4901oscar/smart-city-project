using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SmartCityBackend.Services;
using Microsoft.EntityFrameworkCore; // Asegura este using para Migrate

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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Smart City API", Version = "v1" });
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

// Intentar aplicar migraciones; si no existen (porque no creaste ninguna) usar EnsureCreated como fallback SOLO dev/local
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventDbContext>();
    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARN] Migrate falló ({ex.Message}). Usando EnsureCreated como fallback.");
        try
        {
            if (!context.Database.EnsureCreated())
            {
                Console.WriteLine("[INFO] EnsureCreated indicó que la BD ya existe.");
            }
        }
        catch (Exception inner)
        {
            Console.WriteLine($"[ERROR] EnsureCreated también falló: {inner.Message}. Intentando crear tabla Events manualmente.");
            try
            {
                context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""Events"" (
    ""Id"" SERIAL PRIMARY KEY,
    ""EventType"" TEXT NULL,
    ""Payload"" TEXT NULL,
    ""Timestamp"" TIMESTAMPTZ NOT NULL
);");
                Console.WriteLine("[INFO] Tabla Events verificada/creada manualmente.");
            }
            catch (Exception ddlEx)
            {
                Console.WriteLine($"[FATAL] No se pudo crear la tabla Events: {ddlEx.Message}");
            }
        }
    }
    // Si no hay migraciones para el modelo, aseguramos la tabla Events explícitamente.
    try
    {
        context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""Events"" (
    ""Id"" SERIAL PRIMARY KEY,
    ""EventType"" TEXT NULL,
    ""Payload"" TEXT NULL,
    ""Timestamp"" TIMESTAMPTZ NOT NULL
);");
        Console.WriteLine("[INFO] Verificación/creación explícita de tabla Events completada.");
    }
    catch (Exception finalDdlEx)
    {
        Console.WriteLine($"[ERROR] Verificación de tabla Events falló: {finalDdlEx.Message}");
    }
}

app.Run();