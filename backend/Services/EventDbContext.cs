using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCityBackend.Services;

public class EventDbContext : DbContext
{
    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Alert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.EventType).HasColumnName("event_type").IsRequired();
            entity.Property(e => e.EventVersion).HasColumnName("event_version").IsRequired();
            entity.Property(e => e.Producer).HasColumnName("producer").IsRequired();
            entity.Property(e => e.Source).HasColumnName("source").IsRequired();
            entity.Property(e => e.CorrelationId).HasColumnName("correlation_id");
            entity.Property(e => e.TraceId).HasColumnName("trace_id");
            entity.Property(e => e.PartitionKey).HasColumnName("partition_key").IsRequired();
            entity.Property(e => e.TsUtc).HasColumnName("ts_utc").IsRequired();
            entity.Property(e => e.Zone).HasColumnName("zone");
            entity.Property(e => e.GeoLat).HasColumnName("geo_lat").HasColumnType("numeric");
            entity.Property(e => e.GeoLon).HasColumnName("geo_lon").HasColumnType("numeric");
            entity.Property(e => e.Severity).HasColumnName("severity");
            entity.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
            
            entity.HasIndex(e => e.TsUtc).HasDatabaseName("idx_events_ts");
            entity.HasIndex(e => e.EventType).HasDatabaseName("idx_events_type");
            entity.HasIndex(e => e.Zone).HasDatabaseName("idx_events_zone");
            entity.HasIndex(e => e.PartitionKey).HasDatabaseName("idx_events_pkey");
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.ToTable("alerts");
            entity.HasKey(e => e.AlertId);
            entity.Property(e => e.AlertId).HasColumnName("alert_id");
            entity.Property(e => e.CorrelationId).HasColumnName("correlation_id");
            entity.Property(e => e.Type).HasColumnName("type").IsRequired();
            entity.Property(e => e.Score).HasColumnName("score").HasColumnType("numeric");
            entity.Property(e => e.Zone).HasColumnName("zone");
            entity.Property(e => e.WindowStart).HasColumnName("window_start");
            entity.Property(e => e.WindowEnd).HasColumnName("window_end");
            entity.Property(e => e.Evidence).HasColumnName("evidence").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_alerts_ts");
            entity.HasIndex(e => e.Zone).HasDatabaseName("idx_alerts_zone");
        });
    }
}

[Table("events")]
public class Event
{
    [Key]
    [Column("event_id")]
    public Guid EventId { get; set; }

    [Required]
    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [Column("event_version")]
    public string EventVersion { get; set; } = string.Empty;

    [Required]
    [Column("producer")]
    public string Producer { get; set; } = string.Empty;

    [Required]
    [Column("source")]
    public string Source { get; set; } = string.Empty;

    [Column("correlation_id")]
    public Guid? CorrelationId { get; set; }

    [Column("trace_id")]
    public Guid? TraceId { get; set; }

    [Required]
    [Column("partition_key")]
    public string PartitionKey { get; set; } = string.Empty;

    [Required]
    [Column("ts_utc")]
    public DateTime TsUtc { get; set; }

    [Column("zone")]
    public string? Zone { get; set; }

    [Column("geo_lat")]
    public decimal? GeoLat { get; set; }

    [Column("geo_lon")]
    public decimal? GeoLon { get; set; }

    [Column("severity")]
    public string? Severity { get; set; }

    [Required]
    [Column("payload")]
    public string Payload { get; set; } = string.Empty; // JSON como string
}

[Table("alerts")]
public class Alert
{
    [Key]
    [Column("alert_id")]
    public Guid AlertId { get; set; }

    [Column("correlation_id")]
    public Guid? CorrelationId { get; set; }

    [Required]
    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("score")]
    public decimal? Score { get; set; }

    [Column("zone")]
    public string? Zone { get; set; }

    [Column("window_start")]
    public DateTime? WindowStart { get; set; }

    [Column("window_end")]
    public DateTime? WindowEnd { get; set; }

    [Column("evidence")]
    public string? Evidence { get; set; } // JSON como string

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}