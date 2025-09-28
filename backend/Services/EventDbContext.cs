using Microsoft.EntityFrameworkCore;

namespace SmartCityBackend.Services;

public class EventDbContext : DbContext
{
    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=postgres;Database=SmartCitiesBD;Username=postgres;Password=supersecretpassword;SSL Mode=Disable");
        }
    }
}

public class Event
{
    public int Id { get; set; }
    public string? EventType { get; set; }  // Nullable
    public string? Payload { get; set; }    // Nullable
    public DateTime Timestamp { get; set; }
}