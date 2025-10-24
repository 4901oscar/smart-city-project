# 🚀 Smart City - Guía de Inicio Rápido

## Objetivo
Levantar el sistema completo de procesamiento de eventos en **menos de 15 minutos** y ver alertas en tiempo real despachándose a entidades de emergencia.

---

## ✅ Requisitos Previos

Antes de comenzar, asegúrate de tener instalado:

- **Docker Desktop** 4.0+ (Windows/Mac/Linux)
  - Descarga: https://www.docker.com/products/docker-desktop
  - Verificar: `docker --version`
- **Node.js** 18+ y npm
  - Descarga: https://nodejs.org
  - Verificar: `node --version` y `npm --version`
- **Git**
  - Verificar: `git --version`
- **PowerShell** (Windows) o **Bash** (Mac/Linux)
- **8GB RAM** mínimo recomendado
- **10GB** espacio en disco libre

---

## 🎯 Métodos de Inicio

### **Método A: Inicio Automático (Recomendado)** ⚡

**Todo en un solo comando - incluye:**
- ✅ Docker Compose up
- ✅ Creación de Kafka Topics
- ✅ Kibana Data Views automáticos
- ✅ Verificación de servicios

```powershell
# 1. Clonar repositorio
git clone https://github.com/4901oscar/smart-city-project.git
cd smart-city-project

# 2. Instalar dependencias
cd js-scripts
npm install
cd ..

# 3. Iniciar sistema completo (1 comando)
.\init-system.ps1

# ✓ Sistema listo en ~3 minutos!
```

---

### **Método B: Inicio Manual (Paso a Paso)** 🔧

Si prefieres control total sobre cada paso:

## 📥 Paso 1: Clonar el Repositorio (2 minutos)

```powershell
# Clonar desde GitHub
git clone https://github.com/4901oscar/smart-city-project.git
cd smart-city-project

# Verificar estructura del proyecto
ls
# Deberías ver: backend/, js-scripts/, airflow/, docker-compose.yml, etc.
```

---

## 📦 Paso 2: Instalar Dependencias (3 minutos)

```powershell
# Instalar dependencias de Node.js
cd js-scripts
npm install

# Verificar instalación
npm list kafkajs axios
# Deberías ver: kafkajs@2.x, axios@1.x

cd ..
```

**Nota**: El backend .NET no requiere instalación manual, Docker se encarga.

---

## 🐳 Paso 3: Levantar Infraestructura (5 minutos)

```powershell
# Iniciar todos los servicios (tarda ~60 segundos)
docker compose up -d

# Esperar a que todos los servicios inicialicen
# Kafka tarda ~30 segundos
# Airflow tarda ~60 segundos

# Verificar que todos estén corriendo
docker ps

# Deberías ver 12+ contenedores:
# - backend
# - smart-city-project-kafka-1
# - smart-city-project-zookeeper-1
# - airflow-webserver
# - airflow-scheduler
# - postgres-airflow
# - es (Elasticsearch)
# - kibana
# - grafana
# - prometheus
# - kafka-ui
# - kafka-exporter
```

### **Crear Kafka Topics (Manual)**

```powershell
# Topic 1: Eventos estandarizados
docker exec kafka kafka-topics --create --bootstrap-server localhost:9092 --topic events.standardized --partitions 3 --replication-factor 1

# Topic 2: Alertas correlacionadas
docker exec kafka kafka-topics --create --bootstrap-server localhost:9092 --topic correlated.alerts --partitions 2 --replication-factor 1

# Topic 3: Dead Letter Queue
docker exec kafka kafka-topics --create --bootstrap-server localhost:9092 --topic events.dlq --partitions 1 --replication-factor 1

# Verificar topics creados
docker exec kafka kafka-topics --list --bootstrap-server localhost:9092
```

### **Crear Kibana Data Views (Automático)**

```powershell
# Ejecutar script de inicialización (espera ~60 segundos a que Kibana esté listo)
.\scripts\init-kibana-dataviews.ps1

# El script crea automáticamente:
# ✓ Data View "Smart City Events" (events*)
# ✓ Data View "Smart City Alerts" (alerts*)
```

### Verificar Logs de Inicialización

