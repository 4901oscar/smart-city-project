# 🏙️ Smart City Event Processing System

## Sistema de Procesamiento de Eventos para Ciudad Inteligente

Sistema event-driven de monitoreo urbano que procesa eventos de sensores en tiempo real, genera alertas correlacionadas y despacha notificaciones a entidades de emergencia usando Apache Airflow.

### 🎯 Características Principales

- **Event-Driven Architecture**: Kafka como bus de eventos con 3 topics especializados
- **Validación de Schemas**: Validación estricta de eventos usando JSON Schema
- **Detección Inteligente de Alertas**: Sistema de correlación multi-nivel con 15+ tipos de alertas
- **Orquestación con Airflow**: Despacho automatizado de alertas a entidades de emergencia
- **Persistencia Dual**: PostgreSQL (Azure) + Elasticsearch para eventos y alertas
- **Visualización**: Kibana dashboards + Grafana para métricas en tiempo real
- **API REST**: Backend .NET 9 con Swagger UI

---

## 🚀 Quick Start (5 minutos)

### Prerrequisitos

- Docker Desktop (Windows/Mac/Linux)
- Node.js 18+ y npm
- Git
- (Opcional) .NET 9 SDK para desarrollo

### Instalación

**Opción A: Inicio Automático (Recomendado)** ⚡
```powershell
# 1. Clonar repositorio
git clone https://github.com/4901oscar/smart-city-project.git
cd smart-city-project

# 2. Instalar dependencias JavaScript
cd js-scripts
npm install
cd ..

# 3. Iniciar sistema completo (incluye Docker + Kafka Topics + Kibana Data Views)
.\init-system.ps1

# 4. Sistema listo! Todos los Data Views creados automáticamente
```

**Opción B: Inicio Manual** 🔧
```powershell
# 1. Clonar repositorio
git clone https://github.com/4901oscar/smart-city-project.git
cd smart-city-project

# 2. Instalar dependencias JavaScript
cd js-scripts
npm install
cd ..

# 3. Levantar infraestructura completa
docker compose up -d

# 4. Esperar ~30 segundos para que Kafka y Airflow inicialicen

# 5. Crear Kafka Topics manualmente
docker exec kafka kafka-topics --create --bootstrap-server localhost:9092 --topic events.standardized --partitions 3 --replication-factor 1
docker exec kafka kafka-topics --create --bootstrap-server localhost:9092 --topic correlated.alerts --partitions 2 --replication-factor 1
docker exec kafka kafka-topics --create --bootstrap-server localhost:9092 --topic events.dlq --partitions 1 --replication-factor 1

# 6. Crear Kibana Data Views automáticamente
.\scripts\init-kibana-dataviews.ps1

# 7. Verificar estado
docker ps
```

### Verificación Rápida

```powershell
# Health check del backend
curl http://localhost:5000/health

# Acceder a interfaces web
start http://localhost:5000          # Swagger UI (Backend API)
start http://localhost:8090          # Airflow UI (admin/admin)
start http://localhost:5601          # Kibana
start http://localhost:3000          # Grafana (admin/admin)
```

### Generar Eventos de Prueba

```powershell
# Terminal 1: Producer (genera eventos simulados)
cd js-scripts
npm run producer

# Terminal 2: Consumer (detecta y publica alertas)
npm run consumer

# Terminal 3: Monitor de alertas en tiempo real
npm run alert-monitor

# Ver despachos de Airflow en logs
docker logs airflow-scheduler --tail 50 -f
```

**¡Listo!** El sistema está procesando eventos, generando alertas y despachando a entidades de emergencia.

📖 **Para instrucciones detalladas**: Ver [QUICKSTART.md](QUICKSTART.md)

---

## 🏗️ Arquitectura del Sistema

