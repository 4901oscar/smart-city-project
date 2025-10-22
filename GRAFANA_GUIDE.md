# 📊 Guía Completa de Grafana - Smart City Monitoring

## 🌐 Acceso a Grafana

**URL:** http://localhost:3000/

### Credenciales por Defecto
- **Usuario:** `admin`
- **Password:** `admin`

> 💡 En el primer login, Grafana te pedirá cambiar la contraseña. Puedes saltarlo si es desarrollo local.

---

## 🎯 ¿Qué es Grafana?

Grafana es una plataforma de **visualización y monitoreo** que te permite:
- 📈 Crear dashboards interactivos
- 📊 Visualizar métricas en tiempo real
- 🚨 Configurar alertas
- 📉 Analizar tendencias históricas
- 🔍 Explorar logs y eventos

---

## 🔧 Configuración Inicial

### Paso 1: Conectar PostgreSQL (Base de Datos de Alertas)

1. **Ir a Configuración**
   - Click en el icono de engranaje ⚙️ (izquierda)
   - Seleccionar **Data sources**

2. **Agregar PostgreSQL**
   - Click en **Add data source**
   - Buscar y seleccionar **PostgreSQL**

3. **Configurar conexión a Neon Cloud**
   ```
   Name: Smart City PostgreSQL
   Host: <tu-host-neon>.neon.tech:5432
   Database: SmartCitiesBD
   User: <tu-usuario-neon>
   Password: <tu-password-neon>
   TLS/SSL Mode: require
   ```

4. **Probar conexión**
   - Click en **Save & Test**
   - Deberías ver: ✅ "Database Connection OK"

### Paso 2: Conectar Prometheus (Métricas del Sistema)

1. **Agregar Prometheus**
   - Data sources → **Add data source**
   - Seleccionar **Prometheus**

2. **Configurar**
   ```
   Name: Prometheus
   URL: http://prometheus:9090
   Access: Server (default)
   ```

3. **Guardar**
   - Click en **Save & Test**

### Paso 3: Conectar Elasticsearch (Logs y Eventos)

1. **Agregar Elasticsearch**
   - Data sources → **Add data source**
   - Seleccionar **Elasticsearch**

2. **Configurar**
   ```
   Name: Elasticsearch
   URL: http://elasticsearch:9200
   Index name: events-*
   Time field name: timestamp
   Version: 8.x
   ```

3. **Guardar**
   - Click en **Save & Test**

---

## 📊 Crear tu Primer Dashboard

### Dashboard de Alertas en Tiempo Real

1. **Crear Dashboard**
   - Click en ➕ (izquierda)
   - Seleccionar **Dashboard**
   - Click en **Add new panel**

2. **Panel 1: Total de Alertas por Severidad**
   
   **Query (PostgreSQL):**
   ```sql
   SELECT 
     type AS metric,
     COUNT(*) AS value
   FROM alerts
   WHERE created_at >= NOW() - INTERVAL '24 hours'
   GROUP BY type
   ORDER BY value DESC
   ```

   **Configuración:**
   - Visualization: **Bar chart** o **Pie chart**
   - Title: "Alertas por Tipo (Últimas 24h)"

3. **Panel 2: Alertas Críticas en el Tiempo**
   
   **Query (PostgreSQL):**
   ```sql
   SELECT 
     created_at AS time,
     score AS value,
     type AS metric
   FROM alerts
   WHERE score >= 75
   ORDER BY created_at
   ```

   **Configuración:**
   - Visualization: **Time series**
   - Title: "Alertas Críticas - Timeline"
   - Y-axis: Score

4. **Panel 3: Distribución por Zona**
   
   **Query (PostgreSQL):**
   ```sql
   SELECT 
     zone AS metric,
     COUNT(*) AS value,
     AVG(score) AS avg_score
   FROM alerts
   WHERE created_at >= NOW() - INTERVAL '7 days'
   GROUP BY zone
   ```

   **Configuración:**
   - Visualization: **Stat** o **Gauge**
   - Title: "Alertas por Zona"

5. **Panel 4: Contador de Alertas en Tiempo Real**
   
   **Query (PostgreSQL):**
   ```sql
   SELECT COUNT(*) AS value
   FROM alerts
   WHERE created_at >= NOW() - INTERVAL '1 hour'
   ```

   **Configuración:**
   - Visualization: **Stat**
   - Title: "Alertas Última Hora"
   - Color mode: Value
   - Thresholds: 
     - Green: 0-10
     - Yellow: 10-50
     - Red: 50+

---

## 🎨 Dashboards Pre-configurados Recomendados

### Dashboard 1: Vista General del Sistema

**Paneles:**
1. **Total Eventos Procesados** (Stat)
2. **Alertas Activas** (Stat con color)
3. **Timeline de Alertas** (Time series)
4. **Mapa de Calor por Hora** (Heatmap)
5. **Top 5 Tipos de Alerta** (Bar chart)
6. **Distribución de Severidad** (Pie chart)

### Dashboard 2: Análisis de Alertas

