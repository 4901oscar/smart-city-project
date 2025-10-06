# Smart City Event System - Testing Guide

## Descripción General
Este documento describe las estrategias y procedimientos de testing para el sistema de eventos Smart City, diseñado para deployment en el área local de la universidad.

---

## 1. Validación de Esquema Canónico (OBLIGATORIO)

### 1.1 Especificación del Envelope
Todos los eventos deben cumplir con el **envelope canónico v1.0** según la especificación:

```json
{
  "event_version": "1.0",
  "event_type": "panic.button | sensor.lpr | sensor.speed | sensor.acoustic | citizen.report",
  "event_id": "uuid-v4",
  "producer": "artillery | python-sim | kafka-cli | js-sim",
  "source": "simulated",
  "correlation_id": "uuid-v4",
  "trace_id": "uuid-v4",
  "timestamp": "2025-10-05T12:34:56.789Z",
  "partition_key": "zone_10 | XYZ123 | ...",
  "geo": { "zone": "Zona 10", "lat": 14.6091, "lon": -90.5252 },
  "severity": "info | warning | critical",
  "payload": { /* específico por sensor */ }
}
```

### 1.2 Validación en Backend
El sistema implementa **defensa en profundidad**:
- ✅ **Validación en Backend API** (`EventValidatorService.cs`)
- ✅ **Validación dual**: Envelope + Payload específico
- ✅ **Esquemas JSON** en `backend/Schemas/`

**Test de validación:**
```powershell
# Probar evento válido
curl -X POST http://localhost:5000/events -H "Content-Type: application/json" -d @test-event.json

# Probar evento inválido (debe retornar 400)
curl -X POST http://localhost:5000/events -H "Content-Type: application/json" -d '{\"invalid\":\"data\"}'
```

---

## 2. Estrategias de Testing

### 2.1 Testing de Integración (Manual)

#### Setup del Entorno
```powershell
# 1. Iniciar infraestructura
docker-compose up -d

# 2. Verificar servicios
docker ps

# Debe mostrar:
# - backend (puerto 5000)
# - kafka (puerto 9092)
# - zookeeper (puerto 2181)

# 3. Verificar conexión a DB
# El backend mostrará en logs: "✓ Conexión a la base de datos establecida correctamente"
```

#### Test 1: Producción de Eventos
```powershell
cd js-scripts

# Instalar dependencias (primera vez)
npm install

# Iniciar productor (genera eventos cada 3 segundos)
npm run producer

# Observar output:
# ✓ [panic.button] Severity: critical - Evento enviado...
# ✓ [sensor.lpr] Severity: warning - Evento enviado...
```

**Validaciones:**
- [ ] Eventos generados cumplen esquema canónico
- [ ] Producer ID es uno de: artillery, python-sim, kafka-cli, js-sim
- [ ] Timestamps en formato ISO 8601 UTC
- [ ] Severity asignada correctamente según payload
- [ ] Geo incluye zone (requerido), lat, lon (opcionales)

#### Test 2: Consumo y Alertas
```powershell
# En otra terminal
cd js-scripts
npm run consumer

# Observar alertas detectadas:
# ================================================================================
# 🚨 ALERTAS DETECTADAS 🚨
# Zona: Zona 10 | Coords: 14.6091, -90.5252
# --------------------------------------------------------------------------------
# [CRÍTICO] DISPARO DETECTADO
#   → Posible disparo de arma de fuego (87.5% confianza)
#   → 145 dB - Requiere unidad policial inmediata
```

**Validaciones:**
- [ ] Consumer se conecta a Kafka en localhost:9092
- [ ] Eventos se leen del topic `events-topic`
- [ ] Alertas se generan correctamente por tipo de evento
- [ ] Niveles de alerta (CRÍTICO/ALTO/MEDIO/INFO) son apropiados