```
┌─────────────────────────────────────────────────────────────────┐
│                    PRODUCERS (Sensores IoT)                      │
│  • Botones de pánico  • Cámaras LPR  • Sensores de velocidad   │
│  • Sensores acústicos  • Reportes ciudadanos                    │
└───────────────────────┬─────────────────────────────────────────┘
                        │ HTTP POST
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│              BACKEND API (.NET 9)                                │
│  • Validación de schemas (NJsonSchema)                          │
│  • Enriquecimiento geoespacial                                  │
│  • Persistencia en PostgreSQL (Azure)                           │
└───────────────────────┬─────────────────────────────────────────┘
                        │ Publish
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│                    KAFKA (3 Topics)                              │
│  • events.standardized (eventos válidos, 3 partitions, 7d)      │
│  • correlated.alerts (alertas generadas, 2 partitions, 30d)     │
│  • events.dlq (eventos fallidos, 1 partition, 14d)              │
└───────────────────────┬─────────────────────────────────────────┘
                        │ Subscribe
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│              CONSUMER (Node.js)                                  │
│  • Correlación de eventos multi-sensor                          │
│  • Generación de alertas (15+ tipos)                            │
│  • Clasificación por severidad (INFO/ALTO/CRÍTICO)              │
│  • Indexación en Elasticsearch                                  │
└───────────────────────┬─────────────────────────────────────────┘
                        │ Publish alerts
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│            APACHE AIRFLOW (Orquestación)                         │
│  • DAG: alert_dispatch_pipeline (ejecuta cada 1 min)           │
│  • Fetch alerts: GET /alerts del backend                        │
│  • Classify: Mapeo de alertas → entidades (18 tipos)           │
│  • Dispatch: POST /dispatch/{entity}                            │
└───────────────────────┬─────────────────────────────────────────┘
                        │ Dispatch
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│          ENTIDADES DE EMERGENCIA (Backend Endpoints)             │
│  • Policía Nacional      • Policía Municipal                    │
│  • Policía de Tránsito   • Bomberos                            │
│  • Bomberos Voluntarios  • Cruz Roja                           │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📁 Estructura del Proyecto

```
smart-city-project/
├── backend/                    # API .NET 9
│   ├── Controllers/            # EventsController, DispatchController, HealthController
│   ├── Services/               # Validación, Kafka, DbContext
│   ├── Schemas/                # JSON Schemas (envelope + 5 payloads)
│   └── appsettings.json        # Configuración (Azure PostgreSQL)
├── js-scripts/                 # Scripts Node.js
│   ├── producer.js             # Generador de eventos simulados
│   ├── consumer.js             # Detector y correlador de alertas
│   ├── alert-monitor.js        # Monitor de alertas en tiempo real
│   └── dlq-monitor.js          # Monitor de eventos fallidos
├── airflow/                    # Apache Airflow
│   ├── dags/                   # DAG de despacho de alertas
│   │   └── alert_dispatch_dag.py
│   └── Dockerfile              # Imagen personalizada con dependencias
├── database/                   # Scripts SQL
│   └── init-neon.sql           # Inicialización de tablas (events, alerts)
├── docker-compose.yml          # Orquestación de servicios
├── README.md                   # Este archivo
├── QUICKSTART.md               # Guía de inicio rápido detallada
├── TESTING.md                  # Estrategias de testing completas
├── COMPLIANCE.md               # Verificación vs especificación canónica
├── ALERTS_REFERENCE.md         # Referencia de tipos de alertas
└── ELASTICSEARCH_GUIDE.md      # Guía de Elasticsearch y Kibana
```

---

## 🔧 Servicios y Puertos

| Servicio               | Puerto | Descripción |
|------------------------|--------|-------------|
| **Backend API**        | 5000   | API REST .NET 9 con Swagger UI |
| **Kafka**              | 9092   | Broker de mensajería (interno: 29092) |
| **Zookeeper**          | 2181   | Coordinación de Kafka |
| **PostgreSQL (Azure)** | -      | Base de datos principal (arqui-pg.postgres.database.azure.com) |
| **Airflow Webserver**  | 8090   | UI de Apache Airflow (admin/admin) |
| **Airflow Scheduler**  | -      | Orquestador de DAGs |
| **Elasticsearch**      | 9200   | Motor de búsqueda y análisis |
| **Kibana**             | 5601   | Visualización de eventos y alertas |
| **Grafana**            | 3000   | Dashboards de métricas (admin/admin) |

---

## 📊 Tipos de Eventos y Alertas

### Eventos Soportados (5 tipos)

1. **panic.button** - Botones de pánico (emergencia/incendio/pánico)
2. **sensor.lpr** - Cámaras de lectura de placas vehiculares
3. **sensor.speed** - Sensores de velocidad radar
4. **sensor.acoustic** - Sensores acústicos (disparos/explosiones/vidrio roto)
5. **citizen.report** - Reportes ciudadanos (app/web/kiosco)

### Alertas Generadas (15+ tipos)

| Tipo de Alerta | Severidad | Entidades Despachadas |
|----------------|-----------|----------------------|
| DISPARO DETECTADO | CRÍTICO | Policía Nacional |
| INCENDIO REPORTADO | CRÍTICO | Bomberos |
| EXPLOSIÓN DETECTADA | CRÍTICO | Policía Nacional + Bomberos |
| EXCESO DE VELOCIDAD PELIGROSO | CRÍTICO | Policía de Tránsito + Policía Nacional |
| EMERGENCIA PERSONAL | ALTO | Policía Nacional + Cruz Roja |
| ACCIDENTE REPORTADO | ALTO | Bomberos Voluntarios + Cruz Roja |
| ALTERCADO REPORTADO | MEDIO | Policía Municipal |
| VELOCIDAD SOBRE LÍMITE | MEDIO | Policía de Tránsito |

📖 **Referencia completa**: Ver [ALERTS_REFERENCE.md](ALERTS_REFERENCE.md)

---

## 🧪 Testing y Validación

El proyecto incluye estrategias de testing exhaustivas:

```powershell
# Unit Tests del Backend
cd backend
dotnet test