**Query ejemplo - Alertas por hora:**
```sql
SELECT 
  DATE_TRUNC('hour', created_at) AS time,
  COUNT(*) AS total_alertas,
  SUM(CASE WHEN score = 100 THEN 1 ELSE 0 END) AS criticas,
  SUM(CASE WHEN score = 75 THEN 1 ELSE 0 END) AS altas,
  SUM(CASE WHEN score = 50 THEN 1 ELSE 0 END) AS medias
FROM alerts
WHERE created_at >= NOW() - INTERVAL '24 hours'
GROUP BY time
ORDER BY time
```

### Dashboard 3: Monitoreo de Infraestructura (Prometheus)

**Paneles:**
1. **CPU Usage** - `rate(process_cpu_seconds_total[5m])`
2. **Memory Usage** - `process_resident_memory_bytes`
3. **HTTP Request Rate** - `rate(http_requests_total[5m])`
4. **Kafka Lag** - `kafka_consumer_lag`

---

## 🚨 Configurar Alertas

### Alerta 1: Alertas Críticas Excesivas

1. **Crear Alert Rule**
   - Dashboard → Panel → Alert tab
   - Click en **Create alert rule from this panel**

2. **Configurar Condición**
   ```
   WHEN: COUNT of query(A)
   IS ABOVE: 10
   FOR: 5m
   ```

3. **Configurar Notificación**
   - Contact point: Email, Slack, o Webhook
   - Message: "⚠️ Más de 10 alertas críticas en 5 minutos en Zona 10"

### Alerta 2: Sistema Sin Eventos

```sql
SELECT COUNT(*) 
FROM events 
WHERE timestamp >= NOW() - INTERVAL '5 minutes'
```

**Condición:**
- WHEN: `IS BELOW 1`
- FOR: `5m`
- Message: "🔴 Sistema no está recibiendo eventos"

---

## 📈 Queries SQL Útiles para Grafana

### 1. Tendencia de Alertas (7 días)
```sql
SELECT 
  DATE_TRUNC('day', created_at) AS time,
  COUNT(*) AS alertas
FROM alerts
WHERE created_at >= NOW() - INTERVAL '7 days'
GROUP BY time
ORDER BY time
```

### 2. Score Promedio por Día
```sql
SELECT 
  DATE_TRUNC('day', created_at) AS time,
  AVG(score) AS avg_score,
  MAX(score) AS max_score,
  MIN(score) AS min_score
FROM alerts
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY time
ORDER BY time
```

### 3. Top Alertas Más Frecuentes
```sql
SELECT 
  type,
  COUNT(*) AS count,
  AVG(score) AS avg_score
FROM alerts
WHERE created_at >= NOW() - INTERVAL '24 hours'
GROUP BY type
ORDER BY count DESC
LIMIT 10
```

### 4. Ventana de Correlación Activa
```sql
SELECT 
  correlation_id,
  MIN(window_start) AS start,
  MAX(window_end) AS end,
  COUNT(*) AS eventos_correlacionados,
  STRING_AGG(type, ', ') AS tipos
FROM alerts
WHERE correlation_id IS NOT NULL
GROUP BY correlation_id
ORDER BY start DESC
LIMIT 20
```

### 5. Eventos por Tipo (Time Series)
```sql
SELECT 
  $__timeGroup(created_at, '5m') AS time,
  type AS metric,
  COUNT(*) AS value
FROM alerts
WHERE $__timeFilter(created_at)
GROUP BY time, type
ORDER BY time
```

> 💡 **Nota:** `$__timeGroup` y `$__timeFilter` son variables especiales de Grafana.

---

## 🎨 Variables de Dashboard

### Crear Variable de Zona

1. **Dashboard Settings** → **Variables** → **Add variable**
2. **Configurar:**
   ```
   Name: zona
   Type: Query
   Data source: Smart City PostgreSQL
   Query: SELECT DISTINCT zone FROM alerts
   Multi-value: Yes
   Include All option: Yes
   ```

3. **Usar en queries:**
   ```sql
   SELECT * FROM alerts
   WHERE zone = '$zona'
   ```

### Variable de Rango de Tiempo
```
Name: timeRange
Type: Interval
Values: 5m,15m,1h,6h,24h,7d
```

---

## 🔍 Explorar Logs con Elasticsearch

### Panel de Logs en Tiempo Real

1. **Crear Panel**
   - Data source: **Elasticsearch**
   - Visualization: **Logs**

2. **Query (Lucene):**
   ```
   event_type:* AND severity:critical
   ```

3. **Filtros:**
   - Time field: `timestamp`
   - Log level field: `severity`

---

## 🎯 Dashboard Completo Recomendado

### Row 1: KPIs
```
[Total Eventos] [Alertas Críticas] [Alertas Última Hora] [Score Promedio]
```

### Row 2: Tendencias
```
[Timeline de Alertas (Time Series - 24h)]
```

### Row 3: Distribuciones
```
[Por Tipo (Pie)]  [Por Zona (Bar)]  [Por Severidad (Donut)]
```

### Row 4: Análisis
```
[Eventos por Hora (Heatmap)]  [Top 10 Alertas (Table)]
```

