# Smart City Event Processing System - AI Agent Instructions

## Architecture Overview
Event-driven smart city monitoring system using **Kafka + PostgreSQL + .NET 9**. Events flow: JS Producer → Backend API → Kafka (3 topics) → Consumers → Postgres (Neon Cloud). The system validates sensor events against JSON schemas, enriches with geolocation, publishes to specialized Kafka topics, generates correlated alerts, and persists to PostgreSQL.

### Core Components
- **Backend (.NET 9 API)**: REST API at port 5000, validates events, produces to Kafka, persists to Postgres
- **Kafka (3-Topic Architecture)**: 
  - `events.standardized` (3 partitions, 7d retention): Valid, enriched events
  - `correlated.alerts` (2 partitions, 30d retention): Generated alerts
  - `events.dlq` (1 partition, 14d retention): Failed events/validation errors
- **PostgreSQL (Neon Cloud)**: Event storage with JSONB payloads, connection via SSL
- **JS Scripts**: 
  - `producer.js`: Event generator sending to Backend API
  - `consumer.js`: Reads events.standardized, generates alerts, publishes to correlated.alerts
  - `alert-monitor.js`: Real-time alert monitoring from correlated.alerts
  - `dlq-monitor.js`: Error monitoring from events.dlq

## Event Schema Architecture

### Envelope Pattern
All events use a **canonical envelope** (`backend/Schemas/event-envelope-schema.json`) with strict validation:
```json
{
  "event_version": "1.0",
  "event_type": "panic.button|sensor.lpr|sensor.speed|sensor.acoustic|citizen.report",
  "event_id": "<uuid>",
  "producer": "string",
  "source": "simulated",
  "timestamp": "ISO 8601",
  "partition_key": "string",
  "geo": { "zone": "string", "lat": number, "lon": number },
  "severity": "info|warning|critical",
  "payload": { /* event-specific schema */ }
}
```

### Event Types & Payload Schemas
Each event type has a dedicated schema in `backend/Schemas/`:
- `panic.button`: `{ tipo_de_alerta: string }`
- `sensor.lpr`: `{ placa_vehicular: string, velocidad_estimada: number }`
- `sensor.speed`: `{ velocidad: number }`
- `sensor.acoustic`: `{ sound_type: string, decibels: number }`
- `citizen.report`: `{ tipo_evento: string, mensaje_descriptivo: string }`

**Validation Flow**: `EventValidatorService` validates envelope first, then payload against event-type-specific schema using NJsonSchema.

## Critical Workflows

### Local Development Setup
```powershell
# Start infrastructure (Kafka, Zookeeper, Backend)
docker-compose up -d

# Backend is exposed at http://localhost:5000 (Swagger UI at root)
# Kafka broker at localhost:9092
# Database: Neon Cloud (SSL required, credentials in appsettings.json)
```

### Testing Event Flow
```powershell
# Send test event via API
curl -X POST http://localhost:5000/events -H "Content-Type: application/json" -d @test-event.json

# Simulate continuous events with realistic payloads (JS producer)
cd js-scripts; npm run producer

# Consume from Kafka with intelligent multi-level alert detection
cd js-scripts; npm run consumer

# Monitor correlated alerts in real-time
cd js-scripts; npm run alert-monitor

# Monitor Dead Letter Queue for failed events
cd js-scripts; npm run dlq-monitor
```

### Alert Detection System
The consumer implements a multi-level alert system that analyzes events and generates actionable alerts:

**Alert Levels**: CRÍTICO (critical) → ALTO (high) → MEDIO (medium) → INFO (informational)

**Alert Types by Event**:
- `panic.button`: Emergency alerts (panic/fire/general) based on `tipo_de_alerta` and `user_context`
- `sensor.lpr`: Speed violations with vehicle details (plate, model, color, location)
- `sensor.speed`: Speed threshold alerts with directional context
- `sensor.acoustic`: Gunshot/explosion/glass break detection with confidence scores
- `citizen.report`: Accident/fire/altercation reports with location and origin tracking

Alerts are color-coded in console output and include geolocation, event correlation IDs, and actionable details (e.g., "Requiere bomberos", "Verificar con cámaras").

