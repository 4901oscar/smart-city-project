# 📊 Análisis de Requerimientos Kafka vs Implementación Actual

## ❌ Estado Actual: IMPLEMENTACIÓN PARCIAL

### Requerimientos del Proyecto

```
Kafka: Backbone del sistema
Topics requeridos:
  1. events.standardized    - Eventos estandarizados/normalizados
  2. correlated.alerts      - Alertas correlacionadas
  3. events.dlq            - Dead Letter Queue (eventos fallidos)
```

---

## ✅ Lo que ESTÁ Implementado

### 1. Topic Actual: `events-topic`
```
✅ Función: Topic principal para todos los eventos
✅ Uso: Producer → Backend → Kafka → Consumer
✅ Configuración: 1 partición, RF=1
✅ Formato: JSON con envelope canónico v1.0
```

**Características:**
- ✅ Recibe todos los tipos de eventos (panic, lpr, speed, acoustic, citizen)
- ✅ Eventos validados antes de publicar
- ✅ Persistencia configurada (Acks.All)
- ✅ Logs de offset y partición

---

## ❌ Lo que NO ESTÁ Implementado

### 1. ❌ `events.standardized` - Eventos Estandarizados
**Estado**: NO implementado

**Propósito esperado**:
- Eventos normalizados después de procesamiento
- Transformaciones aplicadas (enriquecimiento, validación)
- Formato consistente para consumidores downstream

**Actualmente**:
- Los eventos se publican directamente a `events-topic`
- No hay separación entre eventos raw vs estandarizados

---

### 2. ❌ `correlated.alerts` - Alertas Correlacionadas
**Estado**: NO implementado

**Propósito esperado**:
- Alertas generadas por el consumer
- Resultados de correlación multi-evento
- Eventos agregados por ventana temporal/espacial

**Actualmente**:
- Alertas solo se muestran en consola del consumer
- NO se publican de vuelta a Kafka
- NO hay persistencia de alertas en topic

---

### 3. ❌ `events.dlq` - Dead Letter Queue
**Estado**: NO implementado

**Propósito esperado**:
- Eventos que fallaron validación
- Errores de procesamiento
- Reintentos fallidos

**Actualmente**:
- Eventos inválidos retornan 400 al producer
- NO se guardan eventos fallidos
- NO hay mecanismo de retry

---

## 📋 Comparación Detallada

| Característica | Requerido | Implementado | Estado |
|----------------|-----------|--------------|--------|
| Topic de eventos raw | `events.standardized` | `events-topic` | ⚠️ Parcial |
| Topic de alertas | `correlated.alerts` | NO | ❌ Faltante |
| Dead Letter Queue | `events.dlq` | NO | ❌ Faltante |
| Validación de esquemas | ✅ | ✅ | ✅ Completo |
| Enriquecimiento de datos | ✅ | ✅ (Zona 10) | ✅ Completo |
| Persistencia en DB | ✅ | ✅ (PostgreSQL) | ✅ Completo |
| Correlación de eventos | ✅ | ⚠️ (Solo en consumer) | ⚠️ Parcial |
| Publicación de alertas | ✅ | ❌ (Solo consola) | ❌ Faltante |

---

## 🔄 Arquitectura Actual vs Requerida

### Arquitectura Actual (Simplificada)
```
Producer (JS)
    ↓
Backend API (Validación + Enriquecimiento)
    ↓
Kafka: events-topic
    ↓
Consumer (JS) → Alertas en consola
    ↓
PostgreSQL (Persistencia)
```

### Arquitectura Requerida (Completa)
```
Producer (JS)
    ↓
Backend API (Validación)
    ↓ (eventos válidos)
Kafka: events.standardized ← Eventos normalizados
    ↓
Consumer/Processor (Correlación)
    ↓ (alertas generadas)
Kafka: correlated.alerts ← Alertas correlacionadas
    ↓
Alert Consumer → Notificaciones/Dashboard
    ↓
PostgreSQL (Persistencia eventos + alertas)

    ↓ (eventos inválidos desde Backend)
Kafka: events.dlq ← Dead Letter Queue
    ↓
DLQ Monitor → Análisis de errores
```

---

## 🛠️ Plan de Implementación Recomendado

