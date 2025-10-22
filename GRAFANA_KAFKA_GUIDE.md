# 🎯 Grafana Consumiendo de Kafka - Guía Completa

## 📊 Arquitectura Implementada

```
Kafka Topics
    ├── events.standardized
    ├── correlated.alerts
    └── events.dlq
           │
           ▼
    Kafka Exporter (puerto 9308)
           │ Exporta métricas
           ▼
    Prometheus (puerto 9090)
           │ Scrape cada 15s
           ▼
    Grafana (puerto 3000)
           │ Visualiza métricas
           └── Dashboards Auto-provisionados
```

## ✅ ¿Qué se ha configurado?

### 1. **Kafka Exporter**
Ya estaba corriendo en tu sistema:
```yaml
kafka-exporter:
  image: danielqsj/kafka-exporter:latest
  ports: "9308:9308"
  environment:
    KAFKA_SERVER: kafka:29092
```

**Métricas expuestas:**
- `kafka_topic_partition_current_offset` - Offset actual por topic/partición
- `kafka_consumergroup_lag` - Lag de consumer groups
- `kafka_topic_partitions` - Número de particiones
- `kafka_topic_partition_leader` - Líder de cada partición

### 2. **Prometheus**
Configurado para scrapear Kafka Exporter:
```yaml
# monitoring/prometheus/prometheus.yml
scrape_configs:
  - job_name: 'kafka-exporter'
    static_configs:
      - targets: ['kafka-exporter:9308']
```

### 3. **Grafana Datasources (Auto-provisionados)**
Creados en `monitoring/grafana/provisioning/datasources/datasources.yml`:

✅ **Prometheus** (default)
- URL: http://prometheus:9090
- Scrape interval: 15s
- **Uso:** Métricas de Kafka

✅ **PostgreSQL**
- Conexión a Neon Cloud via SSL
- **Uso:** Alertas y eventos persistidos

✅ **Elasticsearch**
- URL: http://elasticsearch:9200
- **Uso:** Logs y búsqueda de eventos

### 4. **Dashboards Pre-cargados**

#### Dashboard 1: **Kafka Monitoring**
Archivo: `monitoring/grafana/dashboards/kafka-monitoring.json`

**Paneles:**
1. **Message Rate por Topic** (Time Series)
   - Muestra eventos/segundo en cada topic
   - Query: `rate(kafka_topic_partition_current_offset[5m])`

2. **Consumer Lag** (Gauge)
   - Retraso del consumer en events.standardized
   - Query: `sum(kafka_consumergroup_lag{topic="events.standardized"})`

3. **Messages per Topic/Partition** (Pie Chart)
   - Distribución de mensajes
   - Query: `kafka_topic_partition_current_offset`

4. **Topics Status Table** (Table)
   - Estado actual de cada topic/partición
   - Offset actual por partición

5. **Consumer Group Lag Timeline** (Time Series)
   - Lag histórico de cada consumer group
   - Query: `kafka_consumergroup_lag`

#### Dashboard 2: **Alertas Dashboard**
Archivo: `monitoring/grafana/dashboards/alerts-dashboard.json`

**Paneles:**
1. **Total Alertas (24h)** - Stat
2. **Alertas CRÍTICAS (24h)** - Stat con fondo rojo
3. **Score Promedio** - Gauge
4. **Alertas Última Hora** - Stat
5. **Timeline de Alertas** - Time Series con todos los tipos
6. **Top 10 Tipos de Alerta** - Donut Chart
7. **Alertas por Zona** - Bar Chart
8. **Últimas 20 Alertas** - Table con colores por score

---

## 🚀 Cómo Acceder

### Paso 1: Abrir Grafana
```
URL: http://localhost:3000/
Usuario: admin
Password: admin
```

### Paso 2: Ver Dashboards
1. Click en el icono de **dashboards** (cuadros apilados) en la barra izquierda
2. Verás la carpeta **Smart City** con 2 dashboards:
   - **Smart City - Kafka Monitoring**
   - **Smart City - Alertas Dashboard**

### Paso 3: Explorar Métricas de Kafka

**Dashboard: Kafka Monitoring**

Aquí verás en tiempo real:
- ✅ Cuántos mensajes/segundo fluyen por cada topic
- ✅ Si el consumer tiene lag (está atrasado)
- ✅ Distribución de mensajes por partición
- ✅ Estado de salud de cada topic

**Ejemplo de lo que verás:**
```
events.standardized - partition 0:  500 mensajes
events.standardized - partition 1:  450 mensajes
events.standardized - partition 2:  520 mensajes

Consumer Lag: 0 ← ✅ Consumer al día
              50 ← ⚠️ Consumer atrasado
```

### Paso 4: Explorar Alertas

