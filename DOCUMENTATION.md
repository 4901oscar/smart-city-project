# 📚 Smart City - Índice de Documentación

## 🎯 Guías Principales

### 🚀 [README.md](README.md)
**Para**: Todos los usuarios  
**Contenido**: Visión general del proyecto, arquitectura, quick start, características principales  
**Tiempo de lectura**: 10 minutos  
**Cuándo usarlo**: Primera vez que accedes al proyecto

### 📖 [QUICKSTART.md](QUICKSTART.md)
**Para**: Desarrolladores nuevos, estudiantes, evaluadores  
**Contenido**: Guía paso a paso para levantar el sistema completo en 15 minutos  
**Tiempo de lectura**: 5 minutos, 15 minutos para ejecutar  
**Cuándo usarlo**: Cuando quieres ver el sistema funcionando rápidamente

---

## 🧪 Testing y Validación

### ✅ [TESTING.md](TESTING.md)
**Para**: QA, desarrolladores, evaluadores técnicos  
**Contenido**: Estrategias completas de testing (unit, integration, E2E, performance)  
**Tiempo de lectura**: 15 minutos  
**Cuándo usarlo**: Para validar que el sistema funciona correctamente o antes de deployment

### 📋 [COMPLIANCE.md](COMPLIANCE.md)
**Para**: Arquitectos, líderes técnicos, auditores  
**Contenido**: Verificación de cumplimiento con la especificación canónica v1.0  
**Tiempo de lectura**: 20 minutos  
**Cuándo usarlo**: Para auditorías o validar que se cumple con la especificación

---

## 📊 Visualización y Monitoreo

### 🔍 [ELASTICSEARCH_GUIDE.md](ELASTICSEARCH_GUIDE.md)
**Para**: Data analysts, desarrolladores  
**Contenido**: Configuración de Elasticsearch y Kibana, queries, dashboards, Data Views  
**Tiempo de lectura**: 25 minutos  
**Cuándo usarlo**: Para analizar eventos y alertas con búsquedas avanzadas

### 📈 [GRAFANA.md](GRAFANA.md)
**Para**: DevOps, SRE, administradores de sistema  
**Contenido**: Dashboards de Grafana, métricas de Kafka, PostgreSQL queries, alertas  
**Tiempo de lectura**: 20 minutos  
**Cuándo usarlo**: Para monitorear el sistema en tiempo real o configurar alertas

---

## 🚨 Referencia de Alertas

### 🎯 [ALERTS_REFERENCE.md](ALERTS_REFERENCE.md)
**Para**: Desarrolladores, diseñadores de reglas, operadores  
**Contenido**: Matriz completa de 15+ tipos de alertas, reglas de detección, routing  
**Tiempo de lectura**: 15 minutos  
**Cuándo usarlo**: Para entender cómo se generan y clasifican las alertas

---

## 🤖 Contexto para AI Agents

### 🧠 [.github/copilot-instructions.md](.github/copilot-instructions.md)
**Para**: GitHub Copilot, AI assistants, onboarding de desarrolladores  
**Contenido**: Arquitectura completa, convenciones, patrones, integración points  
**Tiempo de lectura**: 30 minutos  
**Cuándo usarlo**: Para configurar AI assistants o como referencia técnica profunda

---

## 📁 Archivos Técnicos

### Schemas JSON
- `backend/Schemas/event-envelope-schema.json` - Schema canónico del envelope
- `backend/Schemas/panic-button-schema.json` - Payload de botón de pánico
- `backend/Schemas/lpr-camera-schema.json` - Payload de cámara LPR
- `backend/Schemas/speed-motion-schema.json` - Payload de sensor de velocidad
- `backend/Schemas/acoustic-ambient-schema.json` - Payload de sensor acústico
- `backend/Schemas/citizen-report-schema.json` - Payload de reporte ciudadano

### Scripts de Base de Datos
- `database/init-neon.sql` - Script de inicialización de PostgreSQL