### Fase 1: Renombrar y Reorganizar Topics ✅

#### 1.1 Crear nuevo topic `events.standardized`
```bash
docker exec kafka kafka-topics --bootstrap-server localhost:9092 \
  --create --topic events.standardized \
  --partitions 3 \
  --replication-factor 1 \
  --config retention.ms=604800000 \
  --config compression.type=snappy
```

**Configuración recomendada para laboratorio**:
- **Particiones**: 3 (permite escalar a 3 consumidores)
- **Replication Factor**: 1 (suficiente para lab, 3 en producción)
- **Retention**: 7 días (604800000 ms)
- **Compression**: Snappy (balance velocidad/tamaño)

#### 1.2 Actualizar Backend para publicar a `events.standardized`
```csharp
// En EventsController.cs
await _producer.Publish("events.standardized", eventData.ToString());
```

---

### Fase 2: Implementar Topic de Alertas ⚠️

#### 2.1 Crear topic `correlated.alerts`
```bash
docker exec kafka kafka-topics --bootstrap-server localhost:9092 \
  --create --topic correlated.alerts \
  --partitions 2 \
  --replication-factor 1 \
  --config retention.ms=2592000000 \
  --config compression.type=lz4
```

**Configuración**:
- **Particiones**: 2 (alertas son menos volumen que eventos)
- **Retention**: 30 días (para análisis histórico)
- **Compression**: LZ4 (máxima velocidad para alertas urgentes)

#### 2.2 Modificar Consumer para publicar alertas
```javascript
// En consumer.js
const producer = kafka.producer();

async function publicarAlertas(event, alertas) {
  if (alertas.length === 0) return;
  
  const alertMessage = {
    alert_id: uuidv4(),
    correlation_id: event.correlation_id,
    source_event_id: event.event_id,
    zone: event.geo.zone,
    timestamp: new Date().toISOString(),
    alerts: alertas.map(a => ({
      level: a.nivel,
      type: a.tipo,
      message: a.mensaje,
      details: a.detalles
    }))
  };
  
  await producer.send({
    topic: 'correlated.alerts',
    messages: [{ value: JSON.stringify(alertMessage) }]
  });
}
```

---

### Fase 3: Implementar Dead Letter Queue ❌

#### 3.1 Crear topic `events.dlq`
```bash
docker exec kafka kafka-topics --bootstrap-server localhost:9092 \
  --create --topic events.dlq \
  --partitions 1 \
  --replication-factor 1 \
  --config retention.ms=1209600000 \
  --config cleanup.policy=delete
```

**Configuración**:
- **Particiones**: 1 (eventos fallidos son excepciones)
- **Retention**: 14 días (suficiente para debugging)
- **Cleanup**: Delete (eliminar después de retention)

#### 3.2 Actualizar Backend para manejar errores
```csharp
// En EventsController.cs
public async Task<IActionResult> Post()
{
    try
    {
        var validationResult = _validator.ValidateDetailed(eventData);
        if (!validationResult.isValid)
        {
            // Publicar a DLQ con información de error
            var dlqMessage = new {
                original_event = eventData,
                validation_errors = validationResult.errors,
                timestamp = DateTime.UtcNow,
                reason = "SCHEMA_VALIDATION_FAILED"
            };
            await _producer.Publish("events.dlq", JsonConvert.SerializeObject(dlqMessage));
            
            return BadRequest(new { message = "Invalid schema", errors = validationResult.errors });
        }
        
        // Evento válido → events.standardized
        await _producer.Publish("events.standardized", eventData.ToString());
        
    }
    catch (Exception ex)
    {
        // Error de procesamiento → DLQ
        var dlqMessage = new {
            original_event = eventData,
            exception = ex.Message,
            timestamp = DateTime.UtcNow,
            reason = "PROCESSING_ERROR"
        };
        await _producer.Publish("events.dlq", JsonConvert.SerializeObject(dlqMessage));
        throw;
    }
}
```

---

## 📊 Configuración de Topics para Laboratorio

### Tabla de Configuración Recomendada

