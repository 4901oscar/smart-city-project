// Verificar esquema de la tabla events en Azure
const { Client } = require('pg');

const client = new Client({
  host: 'arqui-pg.postgres.database.azure.com',
  database: 'test_events',
  user: 'grupo2',
  password: '4rqu1.4pp',
  port: 5432,
  ssl: { rejectUnauthorized: false }
});

async function checkSchema() {
  await client.connect();
  
  console.log('📋 Estructura de tabla EVENTS:');
  const columns = await client.query(`
    SELECT column_name, data_type, is_nullable
    FROM information_schema.columns 
    WHERE table_name = 'events'
    ORDER BY ordinal_position
  `);
  console.table(columns.rows);
  
  console.log('\n📋 Estructura de tabla ALERTS:');
  const alertColumns = await client.query(`
    SELECT column_name, data_type, is_nullable
    FROM information_schema.columns 
    WHERE table_name = 'alerts'
    ORDER BY ordinal_position
  `);
  console.table(alertColumns.rows);
  
  await client.end();
}

checkSchema().catch(console.error);
