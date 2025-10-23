# Smart City - Guía de Inicio Rápido (Universidad)

## 🚀 Configuración Inicial (5 minutos)

### Requisitos Previos
- ✅ Docker Desktop instalado
- ✅ Node.js 16+ instalado
- ✅ Git instalado
- ✅ Editor de código (VS Code recomendado)

---

## Paso 1: Clonar y Configurar

```powershell
# Clonar repositorio
git clone https://github.com/4901oscar/smart-city-project.git
cd smart-city-project

# Verificar que .env existe (ya está configurado para Neon Cloud)
cat .env
```

---

## Paso 2: Inicializar Base de Datos (Una sola vez)

### Opción A: Neon Cloud (Recomendado - Ya configurado)
1. Ir a: https://console.neon.tech
2. Login con credenciales del proyecto
3. Abrir SQL Editor
4. Copiar y ejecutar: `database/init-neon.sql`
5. Verificar: `SELECT COUNT(*) FROM events;` debe retornar 0

### Opción B: PostgreSQL Local (Alternativa)
```powershell
# Descomentar sección PostgreSQL en docker-compose.yml
# Descomentar configuración local en .env
# Luego:
docker-compose up -d postgres
# Ejecutar init-neon.sql en el contenedor
```

---

## Paso 3: Levantar Infraestructura

```powershell
# Iniciar todos los servicios
docker-compose up -d

# Verificar que estén corriendo
docker ps

# Deberías ver:
# - backend (puerto 5000)
# - kafka (puerto 9092)
# - zookeeper (puerto 2181)

# Ver logs del backend
docker logs backend

# Buscar: "✓ Conexión a la base de datos establecida correctamente"
```

---

## Paso 4: Probar API

```powershell
# Abrir Swagger UI en navegador
start http://localhost:5000

# Probar health check
curl http://localhost:5000/health

# Enviar evento de prueba
curl -X POST http://localhost:5000/events -H "Content-Type: application/json" -d @test-event.json

# Ver eventos guardados
curl http://localhost:5000/events?take=5
```

---

## Paso 5: Iniciar Simulación de Eventos

```powershell
# Terminal 1: Instalar dependencias JavaScript
cd js-scripts
npm install

# Iniciar productor (genera eventos cada 3 segundos)
npm run producer

# Deberías ver:
# ✓ [panic.button] Severity: critical - Evento enviado...
# ✓ [sensor.lpr] Severity: warning - Evento enviado...
```

---

## Paso 6: Monitorear Alertas

```powershell
# Terminal 2: Iniciar consumer (detector de alertas)
cd js-scripts
npm run consumer

# Deberías ver alertas en color:
# ================================================================================
# 🚨 ALERTAS DETECTADAS 🚨
# Zona: Zona 10 | Coords: 14.6091, -90.5252
# --------------------------------------------------------------------------------
# [CRÍTICO] DISPARO DETECTADO
#   → Posible disparo de arma de fuego (87.5% confianza)
#   → 145 dB - Requiere unidad policial inmediata
# ================================================================================
```

---

## 🎯 ¡Sistema Funcionando!

Ahora tienes:
- ✅ Backend API procesando eventos
- ✅ Kafka streaming eventos
- ✅ PostgreSQL guardando eventos
- ✅ Producer generando eventos realistas
- ✅ Consumer detectando alertas inteligentes

---

## 📊 Verificación del Sistema

### Checklist Rápido
```powershell
# 1. Backend responde
curl http://localhost:5000/health
# Esperado: {"status":"Healthy"}

# 2. Eventos se están guardando
curl http://localhost:5000/events?take=1
# Esperado: JSON con al menos 1 evento

# 3. Kafka funciona
docker exec -it kafka kafka-topics --bootstrap-server localhost:9092 --list
# Esperado: events-topic

# 4. Base de datos tiene eventos
# En Neon Console SQL Editor:
SELECT COUNT(*) FROM events;
# Esperado: número > 0 y creciendo
```

---

## 🔧 Comandos Útiles

### Reiniciar Todo
```powershell
docker-compose down
docker-compose up -d
```

### Ver Logs en Tiempo Real
```powershell
# Backend
docker logs backend -f

# Kafka
docker logs kafka -f

# Todos
docker-compose logs -f
```

### Limpiar Eventos de Prueba
```sql
-- En Neon Console SQL Editor
TRUNCATE TABLE events;
TRUNCATE TABLE alerts;
```

### Detener Todo
```powershell
# Detener servicios pero mantener datos
docker-compose stop

# Detener y eliminar contenedores (mantiene volúmenes)
docker-compose down

# Eliminar TODO (incluyendo volúmenes)
docker-compose down -v
```

---

## 📚 Tipos de Eventos Soportados

### 1. Botón de Pánico
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
**Alertas**: EMERGENCIA PERSONAL (CRÍTICO)

