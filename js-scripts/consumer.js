const { Client } = require('pg');
const { v4: uuidv4 } = require('uuid');
const axios = require('axios');
require('dotenv').config({ path: '../.env' });

// Configuración
const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:5000';
const ELASTICSEARCH_URL = process.env.ELASTICSEARCH_URL || 'http://localhost:9200';
const POLL_INTERVAL_MS = 5000; // Verificar cada 5 segundos

// Configuración de PostgreSQL (Neon Cloud)
const pgConfig = {
  host: process.env.PGHOST || 'ep-solitary-waterfall-adymysgs-pooler.c-2.us-east-1.aws.neon.tech',
  database: process.env.PGDATABASE || 'SmartCitiesBD',
  user: process.env.PGUSER || 'neondb_owner',
  password: process.env.PGPASSWORD || 'npg_c1UK0FbwvrER',
  port: 5432,
  ssl: {
    rejectUnauthorized: false
  }
};

// Cliente PostgreSQL
const client = new Client(pgConfig);

// Colores para consola
const colors = {
  reset: '\x1b[0m',
  bright: '\x1b[1m',
  red: '\x1b[31m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m',
  magenta: '\x1b[35m',
  cyan: '\x1b[36m',
  green: '\x1b[32m'
};

// Variable para tracking de último evento procesado
let lastProcessedTimestamp = null;

