# 🚨 Sistema de Alertas - Reglas y Tipos

## 📊 Resumen Ejecutivo

**Total de Tipos de Alertas**: **16 tipos de alertas**
- 🔴 **CRÍTICO**: 7 alertas
- 🟡 **ALTO**: 5 alertas  
- 🟠 **MEDIO**: 3 alertas
- 🔵 **INFO**: 1 alerta

---

## 🔴 ALERTAS CRÍTICAS (7)

### 1. EMERGENCIA PERSONAL
- **Evento**: `panic.button`
- **Regla**: `payload.tipo_de_alerta === 'panico'`
- **Acción**: Despachar servicios de emergencia inmediatamente
- **Datos**: Dispositivo ID, contexto de usuario (móvil/quiosco/web)

### 2. INCENDIO REPORTADO (Botón Pánico)
- **Evento**: `panic.button`
- **Regla**: `payload.tipo_de_alerta === 'incendio'`
- **Acción**: Alertar a bomberos
- **Datos**: Dispositivo ID, contexto de usuario

### 3. EXCESO DE VELOCIDAD PELIGROSO
- **Evento**: `sensor.lpr`
- **Regla**: `payload.velocidad_estimada > 100 km/h`
- **Acción**: Aplicación inmediata de ley de tránsito
- **Datos**: Placa vehicular, modelo, color, ubicación del sensor

### 4. DISPARO DETECTADO
- **Evento**: `sensor.acoustic`
- **Regla**: `payload.tipo_sonido_detectado === 'disparo'`
- **Acción**: Despachar policía inmediatamente
- **Datos**: Nivel de decibeles, probabilidad de evento crítico

### 5. EXPLOSIÓN DETECTADA
- **Evento**: `sensor.acoustic`
- **Regla**: `payload.tipo_sonido_detectado === 'explosion'`
- **Acción**: Alertar bomberos y policía
- **Datos**: Nivel de decibeles, probabilidad de evento crítico

### 6. INCENDIO REPORTADO POR CIUDADANO
- **Evento**: `citizen.report`
- **Regla**: `payload.tipo_evento === 'incendio'`
- **Acción**: Alertar bomberos
- **Datos**: Ubicación aproximada, mensaje descriptivo, origen del reporte

### 7. EVENTO CRÍTICO (Correlación)
- **Evento**: Cualquiera
- **Regla**: `severity === 'critical' && alertas.length === 0`
- **Acción**: Requiere atención inmediata
- **Datos**: Tipo de evento

---

## 🟡 ALERTAS DE NIVEL ALTO (5)

### 8. EMERGENCIA GENERAL
- **Evento**: `panic.button`
- **Regla**: `payload.tipo_de_alerta === 'emergencia'`
- **Acción**: Evaluar situación, preparar respuesta
- **Datos**: Dispositivo ID, contexto de usuario

### 9. VELOCIDAD EXCESIVA DETECTADA
- **Evento**: `sensor.speed`
- **Regla**: `payload.velocidad_detectada > 80 km/h`
- **Acción**: Riesgo de accidente - alertar unidades de tráfico
- **Datos**: Velocidad, dirección, sensor ID

### 10. VIDRIO ROTO DETECTADO
- **Evento**: `sensor.acoustic`
- **Regla**: `payload.tipo_sonido_detectado === 'vidrio_roto'`
- **Acción**: Verificar con cámaras - posible robo/vandalismo
- **Datos**: Nivel de decibeles, probabilidad de evento crítico

### 11. CONTAMINACIÓN ACÚSTICA EXTREMA
- **Evento**: `sensor.acoustic`
- **Regla**: `payload.nivel_decibeles > 120 dB`
- **Acción**: Puede causar daños auditivos - investigar fuente
- **Datos**: Nivel de decibeles

### 12. ACCIDENTE REPORTADO
- **Evento**: `citizen.report`
- **Regla**: `payload.tipo_evento === 'accidente'`
- **Acción**: Despachar servicios médicos de emergencia
- **Datos**: Ubicación aproximada, mensaje descriptivo, origen

---

## 🟠 ALERTAS DE NIVEL MEDIO (3)