### Row 5: Logs
```
[Logs en Tiempo Real (Logs panel - Elasticsearch)]
```

---

## 🚀 Importar Dashboard Preconstruido

### Desde JSON

1. **Crear archivo** `smart-city-dashboard.json`:
```json
{
  "dashboard": {
    "title": "Smart City - Monitoreo de Alertas",
    "panels": [
      {
        "title": "Total Alertas",
        "type": "stat",
        "targets": [
          {
            "rawSql": "SELECT COUNT(*) FROM alerts WHERE created_at >= NOW() - INTERVAL '24 hours'"
          }
        ]
      }
    ]
  }
}
```

2. **Importar:**
   - Dashboard → **Import**
   - Upload JSON file
   - Select data source: **Smart City PostgreSQL**

---

## 📱 Dashboards Móviles

Grafana es responsive, puedes acceder desde móvil:
```
http://<tu-ip>:3000/
```

---

## 🔐 Configuración de Usuarios

### Crear Usuario para Operadores

1. **Server Admin** → **Users**
2. **New user**
   ```
   Name: Operador Zona 10
   Email: operador@smartcity.com
   Username: operador
   Password: ********
   Role: Viewer (solo lectura)
   ```

3. **Asignar Dashboards**
   - Dashboard → Settings → Permissions
   - Add: Operador (Viewer)

---

## 🎨 Temas y Personalización

### Cambiar Tema
- Profile → Preferences → UI Theme
  - **Dark** (recomendado para NOC)
  - **Light**

### Timezone
- Profile → Preferences → Timezone
  - Browser Time
  - America/Guatemala

---

## 📊 Plugins Útiles

### Instalar Plugins

```bash
docker exec -it smart-city-project-grafana-1 grafana-cli plugins install grafana-worldmap-panel
docker restart smart-city-project-grafana-1
```

**Plugins recomendados:**
- `grafana-worldmap-panel` - Mapa mundial
- `grafana-clock-panel` - Reloj
- `grafana-piechart-panel` - Gráficas de pie mejoradas

---

## 🔄 Refresh Automático

En el dashboard:
- Top right → **Refresh** dropdown
- Seleccionar: **5s**, **10s**, **30s**, **1m**
- Para monitoreo en tiempo real: **5s**

---

## 📥 Exportar/Importar Dashboards

### Exportar
1. Dashboard → Share → **Export**
2. Save to file → `smart-city-alerts.json`

### Importar
1. ➕ → **Import**
2. Upload JSON file o pegar JSON
3. Select data source

---

## 🎯 Casos de Uso Prácticos

### 1. Centro de Operaciones (NOC)
**Dashboard:** Vista general con métricas clave  
**Refresh:** 5 segundos  
**Pantalla:** TV de 55" en modo kiosk  

### 2. Análisis Post-Incidente
**Dashboard:** Análisis detallado con logs  
**Refresh:** Manual  
**Filtros:** Rango de tiempo específico  

### 3. Reportes Ejecutivos
**Dashboard:** KPIs y tendencias  
**Export:** PDF automático diario  
**Email:** A stakeholders  

---

## 🚨 Troubleshooting

### Grafana no se conecta a PostgreSQL
```bash
# Verificar que el contenedor está corriendo
docker ps | grep grafana

# Ver logs
docker logs smart-city-project-grafana-1

# Verificar red Docker
docker network inspect smart-city-project_default
```

### "Database locked" error
- PostgreSQL en Neon usa SSL, asegúrate de configurar `sslmode=require`

### Queries muy lentos
- Agregar índices en PostgreSQL:
```sql
CREATE INDEX idx_alerts_created_at ON alerts(created_at);
CREATE INDEX idx_alerts_zone ON alerts(zone);
CREATE INDEX idx_alerts_score ON alerts(score);
```

---

## 🎓 Recursos Adicionales

- **Documentación oficial:** https://grafana.com/docs/
- **Dashboards públicos:** https://grafana.com/grafana/dashboards/
- **Tutoriales:** https://grafana.com/tutorials/

---

## 📝 Ejemplo: Query para Panel en Tiempo Real

```sql
-- Panel: Alertas en los últimos 5 minutos
SELECT 
  created_at AS "time",
  type AS metric,
  score AS value,
  zone
FROM alerts
WHERE created_at >= NOW() - INTERVAL '5 minutes'
ORDER BY created_at DESC
```

**Configuración del Panel:**
- Visualization: Table
- Refresh: 5s
- Columns: time, type, score, zone
- Sort: time DESC

---

## 🎯 Resumen Rápido

1. **Accede:** http://localhost:3000/ (admin/admin)
2. **Conecta:** PostgreSQL (tu BD de alertas)
3. **Crea:** Dashboard nuevo
4. **Agrega:** Paneles con queries SQL
5. **Configura:** Refresh automático
6. **Alerta:** Configura notificaciones

**¡Listo para monitorear tu Smart City en tiempo real!** 🚀

---

**Última actualización:** 2025-10-18  
**Versión Grafana:** 9.5.0  
**Compatibilidad:** PostgreSQL, Prometheus, Elasticsearch
