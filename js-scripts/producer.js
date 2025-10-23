const axios = require('axios');
const { v4: uuidv4 } = require('uuid');

// Configuración
const BACKEND_URL = 'http://localhost:5000/events';

// Múltiples zonas para simular ciudad inteligente
const ZONAS = [
  { zone: 'Zona 10', lat: 14.6091, lon: -90.5252 },
  { zone: 'Zona 1', lat: 14.6349, lon: -90.5069 },
  { zone: 'Zona 4', lat: 14.6198, lon: -90.4789 },
  { zone: 'Zona 9', lat: 14.5958, lon: -90.5025 },
  { zone: 'Zona 13', lat: 14.6070, lon: -90.4842 },
  { zone: 'Centro Histórico', lat: 14.6407, lon: -90.5133 }
];

// Tipos de eventos (basado en proyecto)
const EVENT_TYPES = ['panic.button', 'sensor.lpr', 'sensor.speed', 'sensor.acoustic', 'citizen.report'];

// Nombres y modelos de vehículos para simulación realista
const PLACAS_PREFIJOS = ['P', 'C', 'A', 'M', 'O'];
const MODELOS_VEHICULOS = ['Toyota Corolla', 'Honda Civic', 'Mazda 3', 'Nissan Sentra', 'Hyundai Elantra', 'Ford Mustang', 'Chevrolet Spark'];
const COLORES_VEHICULOS = ['blanco', 'negro', 'gris', 'rojo', 'azul', 'plateado'];
const SENSORES_LPR = ['Av. Reforma', '6ta Avenida', 'Blvd. Los Próceres', 'Diagonal 6', 'Calzada Roosevelt'];
const DIRECCIONES = ['Norte', 'Sur', 'Este', 'Oeste', 'Noreste', 'Suroeste'];
const DISPOSITIVOS_PANIC = ['BTN-Z10-001', 'BTN-Z10-002', 'BTN-Z10-003', 'KIOSK-Z10-01', 'APP-MOBILE-001'];
const USER_CONTEXTS = ['movil', 'quiosco', 'web'];
const UBICACIONES_CIUDADANO = ['6ta Avenida y 12 calle', 'Plaza central', 'Parque zona 10', 'Centro comercial', 'Avenida reforma'];

// Payloads simulados con datos completos según schemas
function generatePayload(type) {
  switch (type) {
    case 'panic.button':
      const tipoAlerta = ['panico', 'emergencia', 'incendio'][Math.floor(Math.random() * 3)];
      return {
        tipo_de_alerta: tipoAlerta,
        identificador_dispositivo: DISPOSITIVOS_PANIC[Math.floor(Math.random() * DISPOSITIVOS_PANIC.length)],
        user_context: USER_CONTEXTS[Math.floor(Math.random() * USER_CONTEXTS.length)]
      };
      
    case 'sensor.lpr':
      const prefijo = PLACAS_PREFIJOS[Math.floor(Math.random() * PLACAS_PREFIJOS.length)];
      const numeros = Math.floor(Math.random() * 1000).toString().padStart(3, '0');
      const letras = String.fromCharCode(65 + Math.floor(Math.random() * 26)) + 
                     String.fromCharCode(65 + Math.floor(Math.random() * 26));
      return {
        placa_vehicular: `${prefijo}-${numeros}${letras}`,
        velocidad_estimada: Math.floor(Math.random() * 80) + 30, // 30-110 km/h
        modelo_vehiculo: MODELOS_VEHICULOS[Math.floor(Math.random() * MODELOS_VEHICULOS.length)],
        color_vehiculo: COLORES_VEHICULOS[Math.floor(Math.random() * COLORES_VEHICULOS.length)],
        ubicacion_sensor: SENSORES_LPR[Math.floor(Math.random() * SENSORES_LPR.length)]
      };
      
    case 'sensor.speed':
      return {
        velocidad_detectada: Math.floor(Math.random() * 100) + 20, // 20-120 km/h
        sensor_id: `SPEED-Z10-${Math.floor(Math.random() * 10).toString().padStart(3, '0')}`,
        direccion: DIRECCIONES[Math.floor(Math.random() * DIRECCIONES.length)]
      };
      
    case 'sensor.acoustic':
      const sonidos = ['disparo', 'explosion', 'vidrio_roto'];
      const tipoSonido = sonidos[Math.floor(Math.random() * sonidos.length)];
      let decibeles, probabilidad;
      
      if (tipoSonido === 'disparo') {
        decibeles = Math.floor(Math.random() * 30) + 140; // 140-170 dB
        probabilidad = 0.6 + Math.random() * 0.35; // 60-95%
      } else if (tipoSonido === 'explosion') {
        decibeles = Math.floor(Math.random() * 40) + 160; // 160-200 dB
        probabilidad = 0.7 + Math.random() * 0.25; // 70-95%
      } else {
        decibeles = Math.floor(Math.random() * 30) + 85; // 85-115 dB
        probabilidad = 0.5 + Math.random() * 0.4; // 50-90%
      }
      
      return {
        tipo_sonido_detectado: tipoSonido,
        nivel_decibeles: decibeles,
        probabilidad_evento_critico: Math.round(probabilidad * 100) / 100
      };
      
    case 'citizen.report':
      const tiposEvento = ['accidente', 'incendio', 'altercado'];
      const tipoEvento = tiposEvento[Math.floor(Math.random() * tiposEvento.length)];
      const mensajes = {
        'accidente': [
          'Choque entre dos vehículos',
          'Motociclista accidentado',
          'Vehículo volcado en la vía',
          'Accidente con heridos'
        ],
        'incendio': [
          'Humo saliendo de edificio',
          'Fuego en contenedor de basura',
          'Incendio en vehículo estacionado',
          'Llamas visibles en local comercial'
        ],
        'altercado': [
          'Personas discutiendo violentamente',
          'Pelea en vía pública',
          'Disturbio cerca de bar',
          'Grupo causando desorden'
        ]
      };
      
      return {
        tipo_evento: tipoEvento,
        mensaje_descriptivo: mensajes[tipoEvento][Math.floor(Math.random() * mensajes[tipoEvento].length)],
        ubicacion_aproximada: UBICACIONES_CIUDADANO[Math.floor(Math.random() * UBICACIONES_CIUDADANO.length)],
        origen: ['usuario', 'app', 'punto_fisico'][Math.floor(Math.random() * 3)]
      };
      
    default:
      return {};
  }
}