**Dashboard: Alertas Dashboard**

Verás:
- Total de alertas procesadas
- Cuántas son críticas (score = 100)
- Timeline de alertas en las últimas 24h
- Top tipos de alerta más frecuentes
- Distribución por zona

---

## 📊 Queries PromQL Útiles (Kafka)

### Ver eventos/segundo en events.standardized
```promql
rate(kafka_topic_partition_current_offset{topic="events.standardized"}[5m])
```

### Consumer lag total
```promql
sum(kafka_consumergroup_lag)
```

### Consumer lag por topic
```promql
sum by (topic) (kafka_consumergroup_lag)
```

### Total de mensajes en correlated.alerts
```promql
sum(kafka_topic_partition_current_offset{topic="correlated.alerts"})
```

### Particiones con más mensajes
```promql
topk(5, kafka_topic_partition_current_offset)
```

### Alertas en DLQ (eventos fallidos)
```promql
kafka_topic_partition_current_offset{topic="events.dlq"}
```

---

## 🎨 Crear Panel Personalizado

### Panel: "Eventos en Kafka (Último Minuto)"

1. **Ir al Dashboard** → **Add panel**

2. **Configurar Query (Prometheus):**
   ```promql
   sum(rate(kafka_topic_partition_current_offset{topic="events.standardized"}[1m])) * 60
   ```
   
3. **Configuración:**
   - Visualization: **Stat**
   - Title: "Eventos/minuto"
   - Unit: Events
   - Color: Value
   - Thresholds:
     - Green: 0-100
     - Yellow: 100-500
     - Red: 500+

4. **Guardar panel**

---

## 📈 Panel: "Alertas en Tiempo Real desde Kafka"

Aunque Kafka no persiste alertas (solo las publica), puedes ver:

### Opción 1: Ver métricas de correlated.alerts

**Query:**
```promql
rate(kafka_topic_partition_current_offset{topic="correlated.alerts"}[5m])
```

**Interpretación:**
- Si ves `0.5`, significa 0.5 alertas/segundo = 30 alertas/minuto
- Si ves `0`, no hay alertas generándose

### Opción 2: Ver alertas desde PostgreSQL

**Query (PostgreSQL):**
```sql
SELECT 
  created_at AS "time",
  type AS metric,
  score AS value
FROM alerts
WHERE created_at >= NOW() - INTERVAL '5 minutes'
ORDER BY created_at DESC
```

**Visualization:** Table o Time Series

---

## 🔔 Configurar Alertas en Grafana

### Alerta 1: Consumer Lag Alto

1. **Dashboard** → Panel "Consumer Lag" → **Alert**

2. **Configurar condición:**
   ```
   WHEN: last() of query(A)
   IS ABOVE: 100
   FOR: 5m
   ```

3. **Notification:**
   - Contact point: Email/Slack
   - Message: "⚠️ Consumer lag alto: {{$value}} mensajes pendientes"

### Alerta 2: Eventos en DLQ

**Query:**
```promql
kafka_topic_partition_current_offset{topic="events.dlq"}
```

**Condición:**
```
WHEN: increase() of query(A) over 5m
IS ABOVE: 10
```

**Message:** "🔴 {{$value}} eventos fallidos en DLQ en los últimos 5 minutos"

### Alerta 3: No hay eventos nuevos

**Query:**
```promql
rate(kafka_topic_partition_current_offset{topic="events.standardized"}[5m])
```

**Condición:**
```
WHEN: avg() of query(A)
IS BELOW: 0.01
FOR: 5m
```

**Message:** "⚠️ Sistema no está recibiendo eventos (< 1 evento/minuto)"

---

## 🎯 Variables de Dashboard

### Variable: Topic Selector

Permite filtrar por topic dinámicamente.

1. **Dashboard Settings** → **Variables** → **Add variable**

2. **Configurar:**
   ```
   Name: topic
   Type: Query
   Data source: Prometheus
   Query: label_values(kafka_topic_partition_current_offset, topic)
   Multi-value: Yes
   Include All: Yes
   ```

3. **Usar en queries:**
   ```promql
   kafka_topic_partition_current_offset{topic="$topic"}
   ```

### Variable: Consumer Group

```
Name: consumergroup
Type: Query
Data source: Prometheus
Query: label_values(kafka_consumergroup_lag, consumergroup)
```

---

## 📊 Ejemplos de Visualizaciones

### 1. Heatmap de Actividad por Hora

**Query (PostgreSQL):**
```sql
SELECT 
  DATE_TRUNC('hour', created_at) AS time,
  COUNT(*) AS value
FROM alerts
WHERE created_at >= NOW() - INTERVAL '7 days'
GROUP BY time
ORDER BY time
```