# Validación de Schemas
curl -X POST http://localhost:5000/events -d @test-event.json

# Testing de flujo completo
cd js-scripts
node publish-test-alert.js  # Publica alerta de prueba
npm run alert-monitor       # Monitorea recepción

# Verificar despachos en Airflow
docker logs airflow-scheduler | Select-String "Despachado"
```

📖 **Guía completa de testing**: Ver [TESTING.md](TESTING.md)

---

## 📖 Documentación

📚 **[Ver Índice Completo de Documentación](DOCUMENTATION.md)** - Guía de navegación por roles y temas

### Guías Principales

| Documento | Descripción | Audiencia |
|-----------|-------------|-----------|
| [QUICKSTART.md](QUICKSTART.md) | Guía paso a paso (15 min para sistema funcionando) | Todos |
| [TESTING.md](TESTING.md) | Estrategias de testing y validación completas | Desarrolladores, QA |
| [COMPLIANCE.md](COMPLIANCE.md) | Verificación vs especificación canónica v1.0 | Arquitectos, Auditores |
| [ALERTS_REFERENCE.md](ALERTS_REFERENCE.md) | Matriz completa de tipos de alertas (15+) | Desarrolladores, Operadores |
| [ELASTICSEARCH_GUIDE.md](ELASTICSEARCH_GUIDE.md) | Guía de Elasticsearch, Kibana y queries | Data Analysts |
| [GRAFANA.md](GRAFANA.md) | Dashboards, métricas de Kafka y PostgreSQL | DevOps, SRE |
| [.github/copilot-instructions.md](.github/copilot-instructions.md) | Contexto técnico completo para AI agents | AI Assistants, Developers |

### Flujos de Lectura Recomendados

**Para Usuarios Nuevos** (50 min):
1. README.md → 2. QUICKSTART.md → 3. ALERTS_REFERENCE.md → 4. TESTING.md

**Para Desarrolladores** (90 min):
1. README.md → 2. QUICKSTART.md → 3. copilot-instructions.md → 4. COMPLIANCE.md → 5. TESTING.md

**Para Data Analysts** (85 min):
1. README.md → 2. QUICKSTART.md → 3. ELASTICSEARCH_GUIDE.md → 4. GRAFANA.md → 5. ALERTS_REFERENCE.md

---

## 🛠️ Comandos Útiles

### Gestión de Servicios

```powershell
# Iniciar todos los servicios
docker compose up -d