// Función para detectar alertas basadas en eventos
function detectarAlertas(event) {
  const alertas = [];
  const { event_type, payload, zone, geo_lat, geo_lon, severity, ts_utc } = event;

  // ALERTAS DE BOTÓN DE PÁNICO
  if (event_type === 'panic.button') {
    if (payload.tipo_de_alerta === 'panico') {
      alertas.push({
        nivel: 'CRÍTICO',
        tipo: 'EMERGENCIA PERSONAL',
        mensaje: `Alerta de pánico activada desde ${payload.user_context}`,
        detalles: `Dispositivo: ${payload.identificador_dispositivo}`,
        color: colors.red
      });
    } else if (payload.tipo_de_alerta === 'incendio') {
      alertas.push({
        nivel: 'CRÍTICO',
        tipo: 'INCENDIO REPORTADO',
        mensaje: `Alerta de incendio desde ${payload.user_context}`,
        detalles: `Dispositivo: ${payload.identificador_dispositivo} - Requiere bomberos`,
        color: colors.red
      });
    } else if (payload.tipo_de_alerta === 'emergencia') {
      alertas.push({
        nivel: 'ALTO',
        tipo: 'EMERGENCIA GENERAL',
        mensaje: `Emergencia reportada desde ${payload.user_context}`,
        detalles: `Dispositivo: ${payload.identificador_dispositivo}`,
        color: colors.yellow
      });
    }
  }

  // ALERTAS DE CÁMARA LPR (Lectura de Placas)
  if (event_type === 'sensor.lpr') {
    if (payload.velocidad_estimada > 100) {
      alertas.push({
        nivel: 'CRÍTICO',
        tipo: 'EXCESO DE VELOCIDAD PELIGROSO',
        mensaje: `Vehículo a ${payload.velocidad_estimada} km/h detectado`,
        detalles: `Placa: ${payload.placa_vehicular} | ${payload.color_vehiculo} ${payload.modelo_vehiculo} | Sensor: ${payload.ubicacion_sensor}`,
        color: colors.red
      });
    } else if (payload.velocidad_estimada > 70) {
      alertas.push({
        nivel: 'MEDIO',
        tipo: 'EXCESO DE VELOCIDAD',
        mensaje: `Vehículo a ${payload.velocidad_estimada} km/h en zona`,
        detalles: `Placa: ${payload.placa_vehicular} | ${payload.color_vehiculo} ${payload.modelo_vehiculo}`,
        color: colors.yellow
      });
    }

    if (payload.velocidad_estimada > 60) {
      alertas.push({
        nivel: 'INFO',
        tipo: 'REGISTRO VEHICULAR',
        mensaje: `Vehículo registrado en ${payload.ubicacion_sensor}`,
        detalles: `${payload.placa_vehicular} - ${payload.velocidad_estimada} km/h`,
        color: colors.cyan
      });
    }
  }

  // ALERTAS DE SENSOR DE VELOCIDAD
  if (event_type === 'sensor.speed') {
    if (payload.velocidad_detectada > 80) {
      alertas.push({
        nivel: 'ALTO',
        tipo: 'VELOCIDAD EXCESIVA DETECTADA',
        mensaje: `${payload.velocidad_detectada} km/h en ${payload.direccion || 'zona detectada'}`,
        detalles: `Sensor: ${payload.sensor_id} - Posible riesgo de accidente`,
        color: colors.red
      });
    } else if (payload.velocidad_detectada > 60) {
      alertas.push({
        nivel: 'MEDIO',
        tipo: 'VELOCIDAD SOBRE LÍMITE',
        mensaje: `${payload.velocidad_detectada} km/h detectada`,
        detalles: `Dirección: ${payload.direccion || 'N/A'} | Sensor: ${payload.sensor_id}`,
        color: colors.yellow
      });
    }
  }

  // ALERTAS DE SENSOR ACÚSTICO
  if (event_type === 'sensor.acoustic') {
    if (payload.tipo_sonido_detectado === 'disparo') {
      const probabilidad = (payload.probabilidad_evento_critico * 100).toFixed(1);
      alertas.push({
        nivel: 'CRÍTICO',
        tipo: 'DISPARO DETECTADO',
        mensaje: `Posible disparo de arma de fuego (${probabilidad}% confianza)`,
        detalles: `${payload.nivel_decibeles} dB - Requiere unidad policial inmediata`,
        color: colors.red
      });
    } else if (payload.tipo_sonido_detectado === 'explosion') {
      const probabilidad = (payload.probabilidad_evento_critico * 100).toFixed(1);
      alertas.push({
        nivel: 'CRÍTICO',
        tipo: 'EXPLOSIÓN DETECTADA',
        mensaje: `Posible explosión (${probabilidad}% confianza)`,
        detalles: `${payload.nivel_decibeles} dB - Requiere bomberos y policía`,
        color: colors.red
      });
    } else if (payload.tipo_sonido_detectado === 'vidrio_roto') {
      const probabilidad = (payload.probabilidad_evento_critico * 100).toFixed(1);
      alertas.push({
        nivel: 'ALTO',
        tipo: 'VIDRIO ROTO DETECTADO',
        mensaje: `Posible robo o vandalismo (${probabilidad}% confianza)`,
        detalles: `${payload.nivel_decibeles} dB - Verificar con cámaras`,
        color: colors.yellow
      });
    }

    if (payload.nivel_decibeles > 120) {
      alertas.push({
        nivel: 'ALTO',
        tipo: 'CONTAMINACIÓN ACÚSTICA EXTREMA',
        mensaje: `Nivel de ruido peligroso: ${payload.nivel_decibeles} dB`,
        detalles: 'Puede causar daños auditivos',
        color: colors.magenta
      });
    }
  }

  // ALERTAS DE REPORTE CIUDADANO
  if (event_type === 'citizen.report') {
    if (payload.tipo_evento === 'accidente') {
      alertas.push({
        nivel: 'ALTO',
        tipo: 'ACCIDENTE REPORTADO',
        mensaje: `Ciudadano reporta accidente en ${payload.ubicacion_aproximada || 'ubicación no especificada'}`,
        detalles: `${payload.mensaje_descriptivo} | Origen: ${payload.origen || 'desconocido'}`,
        color: colors.yellow
      });
    } else if (payload.tipo_evento === 'incendio') {
      alertas.push({
        nivel: 'CRÍTICO',
        tipo: 'INCENDIO REPORTADO POR CIUDADANO',
        mensaje: `Reporte de incendio en ${payload.ubicacion_aproximada || 'ubicación no especificada'}`,
        detalles: `${payload.mensaje_descriptivo} | Origen: ${payload.origen || 'desconocido'} - Alertar bomberos`,
        color: colors.red
      });
    } else if (payload.tipo_evento === 'altercado') {
      alertas.push({
        nivel: 'MEDIO',
        tipo: 'ALTERCADO REPORTADO',
        mensaje: `Disturbio en ${payload.ubicacion_aproximada || 'ubicación no especificada'}`,
        detalles: `${payload.mensaje_descriptivo} | Origen: ${payload.origen || 'desconocido'}`,
        color: colors.yellow
      });
    }
  }

  // CORRELACIÓN: Si es severidad crítica, agregar alerta adicional
  if (severity === 'critical' && alertas.length === 0) {
    alertas.push({
      nivel: 'ALTO',
      tipo: 'EVENTO CRÍTICO',
      mensaje: `Evento ${event_type} marcado como crítico`,
      detalles: 'Requiere atención inmediata',
      color: colors.red
    });
  }

  return alertas;
}

