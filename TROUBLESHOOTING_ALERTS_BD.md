# 🔧 Solución: Alertas no se Persisten en Base de Datos

## ❌ Problema Reportado
"Las alertas no están persistiendo en mi BD"

## ✅ Diagnóstico Completado

### Causas Identificadas
1. **Backend no tenía AlertsController compilado** ✅ SOLUCIONADO
2. **Imagen de Docker desactualizada** ✅ SOLUCIONADO
3. **Consumer no está corriendo** ⚠️ REQUIERE ACCIÓN DEL USUARIO

---

## 🛠️ Solución Aplicada

### Paso 1: Rebuild del Backend ✅
```bash
docker compose build backend
docker compose up -d backend
```

**Resultado:** 
- Build exitoso: 0 errores
- AlertsController ahora disponible en `/alerts`
- Modelo `Alert` configurado correctamente

### Paso 2: Verificación del Endpoint ✅
```bash
curl http://localhost:5000/alerts/stats
```

**Respuesta:**
```json
{
  "total": 1,
  "last_24h": 1,
  "by_type": [{"type": "EXCESO DE VELOCIDAD PELIGROSO", "count": 1}],
  "by_zone": [{"zone": "Zona 10", "count": 1}],
  "avg_score": 100.0
}
```

✅ **Endpoint funcionando correctamente**

---

## 📋 Para que las Alertas se Guarden Automáticamente

### Necesitas Correr el Consumer

El consumer es el que:
1. Lee eventos de `events.standardized`
2. Detecta alertas (16 tipos)
3. Publica a `correlated.alerts`
4. **Guarda en PostgreSQL via POST /alerts** ← Esto faltaba

**Comando:**
```bash
cd js-scripts
npm run consumer
```

**Output esperado:**
```
Consumer iniciado - Sistema de alertas inteligente activado
Monitoreando: eventos de pánico, velocidad, acústicos y reportes ciudadanos
Publicando alertas a: correlated.alerts

[INFO] Evento recibido: sensor.lpr | Zona: Zona 10
🚨 ALERTAS DETECTADAS 🚨
[CRÍTICO] EXCESO DE VELOCIDAD PELIGROSO
  → Vehículo a 120 km/h detectado
✓ Alertas publicadas a correlated.alerts
✓ Alertas guardadas en BD: 2 alerta(s)  ← ESTO CONFIRMA QUE SE GUARDÓ
```

---

## 🚀 Inicio Rápido (Recomendado)

### Opción 1: Script Automático
```powershell
.\start.ps1
```

Este script:
- ✅ Verifica Docker
- ✅ Inicia infraestructura
- ✅ Verifica topics de Kafka
- ✅ Verifica endpoint de alertas
- ✅ Te da instrucciones claras

### Opción 2: Manual (2 Terminales Mínimo)

**Terminal 1: Producer**
```bash
cd js-scripts
npm run producer
```

**Terminal 2: Consumer** (este guarda las alertas)
```bash
cd js-scripts
npm run consumer
```

Espera 30 segundos y verifica:
```powershell
Invoke-RestMethod http://localhost:5000/alerts/stats
```

Deberías ver alertas incrementando.

---

## 📊 Verificación

### Ver Estadísticas
```powershell
Invoke-RestMethod http://localhost:5000/alerts/stats
```

**Esperado:**
```json
{
  "total": 45,
  "last_24h": 45,
  "by_type": [
    {"type": "EXCESO DE VELOCIDAD PELIGROSO", "count": 12},
    {"type": "DISPARO DETECTADO", "count": 8},
    {"type": "EMERGENCIA PERSONAL", "count": 7}
  ],
  "by_zone": [
    {"zone": "Zona 10", "count": 45}
  ],
  "avg_score": 75.5
}
```

### Ver Últimas 10 Alertas
```powershell
Invoke-RestMethod "http://localhost:5000/alerts?take=10"
```

### Ver Alertas en PostgreSQL (Neon Cloud)
```sql
-- Conectar a Neon Console SQL Editor

-- Total de alertas
SELECT COUNT(*) FROM alerts;

-- Últimas 10 alertas con detalles
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

-- Alertas por tipo
SELECT 
  type,
  COUNT(*) as count,
  AVG(score) as avg_score
FROM alerts
GROUP BY type
ORDER BY count DESC;
```

---

## 🔍 Troubleshooting Adicional

### Problema: Consumer muestra "Error guardando en BD"

**Diagnóstico:**
```bash
docker logs backend --tail 50
```

**Soluciones:**
1. Verificar que backend esté corriendo: `docker ps`
2. Verificar endpoint: `curl http://localhost:5000/alerts/stats`
3. Rebuild del backend: `docker compose build backend && docker compose up -d backend`

---

### Problema: Consumer no detecta eventos

**Diagnóstico:**
```bash
# Ver eventos en el topic
docker exec kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic events.standardized \
  --from-beginning --max-messages 5
```

**Soluciones:**
1. Verificar que Producer esté corriendo
2. Verificar que Backend publique a `events.standardized` (no `events-topic`)
3. Reiniciar consumer con `fromBeginning: true`

---

### Problema: Backend retorna 404 en /alerts

**Causa:** AlertsController no está compilado en la imagen Docker

**Solución:**
```bash
docker compose build backend
docker compose up -d backend
```

---

### Problema: Backend retorna 500 en POST /alerts

**Diagnóstico:**
```bash
docker logs backend --tail 50 | grep "Error"
```

**Causas comunes:**
1. Tabla `alerts` no existe → Ejecutar `database/init-neon.sql` en Neon Console
2. Error de conexión a BD → Verificar `appsettings.json`
3. JSON mal formado → Verificar formato del consumer

---

## ✅ Checklist de Funcionamiento

- [x] Backend compilado con AlertsController
- [x] Endpoint `/alerts` responde 200
- [x] Endpoint `/alerts/stats` retorna datos
- [x] Tabla `alerts` existe en PostgreSQL
- [ ] **Producer corriendo** (genera eventos)
- [ ] **Consumer corriendo** (guarda alertas) ← **FALTA ESTO**
- [ ] Alertas incrementando en `/alerts/stats`

---

## 📈 Flujo Completo Funcionando

```
Producer (JS)
    ↓ POST /events
Backend API
    ↓ Publica a Kafka
events.standardized
    ↓
Consumer (JS) ← ESTE DEBE ESTAR CORRIENDO
    ↓ Detecta alertas
    ↓ POST /alerts
Backend API
    ↓
PostgreSQL: tabla alerts ✅
```

---

## 🎯 Resumen

### Lo que YA funciona ✅
- ✅ Backend con AlertsController
- ✅ Endpoint POST /alerts (probado manualmente)
- ✅ Endpoint GET /alerts/stats
- ✅ Tabla `alerts` en PostgreSQL
- ✅ Guardado de alertas funcional

### Lo que FALTA ⚠️
- ⚠️ **Iniciar el Consumer** para que procese eventos automáticamente

### Solución Inmediata
```bash
# Terminal 1
cd js-scripts
npm run producer

# Terminal 2 (IMPORTANTE - este guarda las alertas)
cd js-scripts
npm run consumer

# Esperar 30 segundos, luego verificar:
# Terminal 3
Invoke-RestMethod http://localhost:5000/alerts/stats
```

---

**Fecha:** 9 de octubre de 2025  
**Problema:** Alertas no persistiendo en BD  
**Estado:** ✅ SOLUCIONADO (requiere iniciar consumer)  
**Documentos relacionados:** ALERTS_PERSISTENCE.md, start.ps1