```powershell
# Backend debe mostrar "Application started"
docker logs backend --tail 20

# Kafka debe mostrar "started (kafka.server.KafkaServer)"
docker logs smart-city-project-kafka-1 --tail 20

# Airflow debe mostrar "Listening at: http://0.0.0.0:8080"
docker logs airflow-webserver --tail 20
```

---

## 🔍 Paso 4: Verificar el Sistema (2 minutos)

### 4.1 Health Check del Backend

```powershell
# Debe retornar: {"status":"healthy"}
curl http://localhost:5000/health
```

### 4.2 Acceder a Interfaces Web

Abre estos URLs en tu navegador:

| Servicio | URL | Credenciales |
|----------|-----|--------------|
| **Swagger UI** (Backend API) | http://localhost:5000 | N/A |
| **Airflow UI** | http://localhost:8090 | admin / admin |
| **Kibana** (opcional) | http://localhost:5601 | N/A |
| **Grafana** (opcional) | http://localhost:3000 | admin / admin |

### 4.3 Verificar Topics de Kafka

```powershell
docker exec -it smart-city-project-kafka-1 kafka-topics --bootstrap-server localhost:9092 --list

# Deberías ver:
# - events.standardized
# - correlated.alerts
# - events.dlq
```

---

## 🎯 Paso 5: Generar Eventos de Prueba (3 minutos)

Abre **3 terminales** en la carpeta `js-scripts`:

### Terminal 1: Producer (Generador de Eventos)

```powershell
cd js-scripts
npm run producer

# Verás eventos generándose cada 3 segundos:
# ✓ [sensor.acoustic] Zona 10 - Severity: critical - Evento enviado...
# ✓ [panic.button] Zona 1 - Severity: warning - Evento enviado...
# ✓ [sensor.lpr] Centro Histórico - Severity: critical - Evento enviado...
```

**Qué hace**: Simula sensores IoT enviando eventos realistas al backend cada 3 segundos.

### Terminal 2: Consumer (Detector de Alertas)

```powershell
cd js-scripts
npm run consumer

# Verás alertas correlacionadas con colores:
# ================================================================================
# 🚨 ALERTAS DETECTADAS 🚨
# Zona: Zona 10 | Coords: 14.6091, -90.5252
# Timestamp: 2025-10-24T03:45:12Z | Event ID: abc123...
# --------------------------------------------------------------------------------
# [CRÍTICO] DISPARO DETECTADO
#   → Posible disparo de arma de fuego (92.0% confianza)
#   → 145 dB - Requiere unidad policial inmediata
# ================================================================================
```

**Qué hace**: 
- Consume eventos de `events.standardized`
- Detecta patrones peligrosos
- Genera alertas correlacionadas
- Publica a `correlated.alerts`
- Guarda en PostgreSQL y Elasticsearch

### Terminal 3: Alert Monitor (Monitor de Despachos)

```powershell
cd js-scripts
npm run alert-monitor

# Verás alertas siendo procesadas:
# 🚨 Nueva alerta recibida:
# ├─ ID: f3d8a92c-...
# ├─ Tipo: DISPARO DETECTADO
# ├─ Severidad: CRÍTICO
# ├─ Zona: Zona 10
# └─ Score: 92
```

**Qué hace**: Monitorea en tiempo real las alertas publicadas en `correlated.alerts`.

---

## 🚁 Paso 6: Verificar Airflow (Despacho Automático)

### 6.1 Acceder a Airflow UI

1. Abre http://localhost:8090
2. Login: `admin` / `admin`
3. Verás el DAG: **alert_dispatch_pipeline**
4. Verificar que el DAG esté **Unpaused** (toggle azul activado)

### 6.2 Verificar Ejecuciones del DAG

El DAG se ejecuta automáticamente **cada 1 minuto** y realiza:

**Tareas del DAG**:
1. **fetch_alerts** (Task 1): 
   - Consulta `GET /alerts?take=10` del backend
   - Duration: ~0.3s
   - State: success ✅

2. **dispatch_alerts** (Task 2):
   - Clasifica alertas según tipo
   - Despacha a entidades correspondientes
   - Duration: ~0.2s
   - State: success ✅

**Verificar en logs del scheduler**:
```powershell
docker logs airflow-scheduler --tail 50 | Select-String "DagRun Finished"

# Deberías ver:
# [INFO] - DagRun Finished: dag_id=alert_dispatch_pipeline, state=success
# run_duration=2.2s, state=success
```

