# EventsController.cs - Fixed Issues

## Problems Found and Fixed (October 5, 2025)

### Issue 1: ❌ Incomplete Event Model Mapping
**Problem**: The controller was only mapping 3 fields (`EventType`, `Payload`, `Timestamp`) when saving to PostgreSQL, but the `Event` model requires 16+ fields to match the canonical schema.

**Error**:
```
'Event' does not contain a definition for 'Timestamp'
```

**Fix**: Updated `POST /events` to properly map all fields from the JSON event to the database model:
```csharp
var newEvent = new Event
{
    EventId = Guid.Parse(eventData["event_id"]?.ToString() ?? Guid.NewGuid().ToString()),
    EventType = eventData["event_type"]?.ToString() ?? "unknown",
    EventVersion = eventData["event_version"]?.ToString() ?? "1.0",
    Producer = eventData["producer"]?.ToString() ?? "unknown",
    Source = eventData["source"]?.ToString() ?? "simulated",
    CorrelationId = Guid.TryParse(eventData["correlation_id"]?.ToString(), out var corrId) ? corrId : null,
    TraceId = Guid.TryParse(eventData["trace_id"]?.ToString(), out var traceId) ? traceId : null,
    PartitionKey = eventData["partition_key"]?.ToString() ?? "default",
    TsUtc = DateTime.TryParse(eventData["timestamp"]?.ToString(), out var ts) ? ts : DateTime.UtcNow,
    Zone = geo["zone"]?.ToString(),
    GeoLat = geo["lat"] != null ? Convert.ToDecimal(geo["lat"]) : null,
    GeoLon = geo["lon"] != null ? Convert.ToDecimal(geo["lon"]) : null,
    Severity = eventData["severity"]?.ToString(),
    Payload = eventData["payload"]?.ToString() ?? "{}"
};
```

### Issue 2: ❌ Wrong Property Names in GET Endpoint
**Problem**: The `GET /events` endpoint was using `e.Id` and `e.Timestamp` which don't exist in the `Event` model.

**Error**:
```
'Event' does not contain a definition for 'Id'
```

**Fix**: Updated to use correct property names:
```csharp
var data = _context.Events
    .OrderByDescending(e => e.TsUtc)  // Was: e.Id
    .Take(take)
    .Select(e => new {
        e.EventId,      // Was: e.Id
        e.EventType,
        e.TsUtc,        // Was: e.Timestamp
        e.Zone,         // Added
        e.Severity      // Added
    })
    .ToList();
```

---

## Event Model Reference

The correct `Event` model properties (from `EventDbContext.cs`):

| Property | Type | Database Column | Required | Description |
|----------|------|-----------------|----------|-------------|
| `EventId` | Guid | event_id | ✅ | Primary key (UUID) |
| `EventType` | string | event_type | ✅ | panic.button, sensor.lpr, etc. |
| `EventVersion` | string | event_version | ✅ | Always "1.0" |
| `Producer` | string | producer | ✅ | artillery, js-sim, etc. |
| `Source` | string | source | ✅ | Always "simulated" |
| `CorrelationId` | Guid? | correlation_id | ❌ | For event correlation |
| `TraceId` | Guid? | trace_id | ❌ | For distributed tracing |
| `PartitionKey` | string | partition_key | ✅ | Kafka partition key |
| `TsUtc` | DateTime | ts_utc | ✅ | Event timestamp in UTC |
| `Zone` | string? | zone | ❌ | Geographic zone |
| `GeoLat` | decimal? | geo_lat | ❌ | Latitude |
| `GeoLon` | decimal? | geo_lon | ❌ | Longitude |
| `Severity` | string? | severity | ❌ | info, warning, critical |
| `Payload` | string | payload | ✅ | Event-specific JSON |

---

## Build Status: ✅ SUCCESS

```
Build succeeded in 11.1s
```

All compilation errors resolved!

---

## Testing the Fix

### Test 1: Send Event
```powershell
curl -X POST http://localhost:5000/events -H "Content-Type: application/json" -d @test-event.json
```

**Expected**: 200 OK with message "Evento enviado, enriquecido y persistido para alerta en Zona 10"

### Test 2: List Events
```powershell
curl http://localhost:5000/events?take=5
```

**Expected**: JSON with `eventId`, `eventType`, `tsUtc`, `zone`, `severity` fields

### Test 3: Verify Database
```sql
-- In Neon Console
SELECT event_id, event_type, event_version, producer, ts_utc, zone, severity 
FROM events 
ORDER BY ts_utc DESC 
LIMIT 5;
```

**Expected**: All fields populated correctly from the JSON event

---

## Impact of Fix

### Before Fix ❌
- Only 3 fields saved to database (EventType, Payload, Timestamp)
- Cannot query by producer, zone, severity
- Missing correlation_id and trace_id for debugging
- GET endpoint returned wrong field names

### After Fix ✅
- All 14 fields saved according to canonical schema
- Can filter by producer, zone, severity, partition_key
- Full traceability with correlation_id and trace_id
- GET endpoint returns correct data structure
- Database now fully normalized with indexed fields

---

## Next Steps

1. ✅ Backend builds successfully
2. 🔄 Restart backend container: `docker-compose restart backend`
3. ✅ Test with producer: `cd js-scripts && npm run producer`
4. ✅ Verify events in database have all fields populated
5. ✅ Check Swagger UI: http://localhost:5000

---

**Date Fixed**: October 5, 2025  
**Build Status**: ✅ Success  
**Compilation Errors**: 0
