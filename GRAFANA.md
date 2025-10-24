# 📊 Grafana - Monitoreo y Visualización

## 🎯 Introducción

Grafana proporciona dashboards interactivos para monitorear el sistema Smart City en tiempo real.

**URL**: http://localhost:3000  
**Credenciales**: admin / admin

---

## 🔧 Fuentes de Datos Configuradas

### 1. PostgreSQL (Eventos y Alertas)

**Configuración**:
- Host: `arqui-pg.postgres.database.azure.com`
- Database: `SmartCitiesBD`
- User: `grupo2`
- SSL Mode: Require

**Tablas disponibles**:
- `events` - Eventos brutos con payload JSONB
- `alerts` - Alertas correlacionadas

**Ejemplo de query**:
```sql
SELECT 
  event_type,
  severity,
  zone,
  COUNT(*) as total
FROM events
WHERE ts_utc >= NOW() - INTERVAL '1 hour'
GROUP BY event_type, severity, zone
ORDER BY total DESC;
```

### 2. Prometheus (Métricas de Kafka)

**Configuración**:
- URL: http://prometheus:9090
- Scrape interval: 15s

**Métricas exportadas por Kafka Exporter**:
- `kafka_topic_partition_current_offset` - Offset actual de cada partición
- `kafka_topic_partition_oldest_offset` - Offset más antiguo
- `kafka_consumergroup_lag` - Retraso de grupos de consumidores
- `kafka_brokers` - Número de brokers activos

**Ejemplo de query**:
```promql
# Rate de mensajes por segundo en events.standardized
rate(kafka_topic_partition_current_offset{topic="events.standardized"}[1m])

# Consumer lag total
sum(kafka_consumergroup_lag) by (consumergroup)
```

### 3. Elasticsearch (Búsqueda de Eventos)

**Configuración**:
- URL: http://elasticsearch:9200
- Índices: `events*`, `alerts*`

**Uso**: Para búsquedas de texto completo y análisis de payloads JSONB.

---

## 📈 Dashboards Recomendados

### Dashboard 1: Eventos en Tiempo Real

**Paneles sugeridos**:

1. **Eventos por Tipo** (Pie Chart)
```sql
SELECT event_type, COUNT(*) as total
FROM events
WHERE ts_utc >= NOW() - INTERVAL '5 minutes'
GROUP BY event_type;
```

2. **Eventos por Severidad** (Stat Panel)
```sql
SELECT severity, COUNT(*) as total
FROM events
WHERE ts_utc >= NOW() - INTERVAL '1 hour'
GROUP BY severity;
```

3. **Timeline de Eventos** (Graph)
```sql
SELECT 
  $__timeGroup(ts_utc, '1m') as time,
  COUNT(*) as "eventos"
FROM events
WHERE $__timeFilter(ts_utc)
GROUP BY time
ORDER BY time;
```

4. **Mapa de Calor por Zona** (Table)
```sql
SELECT 
  zone,
  event_type,
  COUNT(*) as total
FROM events
WHERE ts_utc >= NOW() - INTERVAL '1 hour'
GROUP BY zone, event_type
ORDER BY total DESC
LIMIT 20;
```

### Dashboard 2: Alertas y Despachos

1. **Alertas Críticas** (Stat Panel + Color Threshold)
```sql
SELECT COUNT(*) as total
FROM alerts
WHERE window_start >= NOW() - INTERVAL '1 hour'
  AND type LIKE '%CRÍTICO%' OR type LIKE '%DISPARO%' OR type LIKE '%INCENDIO%';
```

2. **Alertas por Tipo** (Bar Gauge)
```sql
SELECT type, COUNT(*) as total
FROM alerts
WHERE window_start >= NOW() - INTERVAL '6 hours'
GROUP BY type
ORDER BY total DESC
LIMIT 10;
```

3. **Score Promedio por Zona** (Heatmap)
```sql
SELECT 
  zone,
  AVG(score) as avg_score,
  COUNT(*) as total_alerts
FROM alerts
WHERE window_start >= NOW() - INTERVAL '24 hours'
GROUP BY zone
ORDER BY avg_score DESC;
```

### Dashboard 3: Kafka Metrics (Prometheus)

1. **Mensajes por Segundo** (Graph)
```promql
sum(rate(kafka_topic_partition_current_offset[1m])) by (topic)
```

2. **Consumer Lag** (Graph)
```promql
sum(kafka_consumergroup_lag) by (consumergroup, topic)
```

3. **Partitions por Topic** (Stat Panel)
```promql
count(kafka_topic_partition_current_offset) by (topic)
```

4. **Broker Health** (Stat Panel)
```promql
kafka_brokers
```

---

## 🚨 Alertas en Grafana

### Configurar Alerta de Eventos Críticos

1. Ir a panel "Alertas Críticas"
2. Click en "Alert" tab
3. Configurar:
   - **Evaluate every**: 1m
   - **For**: 2m
   - **Condition**: `WHEN last() OF query(A) IS ABOVE 5`