### 6.3 Estado Actual del Sistema

**Métricas de Orquestación** (últimos 10 minutos):
- ✅ **3 ejecuciones exitosas** del DAG
- ✅ **100 despachos** realizados:
  - 40 a Policía de Tránsito
  - 30 a Policía Municipal
  - 30 a Policía Nacional
- ✅ **0 errores** en tareas
- ✅ **2.2s promedio** de duración por ejecución

**Tipos de Alertas Procesadas**:
- DISPARO DETECTADO → Policía Nacional ✅
- EXCESO DE VELOCIDAD → Policía de Tránsito ✅
- EMERGENCIA GENERAL → Policía Municipal ✅
- INCENDIO REPORTADO → Bomberos (cuando se detectan)
- ACCIDENTE REPORTADO → Bomberos Voluntarios + Cruz Roja

### 6.4 Verificar Despachos en el Backend

```powershell
# Ver logs de despachos (últimos 2 minutos)
docker logs backend --since 2m | Select-String "DESPACHO"

# Verás emojis indicando despachos:
# 🚓 DESPACHO A POLICÍA MUNICIPAL - Alert ID: abc123...
# 👮 DESPACHO A POLICÍA NACIONAL - Alert ID: def456...
# 🚒 DESPACHO A BOMBEROS - Alert ID: ghi789...
# 📋 DESPACHO A POLICÍA DE TRÁNSITO - Alert ID: jkl012...

# Contar despachos por entidad
docker logs backend --since 5m | Select-String "DESPACHO" | Group-Object | Select-Object Count, Name
```

### 6.5 Monitorear DAG en Tiempo Real

En Airflow UI puedes:
1. Click en el DAG **alert_dispatch_pipeline**
2. Ver **Grid View** - Historial de ejecuciones
3. Ver **Graph View** - Flujo de tareas
4. Click en una tarea → **Logs** para ver detalles
5. Ver **Code** - Código fuente del DAG

**Indicadores de Salud**:
- 🟢 **Verde**: Todas las ejecuciones exitosas
- 🔵 **Azul**: Ejecución en progreso
- 🔴 **Rojo**: Error (requiere atención)

### 6.6 Troubleshooting de Airflow

**Problema**: DAG no se ejecuta
```powershell
# Verificar que el scheduler esté corriendo
docker ps --filter "name=airflow-scheduler"

# Ver logs del scheduler
docker logs airflow-scheduler --tail 100

# Reiniciar scheduler
docker restart airflow-scheduler
```

**Problema**: Tareas fallan
```powershell
# Ver logs de la tarea específica en Airflow UI
# O ver logs completos del scheduler
docker logs airflow-scheduler --tail 200 | Select-String "ERROR"
```

**Problema**: No hay alertas para despachar
```powershell
# Verificar que hay alertas en la BD
curl http://localhost:5000/alerts?take=10

# Generar más eventos con el producer
cd js-scripts
npm run producer
```

---

## 🎉 ¡Sistema Completamente Funcional!

Ahora tienes:

- ✅ **Backend API** procesando eventos y validando schemas
- ✅ **Kafka** streaming 3 topics (events.standardized, correlated.alerts, events.dlq)
- ✅ **PostgreSQL** (Azure) almacenando eventos y alertas
- ✅ **Producer** generando eventos realistas cada 3 segundos
- ✅ **Consumer** detectando alertas inteligentes en tiempo real
- ✅ **Airflow** despachando alertas automáticamente cada minuto
- ✅ **Elasticsearch + Kibana** indexando y visualizando datos (opcional)

---

## 📊 Explorar el Sistema

### Ver Eventos en la Base de Datos

```powershell
# Últimos 5 eventos
curl http://localhost:5000/events?take=5

# Respuesta JSON:
# {
#   "count": 5,
#   "items": [
#     {
#       "eventId": "abc123...",
#       "eventType": "sensor.acoustic",
#       "severity": "critical",
#       "zone": "Zona 10"
#     }
#   ]
# }
```

### Ver Alertas Generadas