| Topic | Particiones | RF | Retention | Compression | Min ISR |
|-------|-------------|----|-----------| ------------|---------|
| events.standardized | 3 | 1 | 7d | snappy | 1 |
| correlated.alerts | 2 | 1 | 30d | lz4 | 1 |
| events.dlq | 1 | 1 | 14d | gzip | 1 |

**Notas**:
- **RF=1** para laboratorio (usar 3 en producción)
- **Min ISR=1** por tener RF=1
- **Compression** elegida según caso de uso

---

## 🔍 Verificación Post-Implementación

### Comandos de Verificación

```bash
# 1. Listar todos los topics
docker exec kafka kafka-topics --bootstrap-server localhost:9092 --list

# Esperado:
# events.standardized
# correlated.alerts
# events.dlq

# 2. Describir configuración de cada topic
docker exec kafka kafka-topics --bootstrap-server localhost:9092 \
  --describe --topic events.standardized

# 3. Verificar mensajes en cada topic
docker exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic events.standardized \
  --from-beginning --max-messages 5

docker exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic correlated.alerts \
  --from-beginning --max-messages 5

docker exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic events.dlq \
  --from-beginning --max-messages 5
```

---

## 📈 Métricas a Monitorear

### Por Topic

**events.standardized:**
- Tasa de ingesta (eventos/seg)
- Lag del consumer
- Distribución por tipo de evento

**correlated.alerts:**
- Alertas CRÍTICAS/ALTAS generadas
- Tiempo de correlación
- Alertas por zona

**events.dlq:**
- Tasa de errores
- Tipos de errores más comunes
- Eventos que necesitan corrección

---

## 🎯 Checklist de Implementación

### Básico (Mínimo Viable)
- [ ] Renombrar `events-topic` a `events.standardized`
- [ ] Crear topic `correlated.alerts`
- [ ] Crear topic `events.dlq`
- [ ] Actualizar producer para usar nuevos nombres
- [ ] Actualizar consumer para usar nuevos nombres

### Intermedio (Recomendado)
- [ ] Consumer publica alertas a `correlated.alerts`
- [ ] Backend publica errores a `events.dlq`
- [ ] Configurar retentions apropiadas
- [ ] Configurar particiones según carga

### Avanzado (Opcional)
- [ ] Implementar consumer para DLQ (reintento automático)
- [ ] Implementar consumer para alertas (notificaciones)
- [ ] Guardar alertas en tabla `alerts` de PostgreSQL
- [ ] Dashboard para visualizar métricas de topics
- [ ] Alerting cuando DLQ crece mucho

---

## 💡 Recomendaciones Adicionales

### 1. Esquema de Nomenclatura
```
{sistema}.{tipo}.{version}

Ejemplos:
- smartcity.events.standardized.v1
- smartcity.alerts.correlated.v1
- smartcity.events.dlq.v1
```

### 2. Consumer Groups
```
- events-processor-group    → Lee de events.standardized
- alert-detector-group      → Lee de events.standardized, publica a correlated.alerts
- alert-notifier-group      → Lee de correlated.alerts
- dlq-monitor-group         → Lee de events.dlq
```

### 3. Políticas de Retry
```javascript
// En producer con retry
const producer = kafka.producer({
  retry: {
    retries: 3,
    initialRetryTime: 100,
    factor: 2,
    maxRetryTime: 30000
  }
});
```

---

## 📝 Conclusión

### Estado Actual: ⚠️ FUNCIONAL PERO INCOMPLETO

**Lo que funciona:**
- ✅ Flujo básico de eventos
- ✅ Validación de esquemas
- ✅ Detección de alertas
- ✅ Persistencia en DB

**Lo que falta para cumplir especificación:**
- ❌ Topic `events.standardized` (usando `events-topic`)
- ❌ Topic `correlated.alerts` (alertas solo en consola)
- ❌ Topic `events.dlq` (sin manejo de errores)

**Esfuerzo de implementación**: ~4-6 horas
**Prioridad**: ALTA (requerimiento del proyecto)
**Complejidad**: MEDIA

---

**Próximo paso recomendado**: Implementar los 3 topics requeridos siguiendo la Fase 1, 2 y 3 del plan de implementación.

**Fecha de análisis**: 9 de octubre de 2025  
**Autor**: Análisis de cumplimiento de requerimientos Kafka
