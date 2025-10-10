# ✅ Guía de Verificación - Arquitectura Kafka Completa

## 🎯 Implementación Completada

### Topics Creados

```bash
# Verificar que los 3 topics requeridos existen
docker exec kafka kafka-topics --bootstrap-server localhost:9092 --list
```

**Esperado:**
- ✅ `events.standardized` - Eventos válidos y enriquecidos
- ✅ `correlated.alerts` - Alertas generadas por el sistema
- ✅ `events.dlq` - Eventos fallidos (Dead Letter Queue)

---

## 📊 Arquitectura del Flujo de Datos

```
Producer (JS) 
    ↓ POST /events
Backend API
    ├─→ [Validación OK] → Kafka: events.standardized
    └─→ [Validación FAIL] → Kafka: events.dlq
         ↓
Consumer (JS)
    ├─→ Lee de: events.standardized
    ├─→ Genera alertas
    └─→ Publica a: correlated.alerts
         ↓
Alert Monitor (JS)
    └─→ Lee de: correlated.alerts
         └─→ Muestra alertas en tiempo real

DLQ Monitor (JS)
    └─→ Lee de: events.dlq
         └─→ Muestra eventos fallidos
```

---

## 🚀 Pasos de Verificación

### 1. Verificar Configuración de Topics

```bash
# Describir cada topic para ver su configuración
docker exec kafka kafka-topics --bootstrap-server localhost:9092 --describe --topic events.standardized
docker exec kafka kafka-topics --bootstrap-server localhost:9092 --describe --topic correlated.alerts
docker exec kafka kafka-topics --bootstrap-server localhost:9092 --describe --topic events.dlq
```

**Configuraciones esperadas:**

| Topic | Particiones | Replication Factor | Retention | Compression |
|-------|-------------|-------------------|-----------|-------------|
| events.standardized | 3 | 1 | 7 días | snappy |
| correlated.alerts | 2 | 1 | 30 días | lz4 |
| events.dlq | 1 | 1 | 14 días | gzip |

---

### 2. Prueba del Flujo Completo

#### Terminal 1: Iniciar Producer
```bash
cd js-scripts
npm run producer
```
**Esperado:** Eventos enviados cada 3 segundos con confirmación del backend.

---

#### Terminal 2: Iniciar Consumer (lee events.standardized, publica a correlated.alerts)
```bash
cd js-scripts
npm run consumer
```
**Esperado:** 
- ✅ Mensajes: "Evento recibido: sensor.lpr | Zona: Zona 10"
- ✅ Alertas detectadas con colores
- ✅ Confirmación: "✓ Alertas publicadas a correlated.alerts"

---

#### Terminal 3: Monitor de Alertas (lee correlated.alerts)
```bash
cd js-scripts
npm run alert-monitor
```
**Esperado:**
- ✅ Header verde: "📊 MONITOR DE ALERTAS CORRELACIONADAS"
- ✅ Alertas con formato estructurado
- ✅ Estadísticas cada 5 alertas

---

#### Terminal 4: Monitor de DLQ (lee events.dlq)
```bash
cd js-scripts
npm run dlq-monitor
```
**Esperado:**
- ✅ Header rojo: "⚠️ MONITOR DE DEAD LETTER QUEUE"
- ✅ Sin mensajes si todos los eventos son válidos
- ✅ Errores mostrados si hay validaciones fallidas

---

### 3. Prueba de Validación (Generar Errores)

**Enviar evento inválido para poblar DLQ:**

```bash
curl -X POST http://localhost:5000/events \
  -H "Content-Type: application/json" \
  -d '{
    "event_type": "TIPO_INVALIDO",
    "event_id": "invalid-event-123"
  }'
```

**Verificar en Terminal 4 (DLQ Monitor):**
- ✅ Debe aparecer error: "SCHEMA_VALIDATION_FAILED"
- ✅ Detalles de errores de validación

---

### 4. Verificar Mensajes en Topics (Consola Kafka)

#### events.standardized (eventos válidos)
```bash
docker exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic events.standardized \
  --from-beginning \
  --max-messages 3
```

**Esperado:** JSON con estructura completa de eventos enriquecidos.

---

#### correlated.alerts (alertas generadas)
```bash
docker exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic correlated.alerts \
  --from-beginning \
  --max-messages 3
```

