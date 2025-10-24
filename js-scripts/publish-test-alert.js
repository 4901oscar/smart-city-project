const { Kafka } = require('kafkajs');

const kafka = new Kafka({ clientId: 'test-publisher', brokers: ['localhost:9092'] });
const producer = kafka.producer();

async function run() {
  await producer.connect();
  const alertMessage = {
    alert_id: 'test-' + Date.now(),
    correlation_id: 'corr-test',
    source_event_id: 'evt-test',
    event_type: 'sensor.lpr',
    zone: 'Zona 10',
    coordinates: { lat: 14.6091, lon: -90.5252 },
    timestamp: new Date().toISOString(),
    alerts: [
      { level: 'CRÍTICO', type: 'EXCESO DE VELOCIDAD PELIGROSO', message: 'Vehículo a 130 km/h', details: 'Placa: ABC123' }
    ]
  };

  await producer.send({
    topic: 'correlated.alerts',
    messages: [ { key: alertMessage.zone, value: JSON.stringify(alertMessage) } ]
  });

  console.log('Mensaje de prueba enviado a correlated.alerts:', alertMessage.alert_id);
  await producer.disconnect();
}

run().catch(err => { console.error(err); process.exit(1); });
