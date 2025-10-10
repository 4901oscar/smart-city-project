const { Kafka } = require('kafkajs');

// Configuración Kafka
const kafka = new Kafka({
  clientId: 'dlq-monitor',
  brokers: ['localhost:9092']
});

const consumer = kafka.consumer({ groupId: 'dlq-monitor-group' });

// Colores para consola
const colors = {
  reset: '\x1b[0m',
  bright: '\x1b[1m',
  red: '\x1b[31m',
  yellow: '\x1b[33m',
  magenta: '\x1b[35m',
  cyan: '\x1b[36m'
};

const run = async () => {
  await consumer.connect();
  await consumer.subscribe({ topic: 'events.dlq', fromBeginning: true });

  console.log(`${colors.bright}${colors.red}╔═══════════════════════════════════════════════════════════╗${colors.reset}`);
  console.log(`${colors.bright}${colors.red}║   ⚠️  MONITOR DE DEAD LETTER QUEUE (DLQ) - INICIADO   ║${colors.reset}`);
  console.log(`${colors.bright}${colors.red}╚═══════════════════════════════════════════════════════════╝${colors.reset}\n`);
  console.log(`${colors.yellow}Escuchando topic: events.dlq${colors.reset}`);
  console.log(`${colors.yellow}Monitoreando eventos fallidos...\n${colors.reset}`);

  let errorCount = 0;
  let errorsByReason = {};

  await consumer.run({
    eachMessage: async ({ topic, partition, message }) => {
      const dlqData = JSON.parse(message.value.toString());
      errorCount++;

      const { original_event, validation_errors, error, timestamp, reason } = dlqData;

      // Actualizar estadísticas
      if (!errorsByReason[reason]) errorsByReason[reason] = 0;
      errorsByReason[reason]++;

      console.log('\n' + '═'.repeat(90));
      console.log(`${colors.bright}${colors.red}⚠️  ERROR #${errorCount} - ${reason}${colors.reset}`);
      console.log('─'.repeat(90));
      console.log(`${colors.cyan}Timestamp:${colors.reset}       ${new Date(timestamp).toLocaleString('es-GT')}`);
      
      if (original_event) {
        console.log(`${colors.cyan}Event Type:${colors.reset}      ${original_event.event_type || 'UNKNOWN'}`);
        console.log(`${colors.cyan}Event ID:${colors.reset}        ${original_event.event_id ? original_event.event_id.substring(0, 13) + '...' : 'N/A'}`);
        console.log(`${colors.cyan}Producer:${colors.reset}        ${original_event.producer || 'UNKNOWN'}`);
      }

      console.log('─'.repeat(90));

      // Mostrar errores de validación
      if (validation_errors && validation_errors.length > 0) {
        console.log(`${colors.yellow}${colors.bright}Errores de Validación:${colors.reset}`);
        validation_errors.forEach((err, index) => {
          console.log(`${colors.yellow}  ${index + 1}. ${err}${colors.reset}`);
        });
      }

      // Mostrar error de procesamiento
      if (error) {
        console.log(`${colors.red}${colors.bright}Error de Procesamiento:${colors.reset}`);
        console.log(`${colors.red}  ${error}${colors.reset}`);
      }

      // Mostrar fragmento del evento original
      if (original_event) {
        console.log('─'.repeat(90));
        console.log(`${colors.magenta}Evento Original (fragmento):${colors.reset}`);
        const preview = JSON.stringify(original_event, null, 2).substring(0, 300);
        console.log(`${colors.magenta}${preview}${preview.length >= 300 ? '...' : ''}${colors.reset}`);
      }

      console.log('═'.repeat(90));

      // Mostrar estadísticas cada 5 errores
      if (errorCount % 5 === 0) {
        console.log(`\n${colors.bright}${colors.red}📊 ESTADÍSTICAS DE ERRORES:${colors.reset}`);
        console.log(`${colors.red}Total errores: ${errorCount}${colors.reset}`);
        console.log(`${colors.red}Por tipo de error:${colors.reset}`);
        Object.entries(errorsByReason).forEach(([reason, count]) => {
          console.log(`  ${colors.yellow}${reason}: ${count}${colors.reset}`);
        });
        console.log('');
      }
    },
  });
};

run().catch(error => {
  console.error(`${colors.red}Error fatal en monitor de DLQ:${colors.reset}`, error);
  process.exit(1);
});
