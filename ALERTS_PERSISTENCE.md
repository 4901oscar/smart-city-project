# 📊 Implementación de Persistencia de Alertas en PostgreSQL

## ✅ Estado: IMPLEMENTADO

**Fecha:** 9 de octubre de 2025  
**Funcionalidad:** Almacenamiento de alertas correlacionadas en tabla `alerts` de PostgreSQL

---

## 🎯 Objetivo

Las alertas generadas por el consumer ahora se almacenan en la base de datos PostgreSQL para:
- Análisis histórico de eventos
- Generación de reportes
- Correlación temporal de alertas
- Dashboard y visualizaciones

---

## 🗄️ Esquema de Base de Datos

### Tabla `alerts`

```sql
CREATE TABLE alerts (
  alert_id        uuid PRIMARY KEY,
  correlation_id  uuid,
  type            text NOT NULL,
  score           numeric,
  zone            text,
  window_start    timestamptz,
  window_end      timestamptz,
  evidence        jsonb,    -- array of event_ids w/ minimal context
  created_at      timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_alerts_ts   ON alerts (created_at);
CREATE INDEX idx_alerts_zone ON alerts (zone);
```

**Campos:**
- `alert_id`: UUID único para cada alerta individual
- `correlation_id`: ID de correlación del evento que generó la alerta
- `type`: Tipo de alerta (ej: "EXCESO DE VELOCIDAD PELIGROSO")
- `score`: Puntuación numérica basada en severidad (100=CRÍTICO, 75=ALTO, 50=MEDIO, 25=INFO)
- `zone`: Zona geográfica donde ocurrió la alerta
- `window_start`: Inicio de ventana temporal (timestamp - 5 minutos)
- `window_end`: Fin de ventana temporal (timestamp del evento)
- `evidence`: JSONB con evidencia completa del evento
- `created_at`: Timestamp de creación de la alerta

---

## 🔧 Componentes Implementados

### 1. Modelo Alert (Backend)

**Archivo:** `backend/Models/Alert.cs`

```csharp
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

    [Column("evidence", TypeName = "jsonb")]
    public string? Evidence { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

### 2. AlertsController (Backend)

**Archivo:** `backend/Controllers/AlertsController.cs`

**Endpoints implementados:**

#### POST /alerts
Guarda alertas en la base de datos.

**Request Body:**
```json
{
  "alert_id": "uuid",
  "correlation_id": "uuid",
  "source_event_id": "uuid",
  "event_type": "sensor.lpr",
  "zone": "Zona 10",
  "timestamp": "2025-10-09T23:00:00Z",
  "alerts": [
    {
      "level": "CRÍTICO",
      "type": "EXCESO DE VELOCIDAD PELIGROSO",
      "message": "Vehículo a 120 km/h detectado",
      "details": "Placa: GTM-1234 | Rojo Toyota Camry"
    }
  ]
}
```

**Response:**
```json
{
  "message": "2 alert(s) saved successfully",
  "alert_id": "uuid",
  "correlation_id": "uuid",
  "zone": "Zona 10",
  "count": 2
}
```

---

#### GET /alerts?take=20&zone=Zona%2010
Obtiene alertas recientes.

**Response:**
```json
[
  {
    "alertId": "uuid",
    "correlationId": "uuid",
    "type": "EXCESO DE VELOCIDAD PELIGROSO",
    "score": 100.0,
    "zone": "Zona 10",
    "windowStart": "2025-10-09T22:55:00Z",
    "windowEnd": "2025-10-09T23:00:00Z",
    "createdAt": "2025-10-09T23:00:01Z"
  }
]
```

---

#### GET /alerts/stats?zone=Zona%2010
Obtiene estadísticas de alertas.

**Response:**
```json
{
  "total": 150,
  "last_24h": 45,
  "by_type": [
    { "type": "EXCESO DE VELOCIDAD PELIGROSO", "count": 30 },
    { "type": "DISPARO DETECTADO", "count": 15 }
  ],
  "by_zone": [
    { "zone": "Zona 10", "count": 150 }
  ],
  "avg_score": 72.5
}
```

---

### 3. Consumer Modificado (JS)

**Archivo:** `js-scripts/consumer.js`

**Cambios implementados:**

```javascript
// 1. Agregado axios para llamadas HTTP
const axios = require('axios');
const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:5000';