#### Test 3: Persistencia en PostgreSQL
```powershell
# Verificar eventos guardados
curl http://localhost:5000/events?take=10

# Respuesta esperada:
# {
#   "count": 10,
#   "items": [
#     { "id": 1, "eventType": "panic.button", "timestamp": "..." },
#     ...
#   ]
# }
```

**Validaciones:**
- [ ] Eventos persisten en tabla `events`
- [ ] Payload guardado como JSONB
- [ ] Geo_lat y geo_lon extraídos correctamente
- [ ] Índices funcionando (idx_events_ts, idx_events_type, idx_events_zone)

### 2.2 Testing de Payload por Sensor

#### Panic Button
```json
{
  "event_type": "panic.button",
  "payload": {
    "tipo_de_alerta": "panico",
    "identificador_dispositivo": "BTN-Z10-001",
    "user_context": "movil"
  }
}
```
**Validaciones:**
- [ ] `tipo_de_alerta` es: panico | emergencia | incendio
- [ ] `user_context` es: movil | quiosco | web
- [ ] Alerta generada: CRÍTICO para panico/incendio

#### LPR Camera
```json
{
  "event_type": "sensor.lpr",
  "payload": {
    "placa_vehicular": "P-123AB",
    "velocidad_estimada": 95,
    "modelo_vehiculo": "Toyota Corolla",
    "color_vehiculo": "rojo",
    "ubicacion_sensor": "Av. Reforma"
  }
}
```
**Validaciones:**
- [ ] Todos los campos requeridos presentes
- [ ] velocidad_estimada >= 0
- [ ] Alerta CRÍTICO si velocidad > 100 km/h

#### Speed/Motion Sensor
```json
{
  "event_type": "sensor.speed",
  "payload": {
    "velocidad_detectada": 92,
    "sensor_id": "SPD-017",
    "direccion": "NORTE"
  }
}
```
**Validaciones:**
- [ ] velocidad_detectada >= 0
- [ ] Alerta ALTO si velocidad > 80 km/h

#### Acoustic Sensor
```json
{
  "event_type": "sensor.acoustic",
  "payload": {
    "tipo_sonido_detectado": "disparo",
    "nivel_decibeles": 145,
    "probabilidad_evento_critico": 0.87
  }
}
```
**Validaciones:**
- [ ] tipo_sonido: disparo | explosion | vidrio_roto
- [ ] probabilidad entre 0 y 1
- [ ] Alerta CRÍTICO para disparo/explosion

#### Citizen Report
```json
{
  "event_type": "citizen.report",
  "payload": {
    "tipo_evento": "accidente",
    "mensaje_descriptivo": "Choque en intersección",
    "ubicacion_aproximada": "Zona 10, 6ta Avenida",
    "origen": "app"
  }
}
```
**Validaciones:**
- [ ] tipo_evento: accidente | incendio | altercado
- [ ] origen: usuario | app | punto_fisico
- [ ] Alerta CRÍTICO para incendio

### 2.3 Testing de API (Swagger UI)

Acceder a: **http://localhost:5000**

#### Endpoints disponibles:
1. **POST /events** - Ingerir evento individual
2. **POST /events/bulk** - Ingerir múltiples eventos
3. **GET /events?take=20** - Listar eventos recientes
4. **GET /health** - Health check
5. **GET /schema** - Obtener esquemas cargados

**Test cases:**
```powershell
# Health check
curl http://localhost:5000/health

# Listar eventos
curl http://localhost:5000/events?take=5

# Ver esquemas cargados
curl http://localhost:5000/schema

# Envío bulk
curl -X POST http://localhost:5000/events/bulk -H "Content-Type: application/json" -d '[
  {evento1...},
  {evento2...}
]'
```

### 2.4 Testing de Kafka

#### Verificar topic
```powershell
# Entrar al contenedor Kafka
docker exec -it kafka bash

# Listar topics
kafka-topics --bootstrap-server localhost:9092 --list

# Debe mostrar: events-topic

# Consumir mensajes manualmente
kafka-console-consumer --bootstrap-server localhost:9092 --topic events-topic --from-beginning
```