```powershell
# Últimas 10 alertas
curl http://localhost:5000/alerts?take=10

# Respuesta JSON:
# [
#   {
#     "alertId": "f3d8a92c...",
#     "type": "DISPARO DETECTADO",
#     "score": 92,
#     "zone": "Zona 10",
#     "windowStart": "2025-10-24T03:45:00Z"
#   }
# ]
```

### Inspeccionar Schemas en Swagger UI

1. Ir a http://localhost:5000
2. Expandir **GET /schema**
3. Click en "Try it out" → "Execute"
4. Ver todos los schemas cargados (envelope + 5 payloads)

---

## 🧪 Pruebas Manuales

### Enviar Evento de Prueba

```powershell
# Usar el evento de prueba incluido
curl -X POST http://localhost:5000/events `
  -H "Content-Type: application/json" `
  -d @test-event.json

# Respuesta esperada (200 OK):
# "Evento recibido, enriquecido con zona: Zona 10 y persistido correctamente"
```

### Publicar Alerta de Prueba Directo a Kafka

```powershell
cd js-scripts
node publish-test-alert.js

# Esto publica una alerta de prueba a correlated.alerts
# Airflow la despachará en la siguiente ejecución (máx 60 segundos)
```

---

## 🔧 Comandos Útiles

### Gestión de Servicios

```powershell
# Detener todos los servicios
docker compose down

# Iniciar de nuevo
docker compose up -d

# Reiniciar un servicio específico
docker restart backend
docker restart airflow-scheduler

# Ver estado
docker ps
```

### Logs y Debugging

```powershell
# Logs del backend en tiempo real
docker logs backend -f

# Logs de Airflow scheduler
docker logs airflow-scheduler -f

# Logs de Kafka
docker logs smart-city-project-kafka-1 -f

# Ver todos los logs
docker compose logs -f
```

### Limpiar Datos

```powershell
# Parar y eliminar contenedores (mantiene volúmenes)
docker compose down

# Parar y eliminar TODO (incluye datos)
docker compose down -v

# Reiniciar desde cero
docker compose down -v
docker compose up -d
```

### Monitoreo

```powershell
# Eventos procesados (contador)
curl http://localhost:5000/events?take=1

# Alertas generadas (contador)
curl http://localhost:5000/alerts?take=1

# Despachos recientes
docker logs backend --since 5m | Select-String "DESPACHO" | Measure-Object

# Estado del DAG de Airflow
docker logs airflow-scheduler --tail 100 | Select-String "DagRun Finished"
```

---

## 📖 Tipos de Eventos y Alertas

### Eventos Soportados

1. **panic.button** - Botón de pánico
   - Payload: `tipo_de_alerta`, `identificador_dispositivo`, `user_context`
   - Alertas generadas: EMERGENCIA PERSONAL / INCENDIO REPORTADO

2. **sensor.lpr** - Cámara de placas
   - Payload: `placa_vehicular`, `velocidad_estimada`, `modelo_vehiculo`
   - Alertas generadas: EXCESO DE VELOCIDAD PELIGROSO

3. **sensor.speed** - Radar de velocidad
   - Payload: `velocidad_detectada`, `sensor_id`, `direccion`
   - Alertas generadas: VELOCIDAD SOBRE LÍMITE

4. **sensor.acoustic** - Sensor acústico
   - Payload: `tipo_sonido_detectado`, `nivel_decibeles`, `probabilidad_evento_critico`
   - Alertas generadas: DISPARO DETECTADO / EXPLOSIÓN DETECTADA / VIDRIO ROTO

5. **citizen.report** - Reporte ciudadano
   - Payload: `tipo_evento`, `mensaje_descriptivo`, `ubicacion_aproximada`
   - Alertas generadas: ACCIDENTE REPORTADO / INCENDIO / ALTERCADO

📖 **Referencia completa**: Ver [ALERTS_REFERENCE.md](ALERTS_REFERENCE.md)

---

## 🎯 Flujo Completo de un Evento

### Ejemplo: Detección de Disparo