// Función para formatear y mostrar alertas
async function mostrarAlertas(event, alertas) {
  if (alertas.length === 0) return;

  const { zone, geo_lat, geo_lon, ts_utc, event_id, correlation_id } = event;
  
  console.log('\n' + '='.repeat(80));
  console.log(`${colors.bright}🚨 ALERTAS DETECTADAS 🚨${colors.reset}`);
  console.log(`Zona: ${zone} | Coords: ${geo_lat}, ${geo_lon}`);
  console.log(`Timestamp: ${ts_utc} | Event ID: ${event_id.substring(0, 8)}...`);
  console.log('-'.repeat(80));

  alertas.forEach((alerta, index) => {
    console.log(`${alerta.color}${colors.bright}[${alerta.nivel}] ${alerta.tipo}${colors.reset}`);
    console.log(`${alerta.color}  → ${alerta.mensaje}${colors.reset}`);
    console.log(`${alerta.color}  → ${alerta.detalles}${colors.reset}`);
    if (index < alertas.length - 1) {
      console.log('-'.repeat(80));
    }
  });

  console.log('='.repeat(80) + '\n');

  // Guardar alertas
  if (alertas.length > 0) {
    const alertMessage = {
      alert_id: uuidv4(),
      correlation_id: correlation_id || event_id,
      source_event_id: event_id,
      event_type: event.event_type,
      zone: zone,
      coordinates: {
        lat: geo_lat,
        lon: geo_lon
      },
      timestamp: new Date().toISOString(),
      alerts: alertas.map(a => ({
        level: a.nivel,
        type: a.tipo,
        message: a.mensaje,
        details: a.detalles
      }))
    };

    try {
      // 1. Guardar en base de datos PostgreSQL via Backend API
      try {
        const response = await axios.post(`${BACKEND_URL}/alerts`, alertMessage, {
          headers: { 'Content-Type': 'application/json' },
          timeout: 5000
        });
        console.log(`${colors.green}✓ Alertas guardadas en PostgreSQL: ${response.data.count} alerta(s)${colors.reset}`);
      } catch (dbError) {
        console.error(`${colors.yellow}⚠ Error guardando en BD: ${dbError.message}${colors.reset}`);
      }

      // 2. Indexar en Elasticsearch para búsqueda y análisis
      try {
        const esDoc = {
          ...alertMessage,
          '@timestamp': new Date().toISOString(),
          geo_location: {
            lat: geo_lat,
            lon: geo_lon
          }
        };
        
        await axios.post(`${ELASTICSEARCH_URL}/alerts/_doc`, esDoc, {
          headers: { 'Content-Type': 'application/json' },
          timeout: 5000
        });
        console.log(`${colors.magenta}✓ Alertas indexadas en Elasticsearch${colors.reset}`);
      } catch (esError) {
        if (esError.code !== 'ECONNREFUSED') {
          console.error(`${colors.yellow}⚠ Error indexando en Elasticsearch: ${esError.message}${colors.reset}`);
        }
      }
      
    } catch (error) {
      console.error(`${colors.red}✗ Error procesando alertas: ${error.message}${colors.reset}`);
    }
  }
}

