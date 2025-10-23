# 📊 Dashboard Dinámico de Grafana - Smart City

## 🚀 Acceso al Dashboard

### Credenciales por Defecto
- **URL**: http://localhost:3000
- **Usuario**: `admin`
- **Contraseña**: `admin` (se debe cambiar en primer login)

### 📍 Localización del Dashboard
- **Nombre**: Smart City - Dashboard de Métricas Dinámicas
- **ID**: smart-city-metrics
- **Ruta**: Dashboards > Browse > smart-city-metrics.json

## 📈 Métricas Implementadas

### 1. **Métricas Temporales (Time Series)**
- **Eventos Recibidos por Minuto**: Visualiza el flujo de eventos en tiempo real
- **Alertas Generadas por Minuto**: Muestra la generación de alertas por periodo
- **Eventos por Tipo a lo Largo del Tiempo**: Distribución temporal por tipo de evento

### 2. **Contadores (Stat Panels)**
- **Total Eventos**: Contador total de eventos recibidos
- **Total Alertas**: Contador total de alertas generadas
- **Score Promedio de Alertas**: Métrica de gauge mostrando criticidad promedio

### 3. **Distribuciones (Pie Charts)**
- **Distribución por Tipo de Evento**: 
  - panic.button, sensor.lpr, sensor.speed, sensor.acoustic, citizen.report
- **Alertas por Zona**: Distribución geográfica de alertas
- **Eventos por Tipo de Sensor**: Clasificación por categoría de sensor

### 4. **Tablas Informativas**
- **Top 10 Tipos de Alertas**: Con cantidad, score promedio y última ocurrencia
- **Últimas 100 Alertas Detalladas**: Log completo de alertas recientes

## 🔧 Configuración de Data Sources

### PostgreSQL Smart City
```yaml
Name: Smart City PostgreSQL
Type: postgres
Host: ${PGHOST}:5432
Database: ${PGDATABASE}
User: ${PGUSER}
SSL Mode: require
```

### Prometheus (Métricas de Kafka)
```yaml
Name: Prometheus
Type: prometheus
URL: http://prometheus:9090
```

## 🗄️ Queries SQL Principales

### Eventos por Minuto
```sql
SELECT
  DATE_TRUNC('minute', ts_utc) as time,
  COUNT(*) as "Eventos Recibidos"
FROM events
WHERE $__timeFilter(ts_utc)
GROUP BY DATE_TRUNC('minute', ts_utc)
ORDER BY time
```

### Alertas por Tipo
```sql
SELECT 
  type as "Tipo de Alerta",
  COUNT(*) as "Cantidad",
  AVG(score) as "Score Promedio",
  MAX(created_at) as "Última Ocurrencia"
FROM alerts 
WHERE $__timeFilter(created_at)
GROUP BY type
ORDER BY COUNT(*) DESC
```

### Distribución por Sensor
```sql
SELECT 
  CASE 
    WHEN payload_data->>'event_type' = 'panic.button' THEN 'Botón Pánico'
    WHEN payload_data->>'event_type' = 'sensor.lpr' THEN 'Sensor LPR'
    WHEN payload_data->>'event_type' = 'sensor.speed' THEN 'Sensor Velocidad'
    WHEN payload_data->>'event_type' = 'sensor.acoustic' THEN 'Sensor Acústico'
    WHEN payload_data->>'event_type' = 'citizen.report' THEN 'Reporte Ciudadano'
    ELSE 'Otros'
  END as "Tipo de Sensor",
  COUNT(*) as "Cantidad"
FROM events 
WHERE $__timeFilter(ts_utc)
GROUP BY payload_data->>'event_type'
ORDER BY COUNT(*) DESC
```

## ⚙️ Configuración de Alertas

### Umbrales Configurados
- **Verde**: Operación normal (< 50 eventos/minuto)
- **Amarillo**: Carga media (50-100 eventos/minuto) 
- **Rojo**: Carga alta (> 100 eventos/minuto)

### Refresh Automático
- **Intervalo**: 30 segundos
- **Rango temporal por defecto**: Últimas 6 horas

## 🎯 Casos de Uso

### Para Operadores
1. **Monitoreo en Tiempo Real**: Panels de time series para tendencias
2. **Análisis de Patrones**: Pie charts para identificar tipos de eventos frecuentes
3. **Respuesta a Incidentes**: Tabla de alertas recientes para investigación

### Para Administradores
1. **Análisis de Capacidad**: Métricas de throughput y carga del sistema
2. **Análisis de Zonas**: Identificar áreas con mayor actividad
3. **Tendencias Históricas**: Usar filtros temporales para análisis retrospectivo

### Para Analistas
1. **Correlación de Eventos**: Análisis temporal de diferentes tipos de sensores
2. **Evaluación de Criticidad**: Score promedio y distribución de alertas
3. **Eficiencia del Sistema**: Ratio eventos/alertas generadas

## 🔍 Filtros y Variables

### Filtros Temporales
- Último minuto, 5 minutos, 15 minutos
- Última hora, 6 horas, 12 horas
- Último día, semana, mes
- Rango personalizado

### Drill-Down Capabilities
- Clic en pie charts para filtrar por tipo
- Selección de rangos temporales en time series
- Ordenamiento dinámico en tablas

## 📱 Responsividad

El dashboard está optimizado para:
- **Desktop**: Vista completa con todos los paneles
- **Tablet**: Reorganización automática de paneles
- **Mobile**: Vista simplificada con métricas clave

## 🚀 Próximas Mejoras

### Variables Dinámicas
- Selector de zona geográfica
- Filtro por tipo de sensor
- Selector de nivel de severidad

### Alerting
- Notificaciones por email/SMS
- Webhooks para integración externa
- Escalación automática

### Análisis Avanzado
- Correlación temporal entre eventos
- Predicción de patrones
- Mapas de calor geográficos

## 🛠️ Mantenimiento

### Backup del Dashboard
```bash
# Exportar dashboard
curl -H "Authorization: Bearer YOUR_API_KEY" \
  http://localhost:3000/api/dashboards/uid/smart-city-metrics > backup.json
```

### Actualización de Queries
1. Acceder a Edit Panel
2. Modificar Query SQL
3. Test y Save
4. Export dashboard actualizado

### Monitoreo de Performance
- Verificar tiempo de respuesta de queries
- Optimizar índices en PostgreSQL si es necesario
- Ajustar intervalos de refresh según carga

---

**Fecha de Creación**: Octubre 23, 2025  
**Versión**: 1.0  
**Mantenido por**: Smart City Development Team