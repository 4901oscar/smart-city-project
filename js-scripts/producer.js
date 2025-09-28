const axios = require('axios');
const { v4: uuidv4 } = require('uuid');

// Configuración
const BACKEND_URL = 'http://localhost:5000/events';
const ZONA_10 = { zone: 'Zona 10', lat: 14.6091, lon: -90.5252 };

// Tipos de eventos (basado en proyecto)
const EVENT_TYPES = ['panic.button', 'sensor.lpr', 'sensor.speed', 'sensor.acoustic', 'citizen.report'];

// Payloads simulados
function generatePayload(type) {
  switch (type) {
    case 'panic.button':
      return { tipo_de_alerta: ['panico', 'emergencia', 'incendio'][Math.floor(Math.random() * 3)] };
    case 'sensor.lpr':
      return { placa_vehicular: 'ABC' + Math.floor(Math.random() * 1000), velocidad_estimada: Math.floor(Math.random() * 100) + 50 };
    case 'sensor.speed':
      return { velocidad: Math.floor(Math.random() * 120) + 20 };
    case 'sensor.acoustic':
      return { sound_type: 'disparo', decibels: Math.floor(Math.random() * 100) + 80 };
    case 'citizen.report':
      return { tipo_evento: 'accidente', mensaje_descriptivo: 'Accidente en intersección' };
    default:
      return {};
  }
}

// Enviar evento
async function sendEvent() {
  const eventType = EVENT_TYPES[Math.floor(Math.random() * EVENT_TYPES.length)];
  const payload = generatePayload(eventType);

  const eventData = {
    event_version: '1.0',
    event_type: eventType,
    event_id: uuidv4(),
    producer: 'js-sim',
    source: 'simulated',
    correlation_id: uuidv4(),
    trace_id: uuidv4(),
    timestamp: new Date().toISOString(),
    partition_key: 'zone_10',
    geo: ZONA_10,
    severity: ['info', 'warning', 'critical'][Math.floor(Math.random() * 3)],
    payload: payload
  };

  try {
    const response = await axios.post(BACKEND_URL, eventData);
    console.log('Evento enviado:', response.data);
  } catch (error) {
    console.error('Error al enviar evento:', error.response?.data || error.message);
  }
}

// Simular eventos cada 5 segundos
setInterval(sendEvent, 5000);
console.log('Producer iniciado - Enviando eventos simulados a Zona 10...');