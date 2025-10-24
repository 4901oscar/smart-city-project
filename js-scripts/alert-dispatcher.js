const { Kafka } = require('kafkajs');
const axios = require('axios');

// Configuración
const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:5000';

// Configuración Kafka
const kafka = new Kafka({
  clientId: 'alert-dispatcher',
  brokers: ['localhost:9092']
});

const consumer = kafka.consumer({ groupId: 'dispatch-group' });

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

// Mapeo de tipos de alerta a entidades responsables
const ALERT_ROUTING = {
  'EXCESO DE VELOCIDAD': ['policia-transito'],
  'EXCESO DE VELOCIDAD PELIGROSO': ['policia-transito', 'policia-nacional'],
  'INCENDIO REPORTADO': ['bomberos'],
  'ACCIDENTE REPORTADO': ['bomberos-voluntarios', 'cruz-roja'],
  'ALTERCADO REPORTADO': ['policia-municipal'],
  'DISPARO DETECTADO': ['policia-nacional'],
  'EXPLOSIÓN DETECTADA': ['policia-nacional', 'bomberos'],
  'VIDRIO ROTO DETECTADO': ['policia-nacional'],
  'RUIDO EXCESIVO': ['policia-municipal'],
  'EMERGENCIA PERSONAL': ['policia-nacional', 'cruz-roja'],
  'EMERGENCIA GENERAL': ['policia-municipal'],
  'VELOCIDAD PELIGROSA RADAR': ['policia-transito'],
  'ANOMALÍA DE TRÁFICO': ['policia-transito']
};

// Función para clasificar y determinar entidades destino
function classifyAlert(alertData) {
  const entities = [];
  
  // Extraer todos los tipos de alerta del mensaje
  alertData.alerts.forEach(alert => {
    const alertType = alert.type;
    const routing = ALERT_ROUTING[alertType];
    
    if (routing) {
      routing.forEach(entity => {
        if (!entities.includes(entity)) {
          entities.push(entity);
        }
      });
    }
  });

  // Si es crítico, siempre notificar a policía nacional
  const hasCritical = alertData.alerts.some(a => a.level === 'CRÍTICO');
  if (hasCritical && !entities.includes('policia-nacional')) {
    entities.push('policia-nacional');
  }

  return entities;
}

// Función para enviar alerta a una entidad específica
async function dispatchToEntity(entity, alertData) {
  try {
    const response = await axios.post(`${BACKEND_URL}/dispatch/${entity}`, alertData, {
      headers: { 'Content-Type': 'application/json' },
      timeout: 5000
    });
    
    console.log(`${colors.green}  ✓ Despachado a ${entity.toUpperCase().replace('-', ' ')}: ${response.data.status}${colors.reset}`);
    return { entity, success: true, response: response.data };
  } catch (error) {
    console.error(`${colors.red}  ✗ Error despachando a ${entity}: ${error.message}${colors.reset}`);
    return { entity, success: false, error: error.message };
  }
}

// Función principal para procesar y despachar alertas
async function processAndDispatchAlert(alertData) {
  console.log(`\n${colors.cyan}${'='.repeat(80)}`);
  console.log(`🚨 PROCESANDO ALERTA PARA DESPACHO`);
  console.log(`${'='.repeat(80)}${colors.reset}`);
  
  console.log(`${colors.bright}Alert ID:${colors.reset} ${alertData.alert_id}`);
  console.log(`${colors.bright}Zona:${colors.reset} ${alertData.zone}`);
  console.log(`${colors.bright}Timestamp:${colors.reset} ${alertData.timestamp}`);
  console.log(`${colors.bright}Alertas:${colors.reset} ${alertData.alerts.length}`);
  
  // Mostrar alertas
  alertData.alerts.forEach((alert, idx) => {
    const levelColor = alert.level === 'CRÍTICO' ? colors.red : 
                       alert.level === 'ALTO' ? colors.yellow : colors.blue;
    console.log(`\n  ${levelColor}[${alert.level}]${colors.reset} ${alert.type}`);
    console.log(`  ${alert.message}`);
    console.log(`  ${colors.magenta}${alert.details}${colors.reset}`);
  });

  // Clasificar y determinar entidades destino
  const targetEntities = classifyAlert(alertData);
  
  if (targetEntities.length === 0) {
    console.log(`\n${colors.yellow}⚠ No se encontró routing para esta alerta. Enviando a policía municipal por defecto.${colors.reset}`);
    targetEntities.push('policia-municipal');
  }

  console.log(`\n${colors.cyan}📤 DESPACHANDO A: ${targetEntities.join(', ').toUpperCase().replace(/-/g, ' ')}${colors.reset}`);
  console.log(`${colors.cyan}${'-'.repeat(80)}${colors.reset}`);

  // Despachar a cada entidad
  const dispatchPromises = targetEntities.map(entity => dispatchToEntity(entity, alertData));
  const results = await Promise.all(dispatchPromises);

  // Resumen
  const successful = results.filter(r => r.success).length;
  const failed = results.filter(r => !r.success).length;
  
  console.log(`\n${colors.cyan}📊 RESUMEN DE DESPACHO:${colors.reset}`);
  console.log(`  ${colors.green}✓ Exitosos: ${successful}${colors.reset}`);
  if (failed > 0) {
    console.log(`  ${colors.red}✗ Fallidos: ${failed}${colors.reset}`);
  }
  console.log(`${colors.cyan}${'='.repeat(80)}${colors.reset}\n`);
}

// Función principal
async function run() {
  console.log(`${colors.bright}${colors.cyan}`);
  console.log('╔════════════════════════════════════════════════════════════════════════════╗');
  console.log('║                    SMART CITY ALERT DISPATCHER                             ║');
  console.log('║                    Dispatcher de Alertas a Entidades                       ║');
  console.log('╚════════════════════════════════════════════════════════════════════════════╝');
  console.log(colors.reset);
  console.log(`${colors.bright}Conectando a Kafka...${colors.reset}`);
  
  await consumer.connect();
  console.log(`${colors.green}✓ Conectado a Kafka${colors.reset}`);
  
  console.log(`${colors.bright}Suscribiéndose a topic: correlated.alerts${colors.reset}`);
  // Para pruebas locales queremos leer mensajes existentes si los hay; cambiar a `false` en producción si no lo desea
  await consumer.subscribe({ topic: 'correlated.alerts', fromBeginning: true });
  console.log(`${colors.green}✓ Suscrito correctamente${colors.reset}`);
  
  console.log(`\n${colors.cyan}Esperando alertas correlacionadas para despacho...${colors.reset}\n`);

  await consumer.run({
    eachMessage: async ({ topic, partition, message }) => {
      try {
        const alertData = JSON.parse(message.value.toString());
        await processAndDispatchAlert(alertData);
      } catch (error) {
        console.error(`${colors.red}Error procesando mensaje: ${error.message}${colors.reset}`);
        console.error(error);
      }
    },
  });
}

// Manejo de errores y señales
process.on('SIGINT', async () => {
  console.log(`\n${colors.yellow}Cerrando dispatcher...${colors.reset}`);
  await consumer.disconnect();
  process.exit(0);
});

process.on('unhandledRejection', (error) => {
  console.error(`${colors.red}Error no manejado:${colors.reset}`, error);
});

// Iniciar el dispatcher
run().catch(error => {
  console.error(`${colors.red}Error fatal:${colors.reset}`, error);
  process.exit(1);
});