# Ver logs en tiempo real
docker compose logs -f backend
docker logs airflow-scheduler -f

# Reiniciar un servicio específico
docker restart backend

# Detener todos los servicios
docker compose down

# Limpiar todo (¡CUIDADO! Elimina volúmenes)
docker compose down -v
```

### Monitoreo

```powershell
# Ver eventos recientes
curl http://localhost:5000/events?take=10

# Ver alertas recientes
curl http://localhost:5000/alerts?take=10

# Ver despachos en backend
docker logs backend --tail 50 | Select-String "DESPACHO"

# Ver ejecuciones del DAG de Airflow
docker logs airflow-scheduler --tail 100 | Select-String "DagRun Finished"
```

### Troubleshooting

```powershell
# Verificar estado de contenedores
docker ps -a

# Verificar conectividad de Kafka
docker exec -it smart-city-project-kafka-1 kafka-topics --bootstrap-server localhost:9092 --list

# Reiniciar Kafka (si hay problemas)
docker restart smart-city-project-kafka-1 smart-city-project-zookeeper-1

# Ver logs de error del backend
docker logs backend 2>&1 | Select-String "fail|error" -Context 2
```

---

## 🔒 Configuración de Base de Datos

El sistema usa **Azure PostgreSQL** como base de datos principal:

```
Host: arqui-pg.postgres.database.azure.com
Database: SmartCitiesBD
Username: grupo2
Password: 4rqu1.4pp
SSL: Required
```

**Tablas principales**:
- `events` - Eventos brutos con payload JSONB
- `alerts` - Alertas correlacionadas con metadatos

📖 **Script de inicialización**: Ver `database/init-neon.sql`

---

## 🎯 Casos de Uso

### 1. Detección de Disparos

```
Sensor Acústico → Detecta 145 dB + pattern "disparo"
                ↓
Backend valida y publica a Kafka
                ↓
Consumer genera: "DISPARO DETECTADO" (CRÍTICO)
                ↓
Airflow despacha a: Policía Nacional
```

### 2. Exceso de Velocidad

```
Cámara LPR → Lee placa P-123AB a 120 km/h
           ↓
Backend enriquece con geolocalización
           ↓
Consumer genera: "EXCESO DE VELOCIDAD PELIGROSO" (CRÍTICO)
           ↓
Airflow despacha a: Policía de Tránsito + Policía Nacional
```

### 3. Incendio Reportado

```
Ciudadano → Reporta incendio vía app
          ↓
Backend valida payload y publica
          ↓
Consumer genera: "INCENDIO REPORTADO POR CIUDADANO" (CRÍTICO)
          ↓
Airflow despacha a: Bomberos
```

---

## 👥 Contribuidores

- **Grupo 2** - Arquitectura de Software, Universidad
- **Backend**: .NET 9, Entity Framework Core, Confluent.Kafka
- **Consumer**: Node.js, KafkaJS, Elasticsearch client
- **Orquestación**: Apache Airflow 2.7.3

---

## 📝 Licencia

Este proyecto es académico y está desarrollado con fines educativos.

---

## 🆘 Soporte

**Problemas comunes**:
1. "Port 5000 already in use" → Cambiar puerto en `docker-compose.yml`
2. "Kafka connection refused" → Esperar 30s después de `docker compose up`
3. "Database connection failed" → Verificar credenciales en `appsettings.json`

**Logs útiles**:
```powershell
docker compose logs backend
docker compose logs airflow-scheduler
docker logs smart-city-project-kafka-1
```

📖 **Documentación completa**: Ver [QUICKSTART.md](QUICKSTART.md)

---

**Estado del Proyecto**: ✅ Completamente funcional y probado

**Última actualización**: Octubre 2025