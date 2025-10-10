const { Kafka } = require('kafkajs');
const { v4: uuidv4 } = require('uuid');
const axios = require('axios');

// Configuración Kafka
const kafka = new Kafka({
  clientId: 'consumer-app',
  brokers: ['localhost:9092']
});

const consumer = kafka.consumer({ groupId: 'test-group' });
const producer = kafka.producer(); // Producer para publicar alertas

// URL del backend
const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:5000';

// Colores para consola
const colors = {
  reset: '\x1b[0m',
  bright: '\x1b[1m',
  red: '\x1b[31m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m',
  magenta: '\x1b[35m',
  cyan: '\x1b[36m'
};

// Función para detectar alertas basadas en eventos
function detectarAlertas(event) {
  const alertas = [];
  const { event_type, payload, geo, severity, timestamp } = event;

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
    // Exceso de velocidad severo
    if (payload.velocidad_estimada > 100) {
      alertas.push({
        nivel: 'CRÍTICO',
        tipo: 'EXCESO DE VELOCIDAD PELIGROSO',
        mensaje: `Vehículo a ${payload.velocidad_estimada} km/h detectado`,
        detalles: `Placa: ${payload.placa_vehicular} | ${payload.color_vehiculo} ${payload.modelo_vehiculo} | Sensor: ${payload.ubicacion_sensor}`,
        color: colors.red
      });
    } 
    // Exceso moderado
    else if (payload.velocidad_estimada > 70) {
      alertas.push({
        nivel: 'MEDIO',
        tipo: 'EXCESO DE VELOCIDAD',
        mensaje: `Vehículo a ${payload.velocidad_estimada} km/h en zona`,
        detalles: `Placa: ${payload.placa_vehicular} | ${payload.color_vehiculo} ${payload.modelo_vehiculo}`,
        color: colors.yellow
      });
    }

    // Registro de vehículo para correlación futura
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
        mensaje: `${payload.velocidad_detectada} km/h en ${payload.direccion}`,
        detalles: `Sensor: ${payload.sensor_id} - Posible riesgo de accidente`,
        color: colors.red
      });
    } else if (payload.velocidad_detectada > 60) {
      alertas.push({
        nivel: 'MEDIO',
        tipo: 'VELOCIDAD SOBRE LÍMITE',
        mensaje: `${payload.velocidad_detectada} km/h detectada`,
        detalles: `Dirección: ${payload.direccion} | Sensor: ${payload.sensor_id}`,
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

    // Alerta adicional por nivel de ruido extremo
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
        mensaje: `Ciudadano reporta accidente en ${payload.ubicacion_aproximada}`,
        detalles: `${payload.mensaje_descriptivo} | Origen: ${payload.origen}`,
        color: colors.yellow
      });
    } else if (payload.tipo_evento === 'incendio') {
      alertas.push({
        nivel: 'CRÍTICO',
        tipo: 'INCENDIO REPORTADO POR CIUDADANO',
        mensaje: `Reporte de incendio en ${payload.ubicacion_aproximada}`,
        detalles: `${payload.mensaje_descriptivo} | Origen: ${payload.origen} - Alertar bomberos`,
        color: colors.red
      });
    } else if (payload.tipo_evento === 'altercado') {
      alertas.push({
        nivel: 'MEDIO',
        tipo: 'ALTERCADO REPORTADO',
        mensaje: `Disturbio en ${payload.ubicacion_aproximada}`,
        detalles: `${payload.mensaje_descriptivo} | Origen: ${payload.origen}`,
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

  const { geo, timestamp, event_id, correlation_id } = event;
  
  console.log('\n' + '='.repeat(80));
  console.log(`${colors.bright}🚨 ALERTAS DETECTADAS 🚨${colors.reset}`);
  console.log(`Zona: ${geo.zone} | Coords: ${geo.lat}, ${geo.lon}`);
  console.log(`Timestamp: ${timestamp} | Event ID: ${event_id.substring(0, 8)}...`);
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

  // Publicar alertas a topic correlated.alerts
  if (alertas.length > 0) {
    const alertMessage = {
      alert_id: uuidv4(),
      correlation_id: correlation_id || event_id,
      source_event_id: event_id,
      event_type: event.event_type,
      zone: geo.zone,
      coordinates: {
        lat: geo.lat,
        lon: geo.lon
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
      // 1. Publicar a Kafka topic correlated.alerts
      await producer.send({
        topic: 'correlated.alerts',
        messages: [
          {
            key: geo.zone,
            value: JSON.stringify(alertMessage)
          }
        ]
      });
      console.log(`${colors.cyan}✓ Alertas publicadas a correlated.alerts${colors.reset}`);

      // 2. Guardar en base de datos PostgreSQL via Backend API
      try {
        const response = await axios.post(`${BACKEND_URL}/alerts`, alertMessage, {
          headers: { 'Content-Type': 'application/json' },
          timeout: 5000
        });
        console.log(`${colors.green}✓ Alertas guardadas en BD: ${response.data.count} alerta(s)${colors.reset}`);
      } catch (dbError) {
        console.error(`${colors.yellow}⚠ Error guardando en BD: ${dbError.message}${colors.reset}`);
        // No detener el proceso si falla el guardado en BD
      }
      
    } catch (error) {
      console.error(`${colors.red}✗ Error publicando alertas: ${error.message}${colors.reset}`);
    }
  }
}

const run = async () => {
  await consumer.connect();
  await producer.connect(); // Conectar producer
  
  // Suscribirse a events.standardized (topic de eventos válidos)
  await consumer.subscribe({ topic: 'events.standardized', fromBeginning: true });

  console.log(`${colors.bright}${colors.cyan}Consumer iniciado - Sistema de alertas inteligente activado${colors.reset}`);
  console.log(`${colors.cyan}Monitoreando: eventos de pánico, velocidad, acústicos y reportes ciudadanos${colors.reset}`);
  console.log(`${colors.cyan}Publicando alertas a: correlated.alerts${colors.reset}\n`);

  await consumer.run({
    eachMessage: async ({ topic, partition, message }) => {
      const event = JSON.parse(message.value.toString());
      
      // Log básico del evento
      console.log(`${colors.blue}[INFO] Evento recibido: ${event.event_type} | Zona: ${event.geo.zone}${colors.reset}`);

      // Detectar y mostrar alertas
      const alertas = detectarAlertas(event);
      await mostrarAlertas(event, alertas);
    },
  });
};

run().catch(console.error);