### Database Management
- **No migrations**: Database schema managed via SQL script (`database/init-neon.sql`)
- Run SQL script manually in Neon Console to initialize/reset tables
- EF Core configured for Npgsql but schema is SQL-first, not code-first

## Project-Specific Conventions

### Service Registration
All services registered as **Singletons** in `Program.cs`:
- `EventValidatorService`: Loads all JSON schemas on startup
- `KafkaProducerService`: Reuses producer instance across requests
- `EventDbContext`: Standard DbContext with Npgsql

### Geolocation Enrichment
Backend **auto-enriches** events with default "Zona 10" if geo data missing:
```csharp
geo["zone"] = geo["zone"]?.ToString() ?? "Zona 10";
geo["lat"] = geo["lat"] ?? 14.6091;
geo["lon"] = geo["lon"] ?? -90.5252;
```
This is hardcoded in `EventsController.Post()` - modify for production multi-zone support.

### Kafka Configuration
- **Internal Docker**: `kafka:29092` (container-to-container)
- **External**: `localhost:9092` (host machine)
- **Producer Config**: `Acks.All`, 5s timeout, error logging via ILogger
- **Topics**:
  - `events.standardized`: Valid events (Backend → Consumer)
  - `correlated.alerts`: Generated alerts (Consumer → Alert Monitor)
  - `events.dlq`: Failed events (Backend → DLQ Monitor)
- Fallback to `kafka:29092` if `Kafka:BootstrapServers` not configured

### Database Naming Convention
- **Tables**: Snake_case (`events`, `alerts`)
- **Columns**: Snake_case (`event_id`, `ts_utc`, `geo_lat`)
- **EF Models**: PascalCase properties mapped via `[Column]` attributes
- **Payload**: Stored as JSONB in Postgres, not as separate columns

## Integration Points

### API Endpoints
- `POST /events`: Main ingestion (returns enrichment message)
- `GET /events?take=20`: List recent events (ID, type, timestamp only)
- `POST /events/bulk`: Batch ingestion (validates each, publishes valid only)
- `GET /health`: Liveness check
- `GET /schema`: Returns all loaded schemas

### Error Handling Patterns
- **Validation errors**: 400 with detailed schema violation messages, published to `events.dlq`
- **Kafka failures**: 502 with Kafka error details, published to `events.dlq`
- **DB errors**: 500 with connection/query error messages
- Use `ValidateDetailed()` to return structured error arrays
- **Kafka failures**: 502 with Kafka error details
- **DB errors**: 500 with connection/query error messages
- Use `ValidateDetailed()` to return structured error arrays

## Common Modifications

### Adding New Event Type
1. Create schema in `backend/Schemas/<event-type>-schema.json`
2. Add to `event-envelope-schema.json` enum
3. Register in `EventValidatorService` constructor dictionary
4. Add payload generation logic to `js-scripts/producer.js`

### Changing Default Zone
Modify hardcoded values in `EventsController.Post()`:
```csharp
geo["zone"] = geo["zone"]?.ToString() ?? "YOUR_ZONE";
geo["lat"] = geo["lat"] ?? YOUR_LAT;
geo["lon"] = geo["lon"] ?? YOUR_LON;
```

### Database Schema Changes
1. Edit `database/init-neon.sql` (DROP/CREATE pattern)
2. Run SQL in Neon Console
3. Update EF models in `Services/EventDbContext.cs` to match
4. No EF migrations needed - SQL is source of truth

## Key Files Reference
- **API Entry**: `backend/Program.cs` - DI setup, Swagger config, DB health check
- **Event Ingestion**: `backend/Controllers/EventsController.cs` - enrichment logic
- **Validation**: `backend/Services/EventValidatorService.cs` - dual-layer schema validation
- **Kafka**: `backend/Services/KafkaProducerService.cs` - singleton producer with error handling
- **DB Models**: `backend/Services/EventDbContext.cs` - EF mappings for events/alerts tables
- **Test Data**: `test-event.json` - example panic.button event for Zona 10
- **Testing Guide**: `TESTING.md` - comprehensive testing strategies and deployment checklist
- **Compliance**: `COMPLIANCE.md` - verification against canonical specification v1.0
- **Alerts Reference**: `ALERTS_REFERENCE.md` - complete alert types and detection matrix
- **Environment**: `.env` - configuration for DB, Kafka, and testing parameters