### 13. EXCESO DE VELOCIDAD
- **Evento**: `sensor.lpr`
- **Regla**: `payload.velocidad_estimada > 70 km/h` (pero ≤ 100)
- **Acción**: Monitorear y correlacionar con otros sensores
- **Datos**: Placa vehicular, descripción del vehículo

### 14. VELOCIDAD SOBRE LÍMITE
- **Evento**: `sensor.speed`
- **Regla**: `payload.velocidad_detectada > 60 km/h` (pero ≤ 80)
- **Acción**: Monitorear flujo de tráfico
- **Datos**: Dirección, sensor ID

### 15. ALTERCADO REPORTADO
- **Evento**: `citizen.report`
- **Regla**: `payload.tipo_evento === 'altercado'`
- **Acción**: Monitorear y evaluar necesidad de intervención policial
- **Datos**: Ubicación aproximada, mensaje descriptivo, origen

---

## 🔵 ALERTAS INFORMATIVAS (1)

### 16. REGISTRO VEHICULAR
- **Evento**: `sensor.lpr`
- **Regla**: `payload.velocidad_estimada > 60 km/h`
- **Acción**: Registrar para correlación futura (patrones de persecución)
- **Datos**: Placa, velocidad, ubicación del sensor

---

## 📋 Tabla Resumen de Reglas

| # | Tipo de Alerta | Evento | Condición | Nivel | Acción |
|---|----------------|--------|-----------|-------|--------|
| 1 | Emergencia Personal | panic.button | tipo='panico' | CRÍTICO | Emergencia inmediata |
| 2 | Incendio (Pánico) | panic.button | tipo='incendio' | CRÍTICO | Bomberos |
| 3 | Emergencia General | panic.button | tipo='emergencia' | ALTO | Evaluar situación |
| 4 | Vel. Peligrosa | sensor.lpr | velocidad>100 | CRÍTICO | Tránsito urgente |
| 5 | Exceso Velocidad | sensor.lpr | velocidad>70 | MEDIO | Monitorear |
| 6 | Registro Vehículo | sensor.lpr | velocidad>60 | INFO | Registrar |
| 7 | Vel. Excesiva | sensor.speed | velocidad>80 | ALTO | Alerta tráfico |
| 8 | Vel. Sobre Límite | sensor.speed | velocidad>60 | MEDIO | Monitorear |
| 9 | Disparo | sensor.acoustic | tipo='disparo' | CRÍTICO | Policía inmediata |
| 10 | Explosión | sensor.acoustic | tipo='explosion' | CRÍTICO | Policía + Bomberos |
| 11 | Vidrio Roto | sensor.acoustic | tipo='vidrio_roto' | ALTO | Verificar cámaras |
| 12 | Ruido Extremo | sensor.acoustic | decibeles>120 | ALTO | Investigar fuente |
| 13 | Incendio Ciudadano | citizen.report | tipo='incendio' | CRÍTICO | Bomberos |
| 14 | Accidente | citizen.report | tipo='accidente' | ALTO | Servicios médicos |
| 15 | Altercado | citizen.report | tipo='altercado' | MEDIO | Monitorear |
| 16 | Evento Crítico | * | severity='critical' | ALTO | Atención inmediata |

---

## 🎯 Lógica de Detección por Evento

### panic.button
```javascript
if (tipo_de_alerta === 'panico')     → CRÍTICO: Emergencia Personal
if (tipo_de_alerta === 'incendio')   → CRÍTICO: Incendio Reportado
if (tipo_de_alerta === 'emergencia') → ALTO: Emergencia General
```

### sensor.lpr
```javascript
if (velocidad > 100)        → CRÍTICO: Velocidad Peligrosa
else if (velocidad > 70)    → MEDIO: Exceso de Velocidad
if (velocidad > 60)         → INFO: Registro Vehicular
```

### sensor.speed
```javascript
if (velocidad > 80)         → ALTO: Velocidad Excesiva
else if (velocidad > 60)    → MEDIO: Velocidad Sobre Límite
```

### sensor.acoustic
```javascript
if (tipo === 'disparo')     → CRÍTICO: Disparo Detectado
if (tipo === 'explosion')   → CRÍTICO: Explosión Detectada
if (tipo === 'vidrio_roto') → ALTO: Vidrio Roto
if (decibeles > 120)        → ALTO: Contaminación Acústica
```