#### Producir evento directo a Kafka
```powershell
# Desde contenedor Kafka
kafka-console-producer --bootstrap-server localhost:9092 --topic events-topic

# Pegar evento JSON completo
{"event_version":"1.0","event_type":"panic.button",...}
```

---

## 3. Testing de Compliance con Especificación

### Checklist de Validación

#### ✅ Envelope Canónico v1.0
- [x] event_version = "1.0" (constante)
- [x] event_type en enum definido
- [x] event_id como UUID v4
- [x] producer en: artillery | python-sim | kafka-cli | js-sim
- [x] source = "simulated" (constante)
- [x] correlation_id como UUID v4
- [x] trace_id como UUID v4
- [x] timestamp en ISO 8601 UTC
- [x] partition_key como string
- [x] geo.zone (requerido)
- [x] geo.lat, geo.lon (opcionales, numéricos)
- [x] severity en: info | warning | critical
- [x] payload como objeto

#### ✅ Payloads Específicos
- [x] panic.button: tipo_de_alerta, identificador_dispositivo, user_context
- [x] sensor.lpr: placa, velocidad, modelo, color, ubicacion
- [x] sensor.speed: velocidad, sensor_id, direccion
- [x] sensor.acoustic: tipo_sonido, decibeles, probabilidad
- [x] citizen.report: tipo_evento, mensaje, ubicacion, origen

#### ✅ Validación en Profundidad
- [x] Backend valida envelope + payload
- [x] Esquemas JSON en repositorio
- [x] Errores 400 con mensajes detallados
- [x] No se aceptan campos adicionales (additionalProperties: false)

---

## 4. Testing para Deployment Universitario

### 4.1 Configuración de Red Local

**Preparación:**
```powershell
# 1. Verificar IP del servidor
ipconfig

# 2. Actualizar docker-compose.yml si es necesario
# Cambiar localhost por IP del servidor en KAFKA_ADVERTISED_LISTENERS

# 3. Actualizar js-scripts para apuntar a IP del servidor
# En producer.js y consumer.js cambiar localhost por IP
```

### 4.2 Test de Carga (Simulación de Sensores)

```powershell
# Configurar múltiples productores simultáneos
# Terminal 1
npm run producer

# Terminal 2 (con otro producer ID)
# Editar producer.js: producer: 'python-sim'
npm run producer

# Terminal 3 (con otro producer ID)
# Editar producer.js: producer: 'artillery'
npm run producer
```

**Métricas a monitorear:**
- Eventos/segundo procesados
- Latencia de ingesta (< 100ms ideal)
- Latencia Kafka end-to-end
- Uso de CPU/RAM en contenedores

### 4.3 Test de Resiliencia

```powershell
# Test 1: Reiniciar Kafka
docker restart kafka
# Verificar: Backend debe reconectar automáticamente

# Test 2: Apagar/Prender base de datos
# Verificar: Backend muestra error pero no crashea

# Test 3: Enviar eventos inválidos
# Verificar: Retorna 400, no crashea, sigue procesando válidos
```

---

## 5. Validación de Alertas

### Matriz de Alertas Esperadas

| Event Type | Payload Condition | Alert Level | Action Required |
|------------|-------------------|-------------|-----------------|
| panic.button | tipo=panico | CRÍTICO | Dispatch emergency |
| panic.button | tipo=incendio | CRÍTICO | Alert fire dept |
| panic.button | tipo=emergencia | ALTO | Assess situation |
| sensor.lpr | velocidad>100 | CRÍTICO | Traffic enforcement |
| sensor.lpr | velocidad>70 | MEDIO | Monitor vehicle |
| sensor.speed | velocidad>80 | ALTO | Risk of accident |
| sensor.acoustic | tipo=disparo | CRÍTICO | Dispatch police |
| sensor.acoustic | tipo=explosion | CRÍTICO | Police + fire |
| sensor.acoustic | tipo=vidrio_roto | ALTO | Verify with cameras |
| citizen.report | tipo=incendio | CRÍTICO | Alert fire dept |
| citizen.report | tipo=accidente | ALTO | EMS dispatch |
| citizen.report | tipo=altercado | MEDIO | Monitor |