### Scripts JavaScript
- `js-scripts/producer.js` - Generador de eventos simulados
- `js-scripts/consumer.js` - Detector y correlador de alertas
- `js-scripts/alert-monitor.js` - Monitor de alertas en tiempo real
- `js-scripts/dlq-monitor.js` - Monitor de eventos fallidos
- `js-scripts/publish-test-alert.js` - Script de prueba de alertas

### Airflow
- `airflow/dags/alert_dispatch_dag.py` - DAG de despacho de alertas
- `airflow/Dockerfile` - Imagen personalizada con dependencias

---

## 🗺️ Flujo de Lectura Recomendado

### Para Usuarios Nuevos (Estudiantes/Evaluadores)
1. **README.md** (10 min) - Entender qué es el proyecto
2. **QUICKSTART.md** (15 min) - Levantar el sistema
3. **ALERTS_REFERENCE.md** (15 min) - Ver qué alertas se generan
4. **TESTING.md** (10 min) - Probar que funciona

**Total: ~50 minutos** para tener el sistema funcionando y entendido ✅

---

### Para Desarrolladores
1. **README.md** (10 min) - Arquitectura general
2. **QUICKSTART.md** (15 min) - Setup del entorno
3. **.github/copilot-instructions.md** (30 min) - Detalles técnicos profundos
4. **COMPLIANCE.md** (20 min) - Especificaciones y constraints
5. **TESTING.md** (15 min) - Estrategias de testing

**Total: ~90 minutos** para dominar el proyecto ✅

---

### Para Analistas de Datos
1. **README.md** (10 min) - Contexto del proyecto
2. **QUICKSTART.md** (15 min) - Levantar el sistema
3. **ELASTICSEARCH_GUIDE.md** (25 min) - Queries y análisis
4. **GRAFANA.md** (20 min) - Dashboards y visualizaciones
5. **ALERTS_REFERENCE.md** (15 min) - Tipos de datos generados

**Total: ~85 minutos** para analizar datos efectivamente ✅

---

### Para DevOps/SRE
1. **README.md** (10 min) - Arquitectura de servicios
2. **QUICKSTART.md** (15 min) - Deployment
3. **GRAFANA.md** (20 min) - Monitoreo y alertas
4. **TESTING.md** (15 min) - Smoke tests y health checks
5. **docker-compose.yml** (análisis directo)

**Total: ~60 minutos** para operacionalizar el sistema ✅

---

## 🔍 Búsqueda Rápida por Tema

### Configuración
- **Docker Compose**: Ver `docker-compose.yml` y `QUICKSTART.md`
- **Base de Datos**: Ver `README.md` sección "Configuración de Base de Datos"
- **Kafka**: Ver `.github/copilot-instructions.md` sección "Kafka Configuration"
- **Airflow**: Ver `QUICKSTART.md` Paso 6 y `airflow/dags/alert_dispatch_dag.py`

