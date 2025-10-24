const { Client } = require('pg');

const pgConfig = {
  host: 'arqui-pg.postgres.database.azure.com',
  database: 'test_events',
  user: 'grupo2',
  password: '4rqu1.4pp',
  port: 5432,
  ssl: { rejectUnauthorized: false }
};

async function testConnection() {
  const client = new Client(pgConfig);
  
  try {
    console.log('🔌 Conectando a Azure PostgreSQL (test_events)...');
    await client.connect();
    console.log('✅ Conectado exitosamente\n');

    // Verificar tablas
    console.log('📋 Tablas existentes:');
    const tables = await client.query(`
      SELECT table_name 
      FROM information_schema.tables 
      WHERE table_schema = 'public' 
      AND table_type = 'BASE TABLE'
      ORDER BY table_name
    `);
    
    if (tables.rows.length === 0) {
      console.log('⚠️  NO HAY TABLAS');
      console.log('\n📝 Necesitas ejecutar database/init-neon.sql en Azure Portal');
    } else {
      console.log(tables.rows);

      // Contar registros
      console.log('\n📊 Conteo de registros:');
      for (const table of tables.rows) {
        const count = await client.query(`SELECT COUNT(*) FROM ${table.table_name}`);
        console.log(`  ${table.table_name}: ${count.rows[0].count} registros`);
      }
    }

  } catch (error) {
    console.error('❌ Error:', error.message);
  } finally {
    await client.end();
  }
}

testConnection();