**Test de matriz completa:**
```powershell
# Generar eventos específicos para cada fila
# Verificar que consumer emite alerta correcta con nivel correcto
```

---

## 6. Checklist Pre-Deployment

### Infraestructura
- [ ] Docker y Docker Compose instalados
- [ ] Puertos disponibles: 5000, 9092, 2181
- [ ] Conexión a Neon verificada (o PostgreSQL local configurado)
- [ ] Firewall permite tráfico a puertos necesarios

### Base de Datos
- [ ] Script `database/init-neon.sql` ejecutado
- [ ] Tablas `events` y `alerts` creadas
- [ ] Índices creados correctamente
- [ ] Conexión SSL funcionando

### Backend
- [ ] Todos los esquemas JSON en `backend/Schemas/`
- [ ] appsettings.json con credenciales correctas
- [ ] Swagger UI accesible en puerto 5000
- [ ] Health endpoint responde OK

### Kafka
- [ ] Topic `events-topic` creado automáticamente
- [ ] Zookeeper respondiendo
- [ ] Producer puede escribir
- [ ] Consumer puede leer

### Testing Scripts
- [ ] `npm install` ejecutado en js-scripts
- [ ] Producer genera eventos válidos
- [ ] Consumer detecta alertas correctamente
- [ ] Todos los payloads cumplen schemas

---

## 7. Monitoreo en Producción

### Logs a Revisar
```powershell
# Logs del backend
docker logs backend -f

# Logs de Kafka
docker logs kafka -f

# Logs del consumer
cd js-scripts && npm run consumer
```

### Métricas Clave
- **Events ingested/sec**: Contar en GET /events
- **Validation failures**: Buscar en logs del backend
- **Kafka lag**: Diferencia entre producción y consumo
- **Database growth**: SELECT COUNT(*) FROM events

---

## 8. Troubleshooting

### Problema: Eventos no llegan a Kafka
```powershell
# Verificar que backend puede conectar a Kafka
docker logs backend | grep -i kafka

# Probar producción manual
docker exec -it kafka kafka-console-producer --bootstrap-server localhost:9092 --topic events-topic
```

### Problema: Validación falla
```powershell
# Ver errores detallados
curl -X POST http://localhost:5000/events -d @bad-event.json -v

# Verificar esquemas cargados
curl http://localhost:5000/schema
```

### Problema: Consumer no lee eventos
```powershell
# Verificar topic existe
docker exec -it kafka kafka-topics --bootstrap-server localhost:9092 --list

# Verificar mensajes en topic
docker exec -it kafka kafka-console-consumer --bootstrap-server localhost:9092 --topic events-topic --from-beginning
```

---

## 9. Próximos Pasos

### Mejoras Recomendadas
1. **Unit Tests**: Agregar tests para EventValidatorService
2. **Integration Tests**: Automatizar flujo producer → backend → kafka → consumer
3. **Load Tests**: Usar Artillery para generar 1000+ eventos/seg
4. **Schema Registry**: Implementar Avro + Confluent Schema Registry
5. **Alerting**: Webhook a sistema externo para alertas CRÍTICO
6. **Dashboard**: Grafana + Prometheus para métricas en tiempo real

### Para Deployment Final
- [ ] Configurar SSL/TLS en Kafka
- [ ] Implementar autenticación en API
- [ ] Rate limiting en endpoints
- [ ] Backup automático de PostgreSQL
- [ ] Log aggregation (ELK Stack o similar)
- [ ] Configurar múltiples zonas geográficas