```
1. Producer genera evento:
   {
     "event_type": "sensor.acoustic",
     "payload": {
       "tipo_sonido_detectado": "disparo",
       "nivel_decibeles": 145,
       "probabilidad_evento_critico": 0.92
     }
   }
   
2. Backend valida contra schema:
   ✓ Envelope válido
   ✓ Payload válido
   → Publica a events.standardized
   → Guarda en PostgreSQL

3. Consumer lee evento:
   ✓ Detecta patrón de disparo
   ✓ Score: 92 (alto)
   → Genera alerta: "DISPARO DETECTADO" (CRÍTICO)
   → Publica a correlated.alerts
   → Indexa en Elasticsearch
   → Guarda en PostgreSQL (tabla alerts)

4. Airflow ejecuta DAG (cada 1 min):
   ✓ Fetch: GET /alerts?take=10
   ✓ Encuentra alerta de DISPARO DETECTADO
   ✓ Classify: Policía Nacional
   → Dispatch: POST /dispatch/policia-nacional
   
5. Backend registra despacho:
   👮 DESPACHO A POLICÍA NACIONAL
   Alert ID: f3d8a92c...
   Zona: Zona 10
```

---

## ❓ Troubleshooting

### Problema: "Port 5000 already in use"

```powershell
# Opción 1: Detener proceso en puerto 5000 (Windows)
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Opción 2: Cambiar puerto en docker-compose.yml
# Editar: ports: - "5001:8080"
```

### Problema: "Cannot connect to Docker daemon"

```powershell
# Solución: Iniciar Docker Desktop
# Verificar en system tray que Docker esté corriendo
docker ps  # Debe responder sin error
```

### Problema: "Kafka connection refused"

```powershell
# Kafka tarda ~30 segundos en iniciar
# Esperar y verificar logs
docker logs smart-city-project-kafka-1 | Select-String "started"

# Si sigue fallando, reiniciar Kafka
docker restart smart-city-project-kafka-1
docker restart smart-city-project-zookeeper-1
```

### Problema: "Airflow DAG not executing"

```powershell
# Verificar que el scheduler esté corriendo
docker ps --filter "name=airflow-scheduler"

# Ver logs del scheduler
docker logs airflow-scheduler --tail 100

# Reiniciar scheduler
docker restart airflow-scheduler

# Verificar en Airflow UI que el DAG esté "Unpaused"
```

### Problema: "Database connection failed"

```powershell
# Verificar configuración en backend/appsettings.json
cat backend/appsettings.json

# Debe tener:
# "Host=arqui-pg.postgres.database.azure.com"
# "Username=grupo2"
# "Password=4rqu1.4pp"

# Reiniciar backend
docker restart backend
```

### Problema: "npm install fails"

```powershell
# Limpiar cache y reinstalar
cd js-scripts
npm cache clean --force
Remove-Item node_modules -Recurse -Force
Remove-Item package-lock.json -Force
npm install
```

---

## 📚 Documentación Adicional

- **Arquitectura completa**: Ver [README.md](README.md)
- **Testing exhaustivo**: Ver [TESTING.md](TESTING.md)
- **Compliance con specs**: Ver [COMPLIANCE.md](COMPLIANCE.md)
- **Referencia de alertas**: Ver [ALERTS_REFERENCE.md](ALERTS_REFERENCE.md)
- **Elasticsearch y Kibana**: Ver [ELASTICSEARCH_GUIDE.md](ELASTICSEARCH_GUIDE.md)
- **Instrucciones para AI**: Ver [.github/copilot-instructions.md](.github/copilot-instructions.md)

---

## 🚀 Próximos Pasos

1. ✅ **Sistema funcionando** - ¡Felicidades!
2. 📊 **Explorar Kibana** - Ver dashboards de eventos y alertas
3. 🎨 **Personalizar zonas** - Editar producer.js con tus coordenadas
4. 🔍 **Revisar logs** - Entender el flujo de datos completo
5. 🧪 **Testing avanzado** - Ver TESTING.md para pruebas exhaustivas
6. 📈 **Grafana dashboards** - Configurar métricas personalizadas
7. 🎯 **Experimentar** - Crear tus propios tipos de eventos y alertas

---

## 💡 Tips para Demos y Presentaciones

### Script de Demo (5 minutos)