**Esperado:** JSON con estructura:
```json
{
  "alert_id": "uuid",
  "correlation_id": "uuid",
  "source_event_id": "uuid",
  "event_type": "sensor.lpr",
  "zone": "Zona 10",
  "coordinates": { "lat": 14.6091, "lon": -90.5252 },
  "timestamp": "2025-10-09T...",
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

---

#### events.dlq (eventos fallidos)
```bash
docker exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic events.dlq \
  --from-beginning \
  --max-messages 3
```

**Esperado (solo si hay errores):** JSON con estructura:
```json
{
  "original_event": { /* evento original */ },
  "validation_errors": ["error1", "error2"],
  "timestamp": "2025-10-09T...",
  "reason": "SCHEMA_VALIDATION_FAILED"
}
```

---

## 🔍 Checklist de Verificación

### Infraestructura
- [ ] 3 topics creados: events.standardized, correlated.alerts, events.dlq
- [ ] Configuraciones correctas (particiones, retention, compression)
- [ ] Backend reiniciado con cambios aplicados

### Flujo de Eventos Válidos
- [ ] Producer envía eventos a Backend API
- [ ] Backend valida y publica a `events.standardized`
- [ ] Consumer lee de `events.standardized`
- [ ] Consumer detecta alertas
- [ ] Consumer publica alertas a `correlated.alerts`
- [ ] Alert Monitor muestra alertas en tiempo real

### Flujo de Errores (DLQ)
- [ ] Backend detecta eventos inválidos
- [ ] Backend publica errores a `events.dlq`
- [ ] DLQ Monitor muestra errores
- [ ] Errores contienen información completa (evento original + errores)

### Persistencia en Base de Datos
- [ ] Eventos válidos se guardan en PostgreSQL tabla `events`
- [ ] Todos los 14 campos poblados correctamente
- [ ] Coordenadas geográficas correctas

---

## 📈 Métricas a Observar

### En Consumer
```
✓ Alertas publicadas a correlated.alerts
```
Debe aparecer después de cada alerta detectada.

### En Alert Monitor
```
📈 ESTADÍSTICAS DEL SISTEMA:
Total alertas procesadas: 15
Por nivel:
  CRÍTICO: 5
  ALTO: 7
  MEDIO: 2
  INFO: 1
Por zona:
  Zona 10: 15
```

### En DLQ Monitor
```
📊 ESTADÍSTICAS DE ERRORES:
Total errores: 3
Por tipo de error:
  SCHEMA_VALIDATION_FAILED: 3
```

---

## 🐛 Troubleshooting

### Problema: Consumer no recibe mensajes

**Diagnóstico:**
```bash
# Verificar que events.standardized tiene mensajes
docker exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic events.standardized \
  --from-beginning --max-messages 1
```

**Soluciones:**
1. Verificar que Producer esté enviando eventos
2. Verificar que Backend esté publicando a `events.standardized` (no `events-topic`)
3. Reiniciar consumer con `fromBeginning: true`

---

### Problema: Alert Monitor no muestra alertas

**Diagnóstico:**
```bash
# Verificar mensajes en correlated.alerts
docker exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic correlated.alerts \
  --from-beginning --max-messages 1
```

**Soluciones:**
1. Verificar que Consumer esté conectado (mensaje de inicio)
2. Verificar que alertas se estén detectando (revisar terminal del consumer)
3. Verificar errores de conexión del producer en consumer

---

### Problema: DLQ está vacío cuando debería tener errores

**Causa:** Todos los eventos están siendo validados correctamente.

**Para probar DLQ:**
```bash
# Enviar evento sin campos requeridos
curl -X POST http://localhost:5000/events \
  -H "Content-Type: application/json" \
  -d '{"event_type": "panic.button"}'
```

---

## 🎨 Visualización de Logs

### Consumer (events.standardized → alertas)
```
[INFO] Evento recibido: sensor.lpr | Zona: Zona 10
================================================================================
🚨 ALERTAS DETECTADAS 🚨
Zona: Zona 10 | Coords: 14.6091, -90.5252
Timestamp: 2025-10-09T... | Event ID: a1b2c3d4...
--------------------------------------------------------------------------------
[CRÍTICO] EXCESO DE VELOCIDAD PELIGROSO
  → Vehículo a 120 km/h detectado
  → Placa: GTM-1234 | Rojo Toyota Camry | Sensor: Av_Reforma_Norte
