# Elasticsearch en Smart City - Guia Completa

## Estado Actual: OPERACIONAL

- Elasticsearch: http://localhost:9200/ 
- Kibana: http://localhost:5601/ 

## Que Necesitas para Usar Elasticsearch

### 1. Indices Creados
- `events` - Para almacenar todos los eventos del sistema
- `alerts` - Para almacenar todas las alertas generadas

### 2. Flujo de Datos Actual

```
Producer → Backend API → PostgreSQL (persistencia permanente)
                      ↓
                    Kafka events.standardized
                      ↓
                  Consumer
                      ├─→ PostgreSQL (alertas)
                      ├─→ Kafka correlated.alerts
                      └─→ Elasticsearch (indexacion para busqueda)
```

### 3. Que Hace Elasticsearch en Tu Proyecto

1. Busqueda Rapida
   - Buscar eventos por tipo, zona, severidad
   - Buscar alertas por nivel, mensaje
   - Full-text search en descripciones

2. Analisis en Tiempo Real
   - Agregaciones por zona
   - Conteo de eventos por tipo
   - Tendencias temporales

3. Visualizaciones en Kibana
   - Dashboards interactivos
   - Mapas de calor
   - Graficas de series de tiempo

4. Consultas Complejas
   - Filtros combinados
   - Rangos de tiempo
   - Geo-queries (eventos cerca de una ubicacion)

## Como Usar Elasticsearch

### Opcion 1: Via Consumer (Automatico)

El consumer ya esta modificado para enviar alertas a Elasticsearch:

```javascript
// En consumer.js - linea ~280
await axios.post(`${ELASTICSEARCH_URL}/alerts/_doc`, esDoc, {
  headers: { 'Content-Type': 'application/json' }
});
```

Solo necesitas reiniciar el consumer para que empiece a indexar.

### Opcion 2: Via API REST Directa

Puedes enviar documentos directamente:

```powershell
# Indexar un evento
$evento = @{
    event_id = "test-123"
    event_type = "sensor.speed"
    zone = "Zona 10"
    severity = "critical"
    timestamp = (Get-Date).ToUniversalTime().ToString("o")
    payload = @{
        velocidad = 150
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:9200/events/_doc" -Method POST -Body $evento -ContentType "application/json"
```

### Opcion 3: Via Kibana Dev Tools

1. Abrir Kibana: http://localhost:5601/
2. Ir a Menu → Dev Tools
3. Ejecutar queries DSL

## Queries Utiles de Elasticsearch

### Buscar Todas las Alertas
```json
GET /alerts/_search
{
  "query": {
    "match_all": {}
  },
  "size": 20,
  "sort": [
    { "@timestamp": "desc" }
  ]
}
```

### Buscar Alertas Criticas en Zona 10
```json
GET /alerts/_search
{
  "query": {
    "bool": {
      "must": [
        { "match": { "zone": "Zona 10" } },
        { "nested": {
            "path": "alerts",
            "query": {
              "match": { "alerts.level": "CRITICO" }
            }
          }
        }
      ]
    }
  }
}
```

### Buscar Eventos de Velocidad en Ultimas 24h
```json
GET /events/_search
{
  "query": {
    "bool": {
      "must": [
        { "match": { "event_type": "sensor.speed" } },
        { "range": {
            "timestamp": {
              "gte": "now-24h"
            }
          }
        }
      ]
    }
  }
}
```

### Agregacion: Conteo de Eventos por Tipo
```json
GET /events/_search
{
  "size": 0,
  "aggs": {
    "por_tipo": {
      "terms": {
        "field": "event_type",
        "size": 10
      }
    }
  }
}
```

### Agregacion: Alertas por Zona
```json
GET /alerts/_search
{
  "size": 0,
  "aggs": {
    "por_zona": {
      "terms": {
        "field": "zone",
        "size": 10
      }
    }
  }
}
```

## Crear Data Views en Kibana

### Paso 1: Abrir Kibana
http://localhost:5601/

### Paso 2: Ir a Stack Management
Menu → Stack Management → Data Views

### Paso 3: Crear Data View para Eventos
- Name: Events
- Index pattern: events*
- Timestamp field: timestamp
- Click: Create data view

### Paso 4: Crear Data View para Alertas
- Name: Alerts
- Index pattern: alerts*
- Timestamp field: @timestamp
- Click: Create data view

## Visualizar Datos en Kibana Discover

### Para Eventos:
1. Menu → Discover
2. Seleccionar data view: Events
3. Ajustar rango de tiempo (Last 15 minutes, Last 1 hour, etc.)
4. Agregar filtros:
   - event_type: sensor.speed
   - severity: critical
   - geo.zone: "Zona 10"

### Para Alertas:
1. Menu → Discover
2. Seleccionar data view: Alerts
3. Ver alertas en tiempo real
4. Expandir documentos para ver detalles

## Crear Dashboard en Kibana

### Panel 1: Total de Eventos
1. Menu → Dashboard → Create dashboard
2. Add visualization
3. Tipo: Metric
4. Data view: Events
5. Metric: Count

### Panel 2: Eventos por Tipo (Pie Chart)
1. Add visualization
2. Tipo: Pie
3. Data view: Events
4. Slice by: event_type