### Desarrollo
- **Schemas**: Ver `backend/Schemas/` y `COMPLIANCE.md`
- **API Endpoints**: Ver `README.md` y Swagger UI (http://localhost:5000)
- **Eventos**: Ver `js-scripts/producer.js` y `COMPLIANCE.md`
- **Alertas**: Ver `js-scripts/consumer.js` y `ALERTS_REFERENCE.md`

### Troubleshooting
- **Errores comunes**: Ver `QUICKSTART.md` sección "Troubleshooting"
- **Logs**: Ver `QUICKSTART.md` sección "Logs y Debugging"
- **Health checks**: Ver `TESTING.md` sección "Smoke Tests"

### Monitoreo
- **Métricas de Kafka**: Ver `GRAFANA.md` Dashboard 3
- **Eventos y Alertas**: Ver `ELASTICSEARCH_GUIDE.md` y `GRAFANA.md`
- **Despachos de Airflow**: Ver `QUICKSTART.md` Paso 6.3

---

## 📊 Diagrama de Dependencias de Documentación

```
README.md (entrada principal)
    │
    ├─→ QUICKSTART.md (guía práctica)
    │       │
    │       ├─→ TESTING.md (validación)
    │       ├─→ ELASTICSEARCH_GUIDE.md (análisis)
    │       └─→ GRAFANA.md (monitoreo)
    │
    ├─→ COMPLIANCE.md (especificaciones)
    │
    ├─→ ALERTS_REFERENCE.md (referencia de alertas)
    │
    └─→ .github/copilot-instructions.md (detalles técnicos)
```

---

## 🎯 Documentos por Rol

| Rol | Documentos Requeridos | Tiempo Total |
|-----|----------------------|--------------|
| **Estudiante** | README + QUICKSTART + TESTING | ~35 min |
| **Profesor/Evaluador** | README + QUICKSTART + COMPLIANCE + TESTING | ~55 min |
| **Desarrollador Backend** | README + QUICKSTART + copilot-instructions + COMPLIANCE | ~90 min |
| **Desarrollador Frontend** | README + QUICKSTART + ALERTS_REFERENCE | ~35 min |
| **Data Analyst** | README + ELASTICSEARCH_GUIDE + GRAFANA | ~55 min |
| **DevOps/SRE** | README + QUICKSTART + GRAFANA + docker-compose.yml | ~50 min |
| **Arquitecto** | Todos los documentos | ~150 min |

---

## ✅ Checklist de Onboarding

### Día 1 (2-3 horas)
- [ ] Leer README.md completo
- [ ] Seguir QUICKSTART.md paso a paso
- [ ] Verificar que todos los servicios estén corriendo
- [ ] Generar eventos y ver alertas
- [ ] Explorar Swagger UI

### Día 2 (2-3 horas)
- [ ] Leer COMPLIANCE.md
- [ ] Revisar ALERTS_REFERENCE.md
- [ ] Ejecutar tests en TESTING.md
- [ ] Explorar Kibana (ELASTICSEARCH_GUIDE.md)
- [ ] Configurar un dashboard en Grafana

### Día 3 (2-3 horas)
- [ ] Leer .github/copilot-instructions.md
- [ ] Revisar código del backend
- [ ] Revisar código del consumer
- [ ] Entender el DAG de Airflow
- [ ] Hacer modificaciones de prueba

**Total: ~7-9 horas** para dominio completo del proyecto ✅

---

## 🆘 ¿Dónde Buscar Ayuda?

| Problema | Documento | Sección |
|----------|-----------|---------|
| No puedo levantar el sistema | QUICKSTART.md | Troubleshooting |
| No entiendo cómo funciona | README.md | Arquitectura del Sistema |
| Quiero modificar eventos | COMPLIANCE.md | Payloads por Sensor |
| Quiero crear nuevas alertas | ALERTS_REFERENCE.md | Matriz de Detección |
| Kibana no muestra datos | ELASTICSEARCH_GUIDE.md | Troubleshooting |
| Grafana no conecta | GRAFANA.md | Verificar Conexión |
| Tests fallan | TESTING.md | Expected Results |
| Airflow no despacha | QUICKSTART.md | Paso 6 |

---

## 📝 Convenciones de Documentación

### Emojis Usados
- 🚀 Inicio rápido / Quick start
- 📖 Documentación / Guías
- 🧪 Testing / Pruebas
- 📊 Visualización / Dashboards
- 🔧 Configuración / Setup
- 🎯 Objetivos / Metas
- ✅ Checklist / Verificación
- ⚠️ Advertencias / Warnings
- 💡 Tips / Consejos
- 🔍 Búsqueda / Análisis

### Estructura de Secciones
Todos los documentos siguen una estructura similar:
1. **Introducción** - Qué es y para qué sirve
2. **Configuración** - Cómo setup inicial
3. **Uso** - Cómo usar paso a paso
4. **Ejemplos** - Casos de uso concretos
5. **Troubleshooting** - Solución de problemas
6. **Referencias** - Links a otros documentos

---

## 🔄 Última Actualización

**Fecha**: Octubre 2025  
**Versión del Sistema**: 1.0  
**Estado**: ✅ Documentación completa y actualizada

---

**¿Nuevo en el proyecto?** Empieza por [README.md](README.md) → [QUICKSTART.md](QUICKSTART.md) 🚀