// Función para procesar eventos desde PostgreSQL
async function processEventsFromDB() {
  try {
    // Query para obtener eventos nuevos
    const query = lastProcessedTimestamp
      ? 'SELECT * FROM events WHERE ts_utc > $1 ORDER BY ts_utc ASC LIMIT 50'
      : 'SELECT * FROM events ORDER BY ts_utc DESC LIMIT 10'; // Primera ejecución: últimos 10

    const values = lastProcessedTimestamp ? [lastProcessedTimestamp] : [];
    const result = await client.query(query, values);

    if (result.rows.length > 0) {
      console.log(`${colors.blue}[INFO] Procesando ${result.rows.length} eventos nuevos desde PostgreSQL${colors.reset}`);

      for (const row of result.rows) {
        // Transformar row de BD a formato de evento
        const event = {
          event_id: row.event_id,
          event_type: row.event_type,
          event_version: row.event_version,
          producer: row.producer,
          source: row.source,
          correlation_id: row.correlation_id,
          trace_id: row.trace_id,
          partition_key: row.partition_key,
          ts_utc: row.ts_utc,
          timestamp: row.ts_utc, // Alias para compatibilidad
          zone: row.zone,
          geo_lat: row.geo_lat,
          geo_lon: row.geo_lon, // Neon usa geo_lon
          severity: row.severity,
          payload: row.payload,
          geo: { // Construir objeto geo para compatibilidad
            zone: row.zone,
            lat: row.geo_lat,
            lon: row.geo_lon // Neon usa geo_lon
          }
        };

        console.log(`${colors.blue}[INFO] Evento: ${event.event_type} | Zona: ${event.zone} | Severity: ${event.severity}${colors.reset}`);

        // 1. INDEXAR EVENTO EN ELASTICSEARCH
        try {
          const eventDoc = {
            ...event,
            '@timestamp': event.ts_utc,
            'geo.location': {
              lat: event.geo_lat,
              lon: event.geo_lon
            }
          };
          
          await axios.post(`${ELASTICSEARCH_URL}/events/_doc/${event.event_id}`, eventDoc, {
            headers: { 'Content-Type': 'application/json' },
            timeout: 5000
          });
          console.log(`${colors.magenta}✓ Evento indexado en Elasticsearch (events)${colors.reset}`);
        } catch (esError) {
          if (esError.code !== 'ECONNREFUSED') {
            console.error(`${colors.yellow}⚠ Error indexando evento en Elasticsearch: ${esError.message}${colors.reset}`);
          }
        }

        // 2. DETECTAR ALERTAS Y PROCESARLAS
        const alertas = detectarAlertas(event);
        await mostrarAlertas(event, alertas);

        // Actualizar último timestamp procesado
        lastProcessedTimestamp = row.ts_utc;
      }
    } else {
      // Silencioso cuando no hay eventos nuevos
      process.stdout.write('.');
    }
  } catch (error) {
    console.error(`${colors.red}✗ Error procesando eventos: ${error.message}${colors.reset}`);
  }
}

// Función principal
const run = async () => {
  try {
    // Conectar a PostgreSQL
    await client.connect();
    console.log(`${colors.bright}${colors.green}✓ Conectado a PostgreSQL (Azure)${colors.reset}`);
    console.log(`${colors.green}  Host: ${pgConfig.host}${colors.reset}`);
    console.log(`${colors.green}  Database: ${pgConfig.database}${colors.reset}\n`);

    console.log(`${colors.bright}${colors.cyan}Consumer PostgreSQL iniciado - Polling cada ${POLL_INTERVAL_MS/1000}s${colors.reset}`);
    console.log(`${colors.cyan}Leyendo eventos desde tabla: events${colors.reset}`);
    console.log(`${colors.cyan}Detectando patrones y generando alertas${colors.reset}`);
    console.log(`${colors.cyan}Guardando en: PostgreSQL + Elasticsearch${colors.reset}\n`);

    // Procesar eventos inmediatamente
    await processEventsFromDB();

    // Polling cada POLL_INTERVAL_MS
    setInterval(async () => {
      await processEventsFromDB();
    }, POLL_INTERVAL_MS);

  } catch (error) {
    console.error(`${colors.red}✗ Error fatal: ${error.message}${colors.reset}`);
    process.exit(1);
  }
};

// Manejo de cierre graceful
process.on('SIGINT', async () => {
  console.log(`\n${colors.yellow}Cerrando conexión a PostgreSQL...${colors.reset}`);
  await client.end();
  process.exit(0);
});

run().catch(console.error);