### citizen.report
```javascript
if (tipo === 'incendio')    → CRÍTICO: Incendio Reportado
if (tipo === 'accidente')   → ALTO: Accidente Reportado
if (tipo === 'altercado')   → MEDIO: Altercado Reportado
```

---

## 🔄 Correlación y Severidad Automática

### Asignación de Severity en Producer
El producer asigna `severity` automáticamente basado en:

```javascript
// panic.button
if (tipo === 'panico' || tipo === 'incendio') → severity = 'critical'
else → severity = 'warning'

// sensor.lpr
if (velocidad > 100) → severity = 'critical'
else if (velocidad > 70) → severity = 'warning'
else → severity = 'info'

// sensor.speed
if (velocidad > 80) → severity = 'critical'
else if (velocidad > 60) → severity = 'warning'
else → severity = 'info'

// sensor.acoustic
if (tipo === 'disparo' || tipo === 'explosion') → severity = 'critical'
else if (probabilidad > 0.7) → severity = 'warning'
else → severity = 'info'

// citizen.report
if (tipo === 'incendio') → severity = 'critical'
else if (tipo === 'accidente') → severity = 'warning'
else → severity = 'info'
```

---

## 🎨 Codificación de Colores en Console

- 🔴 **CRÍTICO**: Rojo intenso (`colors.red`)
- 🟡 **ALTO**: Amarillo (`colors.yellow`)
- 🟠 **MEDIO**: Amarillo (`colors.yellow`)
- 🔵 **INFO**: Cyan (`colors.cyan`)
- 🟣 **Contaminación Acústica**: Magenta (`colors.magenta`)

---

## 📈 Ejemplo de Output de Alerta

```
================================================================================
🚨 ALERTAS DETECTADAS 🚨
Zona: Zona 10 | Coords: 14.6091, -90.5252
Timestamp: 2025-10-06T03:45:12.234Z | Event ID: abc123...
--------------------------------------------------------------------------------
[CRÍTICO] DISPARO DETECTADO
  → Posible disparo de arma de fuego (87.5% confianza)
  → 145 dB - Requiere unidad policial inmediata
--------------------------------------------------------------------------------
[ALTO] CONTAMINACIÓN ACÚSTICA EXTREMA
  → Nivel de ruido peligroso: 145 dB
  → Puede causar daños auditivos
================================================================================
```

---

## 🔮 Oportunidades de Correlación

### Multi-Sensor
1. **LPR + Speed**: Seguimiento de vehículos a través de zonas
2. **Acoustic + Panic**: Confirmar disparos con activaciones de pánico
3. **Citizen + Sensors**: Validar detecciones automáticas

### Temporal
1. **Ventana de 5 min**: "3 disparos en 5 minutos" → Escalate
2. **Misma zona**: Múltiples eventos → Incidente mayor
3. **Secuencial**: Pánico → Disparo → Accidente → Operación coordinada

---

## 🛠️ Testing de Alertas

### Generar Alerta CRÍTICA
```powershell
# Editar producer.js temporalmente para forzar disparo
cd js-scripts
npm run producer
```

### Ver Todas las Alertas
```powershell
cd js-scripts
npm run consumer
# Observar las alertas en tiempo real con colores
```

### Filtrar por Nivel
```javascript
// En consumer.js, agregar filtro:
if (alertas.some(a => a.nivel === 'CRÍTICO')) {
  // Solo mostrar críticas
}
```

---

## 📊 Estadísticas de Alertas

**Distribución por Severidad:**
- CRÍTICO: 43.75% (7/16)
- ALTO: 31.25% (5/16)
- MEDIO: 18.75% (3/16)
- INFO: 6.25% (1/16)

**Distribución por Tipo de Evento:**
- panic.button: 3 alertas
- sensor.lpr: 3 alertas
- sensor.speed: 2 alertas
- sensor.acoustic: 4 alertas
- citizen.report: 3 alertas
- correlación: 1 alerta

---

**Archivo de Implementación**: `js-scripts/consumer.js`  
**Función Principal**: `detectarAlertas(event)`  
**Última Actualización**: Octubre 6, 2025
