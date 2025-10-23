// Test script para verificar consultas de Grafana
const { Client } = require('pg');

const client = new Client({
  connectionString: 'postgresql://smartcity_owner:OvSQI0a1q1OU@ep-jolly-shadow-a53s8kk9.us-east-2.aws.neon.tech/smartcity?sslmode=require'
});

async function testQueries() {
  try {
    await client.connect();
    console.log('✅ Conectado a PostgreSQL');

    // 1. Consulta de alertas por zona (panel que muestra "No data")
    console.log('\n🔍 Testing: Alertas por Zona');
    const zonasQuery = `
      SELECT 
        zone as "Zona",
        COUNT(*) as "Cantidad"
      FROM alerts 
      GROUP BY zone
      ORDER BY COUNT(*) DESC
    `;
    const zonasResult = await client.query(zonasQuery);
    console.log('Resultado:', zonasResult.rows);

    // 2. Consulta de eventos por tipo de sensor
    console.log('\n🔍 Testing: Eventos por Tipo de Sensor');
    const sensoresQuery = `
      SELECT 
        CASE 
          WHEN payload_data->>'event_type' = 'panic.button' THEN 'Botón Pánico'
          WHEN payload_data->>'event_type' = 'sensor.lpr' THEN 'Sensor LPR'
          WHEN payload_data->>'event_type' = 'sensor.speed' THEN 'Sensor Velocidad'
          WHEN payload_data->>'event_type' = 'sensor.acoustic' THEN 'Sensor Acústico'
          WHEN payload_data->>'event_type' = 'citizen.report' THEN 'Reporte Ciudadano'
          ELSE 'Otros'
        END as "Tipo de Sensor",
        COUNT(*) as "Cantidad"
      FROM events 
      GROUP BY payload_data->>'event_type'
      ORDER BY COUNT(*) DESC
    `;
    const sensoresResult = await client.query(sensoresQuery);
    console.log('Resultado:', sensoresResult.rows);

    // 3. Series temporales - eventos por tipo
    console.log('\n🔍 Testing: Series Temporales - Eventos por Tipo');
    const timeSeriesQuery = `
      SELECT
        DATE_TRUNC('hour', ts_utc) as time,
        payload_data->>'event_type' as metric,
        COUNT(*) as value
      FROM events
      WHERE ts_utc >= NOW() - INTERVAL '24 hours'
      GROUP BY DATE_TRUNC('hour', ts_utc), payload_data->>'event_type'
      ORDER BY time DESC
      LIMIT 10
    `;
    const timeSeriesResult = await client.query(timeSeriesQuery);
    console.log('Resultado:', timeSeriesResult.rows);

  } catch (error) {
    console.error('❌ Error:', error.message);
  } finally {
    await client.end();
  }
}

testQueries();