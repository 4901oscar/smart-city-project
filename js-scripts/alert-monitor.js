const { Kafka } = require('kafkajs');

// Configuración Kafka
const kafka = new Kafka({
  clientId: 'alert-monitor',
  brokers: ['localhost:9092']
});

const consumer = kafka.consumer({ groupId: 'alert-monitor-group' });

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

// Mapeo de colores por nivel
const nivelColors = {
  'CRÍTICO': colors.red,
  'ALTO': colors.yellow,
  'MEDIO': colors.blue,
  'INFO': colors.cyan
};

const run = async () => {
  await consumer.connect();
  await consumer.subscribe({ topic: 'correlated.alerts', fromBeginning: true });

  console.log(`${colors.bright}${colors.green}╔═══════════════════════════════════════════════════════════╗${colors.reset}`);
  console.log(`${colors.bright}${colors.green}║   📊 MONITOR DE ALERTAS CORRELACIONADAS - INICIADO      ║${colors.reset}`);
  console.log(`${colors.bright}${colors.green}╚═══════════════════════════════════════════════════════════╝${colors.reset}\n`);
  console.log(`${colors.cyan}Escuchando topic: correlated.alerts${colors.reset}`);
  console.log(`${colors.cyan}Esperando alertas del sistema...\n${colors.reset}`);

  let alertCount = 0;
  let alertsByLevel = { 'CRÍTICO': 0, 'ALTO': 0, 'MEDIO': 0, 'INFO': 0 };
  let alertsByZone = {};

  await consumer.run({
    eachMessage: async ({ topic, partition, message }) => {
      const alertData = JSON.parse(message.value.toString());
      alertCount++;

      const { alert_id, source_event_id, event_type, zone, coordinates, timestamp, alerts } = alertData;

      // Actualizar estadísticas
      if (!alertsByZone[zone]) alertsByZone[zone] = 0;
      alertsByZone[zone]++;

      console.log('\n' + '═'.repeat(90));
      console.log(`${colors.bright}${colors.green}🔔 ALERTA CORRELACIONADA #${alertCount}${colors.reset}`);
      console.log('─'.repeat(90));
      console.log(`${colors.cyan}Alert ID:${colors.reset}        ${alert_id.substring(0, 13)}...`);
      console.log(`${colors.cyan}Event ID:${colors.reset}        ${source_event_id.substring(0, 13)}...`);
      console.log(`${colors.cyan}Tipo Evento:${colors.reset}     ${event_type}`);
      console.log(`${colors.cyan}Zona:${colors.reset}            ${zone}`);
      console.log(`${colors.cyan}Coordenadas:${colors.reset}     ${coordinates.lat}, ${coordinates.lon}`);
      console.log(`${colors.cyan}Timestamp:${colors.reset}       ${new Date(timestamp).toLocaleString('es-GT')}`);
      console.log('─'.repeat(90));

      // Mostrar cada alerta
      alerts.forEach((alert, index) => {
        const alertColor = nivelColors[alert.level] || colors.reset;
        alertsByLevel[alert.level] = (alertsByLevel[alert.level] || 0) + 1;

        console.log(`${alertColor}${colors.bright}[${alert.level}] ${alert.type}${colors.reset}`);
        console.log(`${alertColor}  ➤ ${alert.message}${colors.reset}`);
        console.log(`${alertColor}  ➤ ${alert.details}${colors.reset}`);
        
        if (index < alerts.length - 1) {
          console.log('  ' + '·'.repeat(43));
        }
      });

      console.log('═'.repeat(90));

      // Mostrar estadísticas cada 5 alertas
      if (alertCount % 5 === 0) {
        console.log(`\n${colors.bright}${colors.magenta}📈 ESTADÍSTICAS DEL SISTEMA:${colors.reset}`);
        console.log(`${colors.magenta}Total alertas procesadas: ${alertCount}${colors.reset}`);
        console.log(`${colors.magenta}Por nivel:${colors.reset}`);
        Object.entries(alertsByLevel).forEach(([nivel, count]) => {
          if (count > 0) {
            console.log(`  ${nivelColors[nivel]}${nivel}: ${count}${colors.reset}`);
          }
        });
        console.log(`${colors.magenta}Por zona:${colors.reset}`);
        Object.entries(alertsByZone).forEach(([zona, count]) => {
          console.log(`  ${colors.cyan}${zona}: ${count}${colors.reset}`);
        });
        console.log('');
      }
    },
  });
};

run().catch(error => {
  console.error(`${colors.red}Error fatal en monitor de alertas:${colors.reset}`, error);
  process.exit(1);
});