### 2. Cámara LPR (Lectura de Placas)
```json
{
  "event_type": "sensor.lpr",
  "payload": {
    "placa_vehicular": "P-456AB",
    "velocidad_estimada": 105,
    "modelo_vehiculo": "Toyota Corolla",
    "color_vehiculo": "rojo",
    "ubicacion_sensor": "Av. Reforma"
  }
}
```
**Alertas**: EXCESO DE VELOCIDAD PELIGROSO (CRÍTICO si >100 km/h)

### 3. Sensor de Velocidad
```json
{
  "event_type": "sensor.speed",
  "payload": {
    "velocidad_detectada": 85,
    "sensor_id": "SPEED-Z10-003",
    "direccion": "Norte"
  }
}
```
**Alertas**: VELOCIDAD EXCESIVA (ALTO si >80 km/h)

### 4. Sensor Acústico
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
**Alertas**: DISPARO DETECTADO (CRÍTICO)

### 5. Reporte Ciudadano
```json
{
  "event_type": "citizen.report",
  "payload": {
    "tipo_evento": "accidente",
    "mensaje_descriptivo": "Choque en intersección",
    "ubicacion_aproximada": "6ta Avenida y 12 calle",
    "origen": "app"
  }
}
```
**Alertas**: ACCIDENTE REPORTADO (ALTO)

---

## 🎓 Para Presentaciones/Demos

### Demo Script (3 minutos)

```powershell
# 1. Mostrar Swagger UI
start http://localhost:5000

# 2. En Swagger, ejecutar GET /events?take=10
# Explicar: "Estos son los eventos recientes en el sistema"

# 3. En Terminal 1, mostrar producer corriendo
# Explicar: "Este productor simula sensores reales enviando eventos"

# 4. En Terminal 2, mostrar consumer con alertas
# Explicar: "El consumer analiza eventos y genera alertas en tiempo real"

# 5. Enviar evento crítico manualmente
curl -X POST http://localhost:5000/events -d @test-event.json

# 6. Ver alerta generada en consumer
# Explicar: "La alerta CRÍTICA se detectó inmediatamente"

# 7. Verificar en DB (Neon Console)
SELECT event_type, severity, ts_utc 
FROM events 
ORDER BY ts_utc DESC 
LIMIT 10;
```

### Puntos Clave para Explicar
1. **Event-Driven Architecture**: Kafka desacopla productores y consumidores
2. **Schema Validation**: Todos los eventos validados contra JSON schemas
3. **Intelligent Alerts**: 15+ tipos de alertas basadas en reglas
4. **Scalability**: Kafka puede manejar millones de eventos/día
5. **Persistence**: PostgreSQL JSONB para queries flexibles

---

## ❓ Troubleshooting Común

### Problema: "Cannot connect to Docker daemon"
```powershell
# Solución: Iniciar Docker Desktop
# Verificar en system tray que Docker está corriendo
```

### Problema: "Port 5000 already in use"
```powershell
# Solución 1: Detener proceso en puerto 5000
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Solución 2: Cambiar puerto en docker-compose.yml
ports:
  - "5001:8080"  # Cambiar 5000 a 5001
```

### Problema: "Connection refused to Kafka"
```powershell
# Solución: Esperar que Kafka termine de iniciar (30 segundos)
docker logs kafka

# Buscar: "started (kafka.server.KafkaServer)"
```

### Problema: "Database connection failed"
```powershell
# Verificar credenciales en .env
cat .env

# Verificar en Neon Console que la base de datos está activa
# Ejecutar: SELECT 1;  para probar conexión
```

### Problema: "npm install fails"
```powershell
# Limpiar cache de npm
npm cache clean --force
rm -rf node_modules package-lock.json
npm install
```

---

## 📖 Documentación Adicional

- **Testing Completo**: Ver `TESTING.md`
- **Referencia de Alertas**: Ver `ALERTS_REFERENCE.md`
- **Compliance Specs**: Ver `COMPLIANCE.md`
- **AI Instructions**: Ver `.github/copilot-instructions.md`

---

## 🎯 Próximos Pasos

1. ✅ Sistema funcionando localmente
2. 📝 Revisar `TESTING.md` para pruebas exhaustivas
3. 🎨 Personalizar zonas geográficas en `.env`
4. 📊 Explorar Swagger UI para todos los endpoints
5. 🔍 Revisar logs para entender el flujo de datos
6. 🚀 ¡Experimentar con eventos personalizados!

---

**¿Necesitas ayuda?** Revisa los logs:
```powershell
docker-compose logs -f backend
```

**¿Todo funciona?** ¡Felicidades! 🎉 El sistema está listo para demos y desarrollo.



PARAR PROCESOS PRODUCER Y CONSUMER

Stop-Process -Name "node" -Force