**Visualization:** Heatmap
- X-axis: Time
- Y-axis: Hour of day
- Color: Number of alerts

### 2. Gauge de Salud del Sistema

**Query (Prometheus):**
```promql
100 - (sum(kafka_consumergroup_lag) / 1000 * 100)
```

**Visualization:** Gauge
- Thresholds:
  - 90-100: Green (Saludable)
  - 70-90: Yellow (Atención)
  - 0-70: Red (Crítico)

### 3. Estadísticas de Eventos por Tipo

**Query (PostgreSQL):**
```sql
SELECT 
  type AS metric,
  COUNT(*) AS total,
  AVG(score) AS avg_score,
  MAX(score) AS max_score
FROM alerts
WHERE created_at >= NOW() - INTERVAL '24 hours'
GROUP BY type
ORDER BY total DESC
```

**Visualization:** Table con columnas coloreadas

---

## 🔧 Troubleshooting

### No veo datos en Kafka Monitoring

**Verificar:**
```bash
# 1. Kafka Exporter está corriendo
docker ps | grep kafka-exporter

# 2. Prometheus puede alcanzar Kafka Exporter
curl http://localhost:9308/metrics

# 3. Prometheus está scrapeando
curl http://localhost:9090/api/v1/targets
```

### PostgreSQL datasource no conecta

**Solución:**
1. Ir a Configuration → Data sources → Smart City PostgreSQL
2. Verificar:
   - Host: `<tu-neon-host>:5432`
   - SSL Mode: `require`
   - Password correcta
3. Click "Save & Test"

### Dashboards no aparecen

**Verificar:**
```bash
# Logs de Grafana
docker logs smart-city-project-grafana-1

# Verificar archivos montados
docker exec smart-city-project-grafana-1 ls /etc/grafana/provisioning/datasources
docker exec smart-city-project-grafana-1 ls /var/lib/grafana/dashboards
```

---

## 📱 Acceso desde Móvil

Grafana es responsive. Accede desde cualquier dispositivo:
```
http://<tu-ip-local>:3000/
```

Para acceso remoto (producción):
1. Configurar reverse proxy (nginx)
2. Habilitar HTTPS
3. Configurar autenticación OAuth

---

## 🎨 Personalización

### Cambiar Theme
- Profile → Preferences → UI Theme → **Dark** (recomendado para NOC)

### Timezone
- Profile → Preferences → Timezone → **Browser Time** o **America/Guatemala**

### Auto-refresh
- Dashboard → Refresh (top right) → **5s** para monitoreo en tiempo real

---

## 📊 Métricas Clave a Monitorear

| Métrica | Query | Threshold | Acción |
|---------|-------|-----------|--------|
| Consumer Lag | `sum(kafka_consumergroup_lag)` | > 100 | Escalar consumers |
| Message Rate | `rate(kafka_topic_partition_current_offset[5m])` | < 0.01 | Verificar producer |
| DLQ Size | `kafka_topic_partition_current_offset{topic="events.dlq"}` | > 50 | Revisar validación |
| Alertas Críticas | `SELECT COUNT(*) FROM alerts WHERE score=100` | > 20/h | Investigar patrón |

---

## 🚀 Próximos Pasos

1. **Configurar notificaciones** (Email, Slack, PagerDuty)
2. **Crear alertas automáticas** para métricas críticas
3. **Exportar dashboards** a PDF para reportes
4. **Configurar usuarios** con diferentes roles (Viewer, Editor, Admin)
5. **Integrar con Kibana** para análisis de logs

---

## 📝 Resumen de URLs

| Servicio | URL | Uso |
|----------|-----|-----|
| Grafana | http://localhost:3000/ | Dashboards y visualización |
| Prometheus | http://localhost:9090/ | Explorar métricas PromQL |
| Kafka Exporter | http://localhost:9308/metrics | Ver métricas raw de Kafka |
| Kafka UI | http://localhost:8080/ | Administrar topics |
| Backend API | http://localhost:5000/ | Ver eventos y alertas |

---

## ✅ Verificación Final

Para confirmar que todo funciona:

```bash
# 1. Grafana responde
curl http://localhost:3000/api/health

# 2. Prometheus tiene targets
curl http://localhost:9090/api/v1/targets | grep kafka-exporter

# 3. Dashboards cargados
# Ir a http://localhost:3000/ → Dashboards
# Deberías ver "Smart City - Kafka Monitoring"
```

---

**¡Ahora Grafana está consumiendo métricas de Kafka en tiempo real!** 🎉

**Creado:** 2025-10-18  
**Stack:** Kafka Exporter → Prometheus → Grafana  
**Dashboards:** 2 pre-configurados (Kafka + Alertas)  
**Refresh:** Auto 5s