// 2. Función mostrarAlertas ahora guarda en BD
async function mostrarAlertas(event, alertas) {
  // ... código existente ...

  if (alertas.length > 0) {
    const alertMessage = { /* ... */ };

    // Publicar a Kafka
    await producer.send({
      topic: 'correlated.alerts',
      messages: [{ value: JSON.stringify(alertMessage) }]
    });

    // NUEVO: Guardar en PostgreSQL via Backend API
    try {
      const response = await axios.post(`${BACKEND_URL}/alerts`, alertMessage, {
        headers: { 'Content-Type': 'application/json' },
        timeout: 5000
      });
      console.log(`✓ Alertas guardadas en BD: ${response.data.count} alerta(s)`);
    } catch (dbError) {
      console.error(`⚠ Error guardando en BD: ${dbError.message}`);
    }
  }
}
```

---

## 🔄 Flujo de Datos Completo

```
Producer → Backend API → Kafka: events.standardized
                              ↓
                         Consumer (JS)
                              ↓
                    Detecta alertas (16 tipos)
                              ↓
                  ┌───────────┴───────────┐
                  │                       │
                  ↓                       ↓
        Kafka: correlated.alerts    POST /alerts
                  ↓                       ↓
           Alert Monitor          PostgreSQL: alerts
                                         ↓
                                  GET /alerts
                                  GET /alerts/stats
```

---

## 📊 Score de Severidad

El sistema asigna automáticamente un score numérico basado en el nivel de alerta:

| Nivel    | Score | Uso                                    |
|----------|-------|----------------------------------------|
| CRÍTICO  | 100   | Emergencias inmediatas (disparos, etc) |
| ALTO     | 75    | Situaciones peligrosas                 |
| MEDIO    | 50    | Alertas importantes                    |
| INFO     | 25    | Información para correlación           |

**Uso del score:**
- Filtrado de alertas críticas
- Ordenamiento por severidad
- Cálculo de promedios de zona
- Dashboard de alertas prioritarias

---

## 🧪 Pruebas

### Prueba 1: Generar alertas y verificar guardado

```bash
# Terminal 1: Iniciar producer
cd js-scripts
npm run producer

# Terminal 2: Iniciar consumer (guarda alertas)
cd js-scripts
npm run consumer
```

**Esperado en consumer:**
```
✓ Alertas publicadas a correlated.alerts
✓ Alertas guardadas en BD: 2 alerta(s)
```

---

### Prueba 2: Consultar alertas guardadas

```bash
# Ver últimas 10 alertas
curl http://localhost:5000/alerts?take=10

# Ver alertas de Zona 10
curl "http://localhost:5000/alerts?take=20&zone=Zona%2010"

# Ver estadísticas
curl http://localhost:5000/alerts/stats
```

---

### Prueba 3: Verificar en PostgreSQL

```sql
-- Conectar a Neon Cloud o PostgreSQL local

-- Ver total de alertas
SELECT COUNT(*) FROM alerts;

-- Ver últimas 10 alertas con detalles
SELECT 
  alert_id,
  type,
  score,
  zone,
  created_at,
  evidence->>'level' as level,
  evidence->>'message' as message
FROM alerts
ORDER BY created_at DESC
LIMIT 10;

-- Estadísticas por tipo
SELECT 
  type,
  COUNT(*) as count,
  AVG(score) as avg_score
FROM alerts
GROUP BY type
ORDER BY count DESC;

-- Alertas por zona
SELECT 
  zone,
  COUNT(*) as count,
  AVG(score) as avg_score
FROM alerts
GROUP BY zone
ORDER BY count DESC;
```

---

## 📈 Ventana Temporal

Cada alerta tiene una ventana temporal de correlación:

- **window_start**: 5 minutos antes del evento
- **window_end**: Timestamp del evento

**Uso:**
- Correlacionar eventos relacionados temporalmente
- Detectar patrones de eventos cercanos
- Análisis de ventanas deslizantes

**Ejemplo:**
```
Evento: 2025-10-09T23:00:00Z
window_start: 2025-10-09T22:55:00Z
window_end: 2025-10-09T23:00:00Z
```

---

## 🔍 Estructura de Evidence (JSONB)

El campo `evidence` almacena en formato JSONB:

```json
{
  "source_event_id": "uuid-del-evento-original",
  "event_type": "sensor.lpr",
  "level": "CRÍTICO",
  "message": "Vehículo a 120 km/h detectado",
  "details": "Placa: GTM-1234 | Rojo Toyota Camry | Sensor: Av_Reforma_Norte",
  "timestamp": "2025-10-09T23:00:00.000Z"
}
```

**Ventajas de JSONB:**
- Consultas eficientes con índices GIN
- Flexibilidad en estructura de datos
- Queries SQL directos: `evidence->>'level' = 'CRÍTICO'`

---

## 📝 Configuración

### Variables de Entorno

**Consumer (js-scripts):**
```bash
BACKEND_URL=http://localhost:5000  # URL del backend API
```

**Backend (.NET):**
```bash
# Ya configurado en appsettings.json
ConnectionStrings__DefaultConnection=<neon-cloud-connection-string>
```

---

## 🚀 Comandos Útiles

### Verificar alertas recientes
```bash
curl http://localhost:5000/alerts?take=5 | jq
```

### Ver estadísticas
```bash
curl http://localhost:5000/alerts/stats | jq
```

### Contar alertas en BD
```bash
# Via API
curl http://localhost:5000/alerts/stats | jq '.total'