### Panel 3: Timeline de Alertas
1. Add visualization
2. Tipo: Area
3. Data view: Alerts
4. Vertical axis: Count
5. Horizontal axis: @timestamp

### Panel 4: Mapa de Alertas
1. Add visualization
2. Tipo: Maps
3. Add layer → Clusters
4. Index pattern: alerts*
5. Geospatial field: geo_location

## Queries PowerShell para Testing

### Verificar Elasticsearch
```powershell
Invoke-RestMethod -Uri "http://localhost:9200/_cluster/health"
```

### Listar Indices
```powershell
Invoke-RestMethod -Uri "http://localhost:9200/_cat/indices?v"
```

### Contar Documentos en events
```powershell
Invoke-RestMethod -Uri "http://localhost:9200/events/_count"
```

### Contar Documentos en alerts
```powershell
Invoke-RestMethod -Uri "http://localhost:9200/alerts/_count"
```

### Buscar Ultimos 10 Eventos
```powershell
$query = @{
    size = 10
    sort = @( @{ timestamp = "desc" } )
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:9200/events/_search" -Method POST -Body $query -ContentType "application/json"
```

## Integracion con Backend .NET

Para enviar eventos a Elasticsearch desde el backend:

```csharp
// En EventsController.cs - despues de guardar en PostgreSQL

using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("http://elasticsearch:9200");

var esDoc = new
{
    event_id = eventEnvelope.event_id,
    event_type = eventEnvelope.event_type,
    timestamp = eventEnvelope.timestamp,
    severity = eventEnvelope.severity,
    geo = eventEnvelope.geo,
    payload = eventEnvelope.payload
};

var content = new StringContent(
    JsonSerializer.Serialize(esDoc),
    Encoding.UTF8,
    "application/json"
);

await httpClient.PostAsync("/events/_doc", content);
```

## Monitoreo de Elasticsearch

### Ver Estado del Cluster
```powershell
Invoke-RestMethod -Uri "http://localhost:9200/_cluster/health?pretty"
```

### Ver Uso de Disco
```powershell
Invoke-RestMethod -Uri "http://localhost:9200/_cat/allocation?v"
```

### Ver Estadisticas de Indices
```powershell
Invoke-RestMethod -Uri "http://localhost:9200/_stats?pretty"
```

## Backup y Mantenimiento

### Ver Tamaño de Indices
```powershell
Invoke-RestMethod -Uri "http://localhost:9200/_cat/indices?v&s=store.size:desc"
```

### Eliminar Datos Antiguos (ILM)
```json
DELETE /events/_query
{
  "query": {
    "range": {
      "timestamp": {
        "lt": "now-30d"
      }
    }
  }
}
```

## Troubleshooting

### Elasticsearch no arranca
```powershell
docker logs es
docker compose restart elasticsearch
```

### Kibana no conecta
```powershell
# Verificar que Elasticsearch este en green o yellow
Invoke-RestMethod -Uri "http://localhost:9200/_cluster/health"

# Reiniciar Kibana
docker compose restart kibana
```

### Consumer no indexa en Elasticsearch
```powershell
# Verificar que ELASTICSEARCH_URL este configurado
$env:ELASTICSEARCH_URL = "http://localhost:9200"

# Reiniciar consumer
cd js-scripts
npm run consumer
```

## Comparacion: PostgreSQL vs Elasticsearch

| Caracteristica | PostgreSQL | Elasticsearch |
|----------------|------------|---------------|
| Persistencia | Permanente | Temporal (configurable) |
| Busqueda | SQL complejo | Full-text rapido |
| Agregaciones | Lentas en millones | Rapidas en millones |
| Geo-queries | Limitadas | Nativas y potentes |
| Uso Principal | Datos transaccionales | Busqueda y analisis |
| En este proyecto | Fuente de verdad | Cache de busqueda |

## Mejores Practicas

1. PostgreSQL para Persistencia
   - Usa PostgreSQL como fuente de verdad
   - Eventos y alertas permanentes

2. Elasticsearch para Busqueda
   - Indexa copias de datos para busqueda rapida
   - Configura retention policies (ej: 30 dias)

3. Kibana para Visualizacion
   - Dashboards en tiempo real
   - Analisis exploratorio
   - Monitoreo operacional

4. Grafana para Metricas
   - Metricas de sistema (Kafka, CPU, memoria)
   - Alertas de infraestructura

## Proximos Pasos

1. Reiniciar consumer para que indexe alertas
2. Generar eventos con producer
3. Ver datos en Kibana Discover
4. Crear dashboard personalizado
5. Configurar alertas en Kibana

## URLs de Referencia

- Elasticsearch: http://localhost:9200/
- Kibana: http://localhost:5601/
- Grafana: http://localhost:3000/
- Kafka UI: http://localhost:8080/
- Backend API: http://localhost:5000/

## Resumen

Para usar Elasticsearch funcionalmente necesitas:

1. Elasticsearch y Kibana corriendo (Ya hecho)
2. Indices creados (Ya hecho - events y alerts)
3. Consumer enviando datos a Elasticsearch (Configurado - solo reiniciar)
4. Data views en Kibana (Manual - 2 minutos)
5. Dashboards en Kibana (Manual - segun necesidades)

Sistema listo para recibir y buscar eventos y alertas!
