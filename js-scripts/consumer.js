const { Kafka } = require('kafkajs');

// Configuración Kafka
const kafka = new Kafka({
  clientId: 'consumer-app',
  brokers: ['localhost:9092']
});

const consumer = kafka.consumer({ groupId: 'test-group' });

const run = async () => {
  await consumer.connect();
  await consumer.subscribe({ topic: 'events-topic', fromBeginning: true });

  await consumer.run({
    eachMessage: async ({ topic, partition, message }) => {
      const event = JSON.parse(message.value.toString());
      console.log('Evento recibido de Kafka:', event);

      // Simular correlación y alerta (básica, en Zona 10)
      if (event.geo.zone === 'Zona 10') {
        let alerta = '';
        if (event.event_type === 'panic.button' && event.payload.velocidad_estimada > 85) {
          alerta = 'ALERTA: Posible robo en curso en Zona 10';
        } else if (event.event_type === 'panic.button') {
          alerta = 'ALERTA: Emergencia en Zona 10';
        }
        if (alerta) {
          console.log(alerta, '- Coords:', event.geo.lat, event.geo.lon);
        }
      }
    },
  });
};

run().catch(console.error);
console.log('Consumer iniciado - Leyendo eventos de Kafka para alertas en Zona 10...');