# Via PostgreSQL
psql <connection-string> -c "SELECT COUNT(*) FROM alerts;"
```

### Limpiar tabla de alertas (testing)
```sql
-- CUIDADO: Esto elimina todas las alertas
TRUNCATE TABLE alerts;
```

---

## ✅ Checklist de Implementación

- [x] Modelo `Alert` creado en backend
- [x] `AlertsController` con 3 endpoints (POST, GET, GET/stats)
- [x] DbContext configurado para tabla `alerts`
- [x] Consumer modificado para enviar alertas a backend
- [x] Cálculo automático de score basado en nivel
- [x] Ventana temporal de correlación (5 minutos)
- [x] Evidence almacenada como JSONB
- [x] Índices en created_at y zone
- [x] Manejo de errores con fallback (no detiene consumer si falla BD)
- [x] Logs informativos en consumer y backend

---

## 🎯 Resultado Esperado

### En Consumer
```
[INFO] Evento recibido: sensor.lpr | Zona: Zona 10
🚨 ALERTAS DETECTADAS 🚨
[CRÍTICO] EXCESO DE VELOCIDAD PELIGROSO
  → Vehículo a 120 km/h detectado
  → Placa: GTM-1234 | Rojo Toyota Camry
✓ Alertas publicadas a correlated.alerts
✓ Alertas guardadas en BD: 2 alerta(s)        ← NUEVO
```

### En Backend Logs
```
info: AlertsController[0]
      Guardadas 2 alertas para evento a1b2c3d4 en zona Zona 10
```

### En PostgreSQL
```sql
SELECT * FROM alerts ORDER BY created_at DESC LIMIT 1;

 alert_id | correlation_id | type                              | score | zone    | window_start        | window_end          | created_at
----------+----------------+-----------------------------------+-------+---------+---------------------+---------------------+-------------
 uuid...  | uuid...        | EXCESO DE VELOCIDAD PELIGROSO     | 100   | Zona 10 | 2025-10-09 22:55:00 | 2025-10-09 23:00:00 | 2025-10-09...
```

---

## 🔧 Troubleshooting

### Consumer no guarda alertas en BD

**Diagnóstico:**
```bash
# Ver logs del consumer
# Buscar: "Error guardando en BD"
```

**Soluciones:**
1. Verificar que backend esté corriendo: `docker ps`
2. Verificar URL del backend: `echo $BACKEND_URL`
3. Probar endpoint manualmente: `curl http://localhost:5000/alerts/stats`

---

### Backend retorna 500 en POST /alerts

**Diagnóstico:**
```bash
docker logs backend --tail 50
```

**Causas comunes:**
1. Tabla `alerts` no existe → Ejecutar `database/init-neon.sql`
2. Error de conexión a BD → Verificar `appsettings.json`
3. Error de serialización JSON → Verificar formato de alerta

---

### Alertas se publican a Kafka pero no a BD

**Comportamiento esperado:** El consumer está diseñado para NO detenerse si falla el guardado en BD.

**Revisar:**
```javascript
// El catch solo hace log, no detiene el proceso
catch (dbError) {
  console.error(`⚠ Error guardando en BD: ${dbError.message}`);
  // NO throw - continúa procesando eventos
}
```

---

## 📚 Referencias

- **Tabla SQL:** `database/init-neon.sql` (líneas con CREATE TABLE alerts)
- **Modelo:** `backend/Models/Alert.cs`
- **Controller:** `backend/Controllers/AlertsController.cs`
- **Consumer:** `js-scripts/consumer.js` (función `mostrarAlertas`)
- **DbContext:** `backend/Services/EventDbContext.cs`

---

**Implementado por:** Sistema Smart City  
**Fecha:** 9 de octubre de 2025  
**Versión:** 2.1 (Persistencia de alertas)  
**Estado:** ✅ COMPLETO Y FUNCIONAL