4. Notification:
   - **Send to**: Email / Slack / Webhook
   - **Message**: "⚠️ Múltiples alertas críticas detectadas"

### Configurar Alerta de Consumer Lag

```promql
# Query
sum(kafka_consumergroup_lag{topic="events.standardized"}) > 1000

# Condición
WHEN last() IS ABOVE 1000 FOR 5m

# Mensaje
"🚨 Consumer lag crítico: {value} mensajes pendientes"
```

---

## 🎨 Tips de Visualización

### Variables de Dashboard

Crear variables para filtros dinámicos:

1. **$zona** - Variable de zona
```sql
SELECT DISTINCT zone FROM events ORDER BY zone;
```

2. **$event_type** - Variable de tipo de evento
```sql
SELECT DISTINCT event_type FROM events ORDER BY event_type;
```

3. **$time_range** - Variable de rango temporal
- Opciones: Last 5m, Last 1h, Last 6h, Last 24h

### Color Thresholds

**Severity Colors**:
- `critical` → Red (#E02F44)
- `warning` → Orange (#FF9830)
- `info` → Green (#73BF69)

**Score Thresholds**:
- 0-30 → Green
- 31-70 → Orange
- 71-100 → Red

---

## 📊 Panel Ejemplos Avanzados

### Panel: Top 10 Zonas con Más Eventos

```sql
SELECT 
  zone,
  COUNT(*) as total_eventos,
  SUM(CASE WHEN severity = 'critical' THEN 1 ELSE 0 END) as criticos,
  SUM(CASE WHEN severity = 'warning' THEN 1 ELSE 0 END) as warnings,
  SUM(CASE WHEN severity = 'info' THEN 1 ELSE 0 END) as info
FROM events
WHERE ts_utc >= NOW() - INTERVAL '1 hour'
GROUP BY zone
ORDER BY total_eventos DESC
LIMIT 10;
```

**Tipo**: Table  
**Transformations**: 
- Add field from calculation: `percentage = (total_eventos / sum(total_eventos)) * 100`

### Panel: Eventos por Hora del Día

```sql
SELECT 
  EXTRACT(HOUR FROM ts_utc) as hora,
  COUNT(*) as total
FROM events
WHERE ts_utc >= NOW() - INTERVAL '24 hours'
GROUP BY hora
ORDER BY hora;
```

**Tipo**: Bar Chart  
**X-axis**: hora  
**Y-axis**: total

### Panel: Alertas con Mayor Score

```sql
SELECT 
  type,
  zone,
  score,
  window_start,
  alert_id
FROM alerts
WHERE window_start >= NOW() - INTERVAL '6 hours'
ORDER BY score DESC
LIMIT 20;
```

**Tipo**: Table  
**Column Styles**:
- score → Color background (gradient red)
- type → Link to alert details

---

## 🔍 Exploración de Datos

### Query para Debugging

```sql
-- Ver payload JSONB de eventos críticos
SELECT 
  event_id,
  event_type,
  severity,
  zone,
  payload::text as payload_json
FROM events
WHERE severity = 'critical'
  AND ts_utc >= NOW() - INTERVAL '1 hour'
ORDER BY ts_utc DESC
LIMIT 50;
```

### Análisis de Payloads

```sql
-- Extraer campos específicos del payload
SELECT 
  event_type,
  zone,
  payload->>'placa_vehicular' as placa,
  payload->>'velocidad_estimada' as velocidad,
  payload->>'tipo_sonido_detectado' as sonido,
  ts_utc
FROM events
WHERE event_type IN ('sensor.lpr', 'sensor.acoustic')
  AND ts_utc >= NOW() - INTERVAL '2 hours'
ORDER BY ts_utc DESC;
```

---

## 🛠️ Comandos Útiles

### Verificar Conexión a PostgreSQL

```sql
-- Test query simple
SELECT COUNT(*) as total_eventos FROM events;

-- Verificar últimos eventos
SELECT * FROM events ORDER BY ts_utc DESC LIMIT 5;
```

### Verificar Métricas de Prometheus

```bash
# Ver métricas disponibles
curl http://localhost:9090/api/v1/label/__name__/values | jq

# Query específica
curl 'http://localhost:9090/api/v1/query?query=kafka_brokers'
```

### Exportar Dashboard

1. Ir a Dashboard Settings (⚙️)
2. JSON Model
3. Copiar JSON
4. Guardar en `grafana/dashboards/my-dashboard.json`

---

## 📚 Recursos Adicionales

- **Grafana Docs**: https://grafana.com/docs/
- **PromQL Guide**: https://prometheus.io/docs/prometheus/latest/querying/basics/
- **PostgreSQL Functions**: https://www.postgresql.org/docs/current/functions.html

---

## 🎯 Próximos Pasos

1. ✅ Conectar fuentes de datos
2. 📊 Crear dashboards personalizados
3. 🚨 Configurar alertas críticas
4. 📈 Analizar tendencias históricas
5. 🎨 Personalizar visualizaciones
6. 💾 Exportar y versionar dashboards

**Estado**: Grafana completamente funcional con 3 fuentes de datos conectadas ✅
