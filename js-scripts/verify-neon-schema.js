// Verificar esquema de Neon Cloud PostgreSQL
const { Client } = require('pg');

const client = new Client({
  host: 'ep-solitary-waterfall-adymysgs-pooler.c-2.us-east-1.aws.neon.tech',
  database: 'SmartCitiesBD',
  user: 'neondb_owner',
  password: 'npg_c1UK0FbwvrER',
  port: 5432,
  ssl: { rejectUnauthorized: false }
});

async function checkSchema() {
  try {
    await client.connect();
    console.log('✅ Conectado a Neon Cloud PostgreSQL\n');

    // Verificar tablas
    console.log('📋 Tablas existentes:');
    const tables = await client.query(`
      SELECT table_name 
      FROM information_schema.tables 
      WHERE table_schema = 'public' 
      AND table_type = 'BASE TABLE'
      ORDER BY table_name
    `);
    console.table(tables.rows);

    if (tables.rows.length === 0) {
      console.log('\n⚠️  NO HAY TABLAS - Ejecuta database/init-neon.sql');
      await client.end();
      return;
    }

    // Estructura de tabla events
    console.log('\n🏗️  Estructura de tabla EVENTS:');
    const eventsColumns = await client.query(`
      SELECT column_name, data_type, is_nullable
      FROM information_schema.columns 
      WHERE table_name = 'events'
      ORDER BY ordinal_position
    `);
    console.table(eventsColumns.rows);

    // Estructura de tabla alerts
    console.log('\n🏗️  Estructura de tabla ALERTS:');
    const alertsColumns = await client.query(`
      SELECT column_name, data_type, is_nullable
      FROM information_schema.columns 
      WHERE table_name = 'alerts'
      ORDER BY ordinal_position
    `);
    console.table(alertsColumns.rows);

    // Contar registros
    console.log('\n📊 Conteo de registros:');
    const eventsCount = await client.query('SELECT COUNT(*) FROM events');
    const alertsCount = await client.query('SELECT COUNT(*) FROM alerts');
    console.log(`  events: ${eventsCount.rows[0].count}`);
    console.log(`  alerts: ${alertsCount.rows[0].count}`);

    await client.end();
  } catch (error) {
    console.error('❌ Error:', error.message);
    process.exit(1);
  }
}

checkSchema();
