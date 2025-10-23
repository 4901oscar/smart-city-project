# 🚀 Quick Start - Dashboard Grafana Smart City

## Inicio Rápido (3 pasos)

### 1. Acceder a Grafana
```bash
# Abrir navegador en:
http://localhost:3000

# Credenciales:
Usuario: admin
Contraseña: admin
```

### 2. Importar Dashboard
```bash
# En Grafana UI:
1. Ir a "+ Dashboards"
2. Seleccionar "Import"
3. Cargar archivo: monitoring/grafana/dashboards/smart-city-metrics.json
4. Click "Load"
```

### 3. Verificar Data Source
```bash
# En Grafana UI:
1. Ir a "Configuration" > "Data Sources"
2. Verificar "Smart City PostgreSQL" está conectado
3. Test connection = Success
```

## 📊 Vistas Principales

### Panel de Control Principal
- **Eventos/min**: Flujo de datos en tiempo real
- **Alertas/min**: Generación de alertas
- **Totales**: Contadores generales

### Análisis por Categorías
- **Tipos de Evento**: panic.button, sensor.lpr, etc.
- **Tipos de Sensor**: Distribución por categoría
- **Alertas por Zona**: Mapa geográfico

### Monitoreo Operacional
- **Score Promedio**: Gauge de criticidad
- **Top 10 Alertas**: Más frecuentes
- **Log de Alertas**: Últimas 100 detalladas

## ⚡ Comandos de Verificación

### Verificar Sistema Activo
```powershell
# Health check backend
Invoke-RestMethod http://localhost:5000/health

# Estadísticas actuales
Invoke-RestMethod http://localhost:5000/alerts/stats

# Contenedores activos
docker ps --format "table {{.Names}}\t{{.Status}}"
```

### Generar Datos de Prueba
```powershell
# Terminal 1: Producer
cd js-scripts; npm run producer

# Terminal 2: Consumer  
cd js-scripts; npm run consumer

# Terminal 3: Monitor
cd js-scripts; npm run alert-monitor
```

## 🎯 URLs de Acceso Directo

- **Grafana**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000 
- **Kafka UI**: http://localhost:8080
- **Prometheus**: http://localhost:9090

## 📱 Vista Móvil

El dashboard se adapta automáticamente a dispositivos móviles con:
- Paneles reorganizados verticalmente
- Métricas clave priorizadas
- Interacción táctil optimizada

---

✅ **Dashboard Listo**: http://localhost:3000  
🔧 **Admin**: admin/admin  
📊 **Data**: PostgreSQL + Prometheus  
🚀 **Status**: Operacional