// Enviar evento
async function sendEvent() {
  const eventType = EVENT_TYPES[Math.floor(Math.random() * EVENT_TYPES.length)];
  const payload = generatePayload(eventType);

  // Asignar severidad basada en el tipo de evento y datos del payload
  let severity = 'info';
  
  if (eventType === 'panic.button') {
    severity = payload.tipo_de_alerta === 'panico' || payload.tipo_de_alerta === 'incendio' ? 'critical' : 'warning';
  } else if (eventType === 'sensor.lpr') {
    severity = payload.velocidad_estimada > 100 ? 'critical' : payload.velocidad_estimada > 70 ? 'warning' : 'info';
  } else if (eventType === 'sensor.speed') {
    severity = payload.velocidad_detectada > 80 ? 'critical' : payload.velocidad_detectada > 60 ? 'warning' : 'info';
  } else if (eventType === 'sensor.acoustic') {
    if (payload.tipo_sonido_detectado === 'disparo' || payload.tipo_sonido_detectado === 'explosion') {
      severity = 'critical';
    } else if (payload.probabilidad_evento_critico > 0.7) {
      severity = 'warning';
    }
  } else if (eventType === 'citizen.report') {
    severity = payload.tipo_evento === 'incendio' ? 'critical' : payload.tipo_evento === 'accidente' ? 'warning' : 'info';
  }

  const randomZone = ZONAS[Math.floor(Math.random() * ZONAS.length)];
  
  const eventData = {
    event_version: '1.0',
    event_type: eventType,
    event_id: uuidv4(),
    producer: 'js-sim',
    source: 'simulated',
    correlation_id: uuidv4(),
    trace_id: uuidv4(),
    timestamp: new Date().toISOString(),
    partition_key: randomZone.zone.toLowerCase().replace(' ', '_'),
    geo: randomZone,
    severity: severity,
    payload: payload
  };

  try {
    const response = await axios.post(BACKEND_URL, eventData);
    console.log(`✓ [${eventType}] ${randomZone.zone} - Severity: ${severity} - ${response.data}`);
  } catch (error) {
    console.error('✗ Error al enviar evento:', error.response?.data || error.message);
  }
}

// Simular eventos cada 3 segundos para ver más variedad
setInterval(sendEvent, 3000);
console.log('🚀 Producer iniciado - Enviando eventos simulados a múltiples zonas...');
console.log('📊 Generando eventos en:', ZONAS.map(z => z.zone).join(', '));
console.log('📊 Generando eventos: panic.button, sensor.lpr, sensor.speed, sensor.acoustic, citizen.report\n');