================================================================================
✓ Alertas publicadas a correlated.alerts
```

### Alert Monitor (correlated.alerts)
```
══════════════════════════════════════════════════════════════════════════════════════════
🔔 ALERTA CORRELACIONADA #1
──────────────────────────────────────────────────────────────────────────────────────────
Alert ID:        f9e8d7c6-5b4a...
Event ID:        a1b2c3d4-5e6f...
Tipo Evento:     sensor.lpr
Zona:            Zona 10
Coordenadas:     14.6091, -90.5252
Timestamp:       9/10/2025 17:30:45
──────────────────────────────────────────────────────────────────────────────────────────
[CRÍTICO] EXCESO DE VELOCIDAD PELIGROSO
  ➤ Vehículo a 120 km/h detectado
  ➤ Placa: GTM-1234 | Rojo Toyota Camry | Sensor: Av_Reforma_Norte
══════════════════════════════════════════════════════════════════════════════════════════
```

### DLQ Monitor (events.dlq)
```
══════════════════════════════════════════════════════════════════════════════════════════
⚠️  ERROR #1 - SCHEMA_VALIDATION_FAILED
──────────────────────────────────────────────────────────────────────────────────────────
Timestamp:       9/10/2025 17:31:00
Event Type:      panic.button
Event ID:        invalid-even...
Producer:        UNKNOWN
──────────────────────────────────────────────────────────────────────────────────────────
Errores de Validación:
  1. Required property 'event_version' is missing
  2. Required property 'payload' is missing
══════════════════════════════════════════════════════════════════════════════════════════
```

---

## 🎯 Resultado Esperado

Al finalizar todas las verificaciones debes tener:

1. **4 terminales corriendo simultáneamente:**
   - Terminal 1: Producer (enviando eventos cada 3s)
   - Terminal 2: Consumer (procesando eventos y generando alertas)
   - Terminal 3: Alert Monitor (mostrando alertas correlacionadas)
   - Terminal 4: DLQ Monitor (mostrando errores si los hay)

2. **3 topics funcionando:**
   - `events.standardized`: recibiendo ~20 eventos/minuto
   - `correlated.alerts`: recibiendo alertas generadas
   - `events.dlq`: vacío o con errores si se envían eventos inválidos

3. **Flujo completo verificado:**
   - ✅ Validación de eventos
   - ✅ Publicación a topic correcto según validación
   - ✅ Detección de alertas
   - ✅ Correlación y publicación de alertas
   - ✅ Persistencia en PostgreSQL

---

## 📝 Comandos Rápidos de Verificación

```bash
# Ver todos los topics
docker exec kafka kafka-topics --bootstrap-server localhost:9092 --list

# Ver últimos 5 eventos estandarizados
docker exec kafka kafka-console-consumer --bootstrap-server localhost:9092 --topic events.standardized --from-beginning --max-messages 5

# Ver últimas 5 alertas correlacionadas
docker exec kafka kafka-console-consumer --bootstrap-server localhost:9092 --topic correlated.alerts --from-beginning --max-messages 5

# Ver errores en DLQ
docker exec kafka kafka-console-consumer --bootstrap-server localhost:9092 --topic events.dlq --from-beginning --max-messages 5

# Contar mensajes en cada topic (aproximado)
docker exec kafka kafka-run-class kafka.tools.GetOffsetShell --broker-list localhost:9092 --topic events.standardized
docker exec kafka kafka-run-class kafka.tools.GetOffsetShell --broker-list localhost:9092 --topic correlated.alerts
docker exec kafka kafka-run-class kafka.tools.GetOffsetShell --broker-list localhost:9092 --topic events.dlq
```

---

## ✅ Criterios de Éxito

La implementación está completa cuando:

- [ ] Los 3 topics existen y están configurados correctamente
- [ ] Producer envía eventos sin errores
- [ ] Backend valida y enruta eventos correctamente
- [ ] Consumer procesa eventos de `events.standardized`
- [ ] Consumer publica alertas a `correlated.alerts`
- [ ] Alert Monitor muestra alertas en tiempo real
- [ ] DLQ Monitor captura errores de validación
- [ ] PostgreSQL persiste eventos correctamente
- [ ] Todos los monitores muestran logs con colores y formato correcto

---

**Fecha de creación:** 9 de octubre de 2025  
**Estado:** Implementación completa de arquitectura Kafka de 3 topics  
**Próximo paso:** Ejecutar verificación completa siguiendo esta guía