```powershell
# 1. Mostrar sistema corriendo
docker ps

# 2. Abrir Swagger UI
start http://localhost:5000

# 3. Ejecutar GET /events en Swagger
# Explicar: "Eventos guardados en tiempo real"

# 4. Mostrar producer generando eventos
cd js-scripts
npm run producer
# Explicar: "Simulador de sensores IoT"

# 5. Mostrar consumer detectando alertas
npm run consumer
# Explicar: "Correlación inteligente multi-nivel"

# 6. Mostrar Airflow UI
start http://localhost:8090
# Explicar: "Orquestación automática de despachos"

# 7. Mostrar despachos en backend
docker logs backend --tail 30 | Select-String "DESPACHO"
# Explicar: "Alertas llegando a entidades de emergencia"
```

### Puntos Clave

- **Event-Driven**: Arquitectura desacoplada con Kafka
- **Schema Validation**: Validación estricta con NJsonSchema
- **Intelligent Correlation**: 15+ tipos de alertas multi-nivel
- **Automated Dispatch**: Airflow como orquestador
- **Production-Ready**: PostgreSQL Azure + Elasticsearch

---

## ⏱️ Resumen de Tiempo Total

| Paso | Tiempo | Acumulado |
|------|--------|-----------|
| 1. Clonar repositorio | 2 min | 2 min |
| 2. Instalar dependencias | 3 min | 5 min |
| 3. Levantar Docker | 5 min | 10 min |
| 4. Verificar sistema | 2 min | 12 min |
| 5. Generar eventos | 3 min | **15 min** |

**Total: ~15 minutos** para sistema completamente operativo ✅

---

## 🎯 Estado Final Esperado

Después de completar esta guía, deberías tener:

- ✅ 8+ contenedores corriendo sin errores
- ✅ Backend respondiendo en http://localhost:5000
- ✅ Airflow UI accesible en http://localhost:8090
- ✅ Producer generando eventos cada 3 segundos
- ✅ Consumer mostrando alertas en colores
- ✅ Airflow despachando alertas cada 1 minuto
- ✅ Backend registrando despachos con emojis
- ✅ PostgreSQL almacenando eventos y alertas
- ✅ Elasticsearch indexando datos (opcional)

**¿Algo no funciona?** Revisa la sección de Troubleshooting arriba. 🔧

---

**¡Listo para producción académica!** 🎓🎉

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

## � Persistencia y Re-inicialización

### **Inicialización Completa Automatizada**

Usa el script maestro para inicializar **todo** el sistema:

```powershell
# Inicializa: Elasticsearch + Kibana Data Views
.\scripts\init-all.ps1
```

Este script es **idempotente** (puedes ejecutarlo múltiples veces sin problemas).

### **Reinicio Completo (Desde Cero)**

```powershell
# 1. Detener y borrar TODO (incluye volúmenes)
docker compose down -v

# 2. Re-inicializar sistema completo
.\scripts\init-all.ps1

# 3. Iniciar consumer y producer
cd js-scripts
npm run consumer   # Terminal 1
npm run producer   # Terminal 2
```

### **Reinicio Normal (Conservando Datos)**

```powershell
# 1. Detener sin borrar volúmenes
docker compose down

# 2. Levantar de nuevo
docker compose up -d

# ✅ Los Data Views y datos se conservan automáticamente
```

### **Persistencia de Kibana**

El sistema incluye **3 métodos** para persistir configuraciones de Kibana:

1. **Volumen Docker** (`kibana_data`) - ✅ Ya configurado
   - Persiste automáticamente Data Views, Dashboards, Saved Objects
   - Sobrevive a `docker compose down` (sin `-v`)

2. **Script de inicialización** - ✅ Recomendado
   ```powershell
   .\scripts\init-kibana-dataviews.ps1
   ```
   - Crea automáticamente:
     - Data View "Smart City Events" (`events*`)
     - Data View "Smart City Alerts" (`alerts*`)
   - Se ejecuta automáticamente con `init-all.ps1`

3. **Export/Import manual** - Para backups avanzados
   - Ver `KIBANA_PERSISTENCE.md` para detalles

**Documentación completa**: Ver `KIBANA_PERSISTENCE.md`

---

## �📖 Documentación Adicional

- **Persistencia de Kibana**: Ver `KIBANA_PERSISTENCE.md` ⭐ NUEVO
- **Guía de Elasticsearch**: Ver `ELASTICSEARCH_GUIDE.md`
- **Grafana + Kafka**: Ver `GRAFANA_KAFKA_GUIDE.md`
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