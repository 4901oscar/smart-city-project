# Smart City Alert System Reference

## Alert Levels
- **CRÍTICO** (Critical): Requires immediate emergency response
- **ALTO** (High): Significant event requiring urgent attention
- **MEDIO** (Medium): Notable event requiring investigation
- **INFO** (Informational): Logging for correlation and analysis

---

## Alert Types by Event Category

### 1. Panic Button Alerts (`panic.button`)

#### EMERGENCIA PERSONAL (CRÍTICO)
- **Trigger**: `tipo_de_alerta === 'panico'`
- **Details**: Device ID, user context (móvil/quiosco/web)
- **Action**: Dispatch emergency services immediately

#### INCENDIO REPORTADO (CRÍTICO)
- **Trigger**: `tipo_de_alerta === 'incendio'`
- **Details**: Device ID, user context
- **Action**: Alert fire department

#### EMERGENCIA GENERAL (ALTO)
- **Trigger**: `tipo_de_alerta === 'emergencia'`
- **Details**: Device ID, user context
- **Action**: Assess situation, prepare response

---

### 2. LPR Camera Alerts (`sensor.lpr`)

#### EXCESO DE VELOCIDAD PELIGROSO (CRÍTICO)
- **Trigger**: `velocidad_estimada > 100 km/h`
- **Details**: License plate, vehicle model/color, sensor location
- **Action**: Immediate traffic enforcement

#### EXCESO DE VELOCIDAD (MEDIO)
- **Trigger**: `velocidad_estimada > 70 km/h`
- **Details**: License plate, vehicle description
- **Action**: Monitor and correlate with other sensors

#### REGISTRO VEHICULAR (INFO)
- **Trigger**: `velocidad_estimada > 60 km/h`
- **Details**: Plate, speed, sensor location
- **Action**: Log for future correlation (potential pursuit patterns)

---

### 3. Speed Sensor Alerts (`sensor.speed`)

#### VELOCIDAD EXCESIVA DETECTADA (ALTO)
- **Trigger**: `velocidad_detectada > 80 km/h`
- **Details**: Speed, direction, sensor ID
- **Action**: Risk of accident - alert traffic units

#### VELOCIDAD SOBRE LÍMITE (MEDIO)
- **Trigger**: `velocidad_detectada > 60 km/h`
- **Details**: Speed, direction, sensor ID
- **Action**: Monitor traffic flow

---

### 4. Acoustic Sensor Alerts (`sensor.acoustic`)

#### DISPARO DETECTADO (CRÍTICO)
- **Trigger**: `tipo_sonido_detectado === 'disparo'`
- **Details**: Decibel level, confidence probability
- **Action**: Dispatch police immediately

#### EXPLOSIÓN DETECTADA (CRÍTICO)
- **Trigger**: `tipo_sonido_detectado === 'explosion'`
- **Details**: Decibel level, confidence probability
- **Action**: Alert police and fire department

#### VIDRIO ROTO DETECTADO (ALTO)
- **Trigger**: `tipo_sonido_detectado === 'vidrio_roto'`
- **Details**: Decibel level, confidence probability
- **Action**: Verify with cameras, possible burglary/vandalism

#### CONTAMINACIÓN ACÚSTICA EXTREMA (ALTO)
- **Trigger**: `nivel_decibeles > 120 dB`
- **Details**: Decibel level
- **Action**: May cause hearing damage - investigate source

---

### 5. Citizen Report Alerts (`citizen.report`)

#### INCENDIO REPORTADO POR CIUDADANO (CRÍTICO)
- **Trigger**: `tipo_evento === 'incendio'`
- **Details**: Location, description, report origin (app/physical/web)
- **Action**: Alert fire department

#### ACCIDENTE REPORTADO (ALTO)
- **Trigger**: `tipo_evento === 'accidente'`
- **Details**: Location, description, origin
- **Action**: Dispatch emergency medical services

#### ALTERCADO REPORTADO (MEDIO)
- **Trigger**: `tipo_evento === 'altercado'`
- **Details**: Location, description, origin
- **Action**: Monitor and assess need for police intervention

---

## Correlation & Severity Mapping

### Automatic Severity Assignment (Producer)
- **Panic/Fire alerts**: `critical`
- **Speed > 100 km/h**: `critical`
- **Speed 70-100 km/h**: `warning`
- **Gunshots/Explosions**: `critical`
- **Acoustic confidence > 0.7**: `warning`
- **Citizen fire reports**: `critical`
- **Citizen accidents**: `warning`

### Cross-Event Correlation Opportunities
1. **LPR + Speed sensors**: Track vehicle patterns across zones
2. **Acoustic + Panic**: Confirm gunfire with emergency button activations
3. **Citizen reports + Sensors**: Validate automated detections
4. **Multiple alerts in same zone/timeframe**: Escalate to major incident

---

## Alert Console Output Format

```
================================================================================
🚨 ALERTAS DETECTADAS 🚨
Zona: Zona 10 | Coords: 14.6091, -90.5252
Timestamp: 2025-10-05T... | Event ID: 12345678...
--------------------------------------------------------------------------------
[CRÍTICO] DISPARO DETECTADO
  → Posible disparo de arma de fuego (87.5% confianza)
  → 145 dB - Requiere unidad policial inmediata
================================================================================
```

---

## Testing Alert System

### Start the alert system:
```bash
# Terminal 1: Start infrastructure
docker-compose up -d

# Terminal 2: Start consumer (alert detector)
cd js-scripts
npm run consumer

# Terminal 3: Start producer (event generator)
cd js-scripts
npm run producer
```

### Expected Behavior:
- Producer generates events every 3 seconds with realistic payloads
- Consumer analyzes each event and generates 0-3 alerts per event
- Critical events show in RED, warnings in YELLOW, info in CYAN
- Each alert includes actionable details and recommended response

---

## Future Enhancements
- [ ] Add time-window aggregation (e.g., "3 disparos en 5 minutos")
- [ ] Implement cross-zone correlation
- [ ] Store alerts in `alerts` table with evidence JSON
- [ ] Add webhook notifications for CRÍTICO alerts
- [ ] Machine learning for confidence score tuning
- [ ] Historical pattern detection (repeat offenders, hotspots)
