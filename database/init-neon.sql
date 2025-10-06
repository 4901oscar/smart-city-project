-- Script de inicialización de base de datos para Smart City
-- Ejecutar este script en Neon Console SQL Editor

-- Eliminar tablas si existen (solo para desarrollo/testing)
DROP TABLE IF EXISTS alerts CASCADE;
DROP TABLE IF EXISTS events CASCADE;

-- Crear tabla de eventos
CREATE TABLE events (
  event_id        uuid PRIMARY KEY,
  event_type      text NOT NULL,
  event_version   text NOT NULL,
  producer        text NOT NULL,
  source          text NOT NULL,
  correlation_id  uuid,
  trace_id        uuid,
  partition_key   text NOT NULL,
  ts_utc          timestamptz NOT NULL,
  zone            text,
  geo_lat         numeric,
  geo_lon         numeric,
  severity        text,
  payload         jsonb NOT NULL
);

-- Crear índices para eventos
CREATE INDEX idx_events_ts ON events (ts_utc);
CREATE INDEX idx_events_type ON events (event_type);
CREATE INDEX idx_events_zone ON events (zone);
CREATE INDEX idx_events_pkey ON events (partition_key);

-- Comentarios para documentación
COMMENT ON TABLE events IS 'Tabla principal de eventos del sistema Smart City';
COMMENT ON COLUMN events.event_id IS 'Identificador único del evento (UUID v4)';
COMMENT ON COLUMN events.event_type IS 'Tipo de evento: panic.button, sensor.lpr, sensor.speed, sensor.acoustic, citizen.report';
COMMENT ON COLUMN events.event_version IS 'Versión del esquema del evento (actualmente 1.0)';
COMMENT ON COLUMN events.producer IS 'Productor del evento: artillery, python-sim, kafka-cli';
COMMENT ON COLUMN events.source IS 'Fuente del evento (actualmente simulated)';
COMMENT ON COLUMN events.correlation_id IS 'UUID para correlacionar eventos relacionados';
COMMENT ON COLUMN events.trace_id IS 'UUID para trazabilidad distribuida';
COMMENT ON COLUMN events.partition_key IS 'Clave de partición para Kafka';
COMMENT ON COLUMN events.ts_utc IS 'Timestamp del evento en UTC';
COMMENT ON COLUMN events.zone IS 'Zona geográfica del evento';
COMMENT ON COLUMN events.geo_lat IS 'Latitud en grados decimales';
COMMENT ON COLUMN events.geo_lon IS 'Longitud en grados decimales';
COMMENT ON COLUMN events.severity IS 'Severidad del evento: info, warning, critical';
COMMENT ON COLUMN events.payload IS 'Payload específico del evento en formato JSON';

-- Crear tabla de alertas
CREATE TABLE alerts (
  alert_id        uuid PRIMARY KEY,
  correlation_id  uuid,
  type            text NOT NULL,
  score           numeric,
  zone            text,
  window_start    timestamptz,
  window_end      timestamptz,
  evidence        jsonb,
  created_at      timestamptz NOT NULL DEFAULT now()
);

-- Crear índices para alertas
CREATE INDEX idx_alerts_ts ON alerts (created_at);
CREATE INDEX idx_alerts_zone ON alerts (zone);
CREATE INDEX idx_alerts_type ON alerts (type);
CREATE INDEX idx_alerts_corr ON alerts (correlation_id);

-- Comentarios para alertas
COMMENT ON TABLE alerts IS 'Tabla de alertas agregadas generadas por el sistema';
COMMENT ON COLUMN alerts.alert_id IS 'Identificador único de la alerta';
COMMENT ON COLUMN alerts.correlation_id IS 'UUID para correlacionar con eventos';
COMMENT ON COLUMN alerts.type IS 'Tipo de alerta';
COMMENT ON COLUMN alerts.score IS 'Puntuación o nivel de confianza de la alerta';
COMMENT ON COLUMN alerts.zone IS 'Zona geográfica de la alerta';
COMMENT ON COLUMN alerts.window_start IS 'Inicio de la ventana temporal';
COMMENT ON COLUMN alerts.window_end IS 'Fin de la ventana temporal';
COMMENT ON COLUMN alerts.evidence IS 'Evidencia asociada (array de event_ids con contexto)';
COMMENT ON COLUMN alerts.created_at IS 'Timestamp de creación de la alerta';

-- Insertar datos de prueba (opcional)
INSERT INTO events (
  event_id, event_type, event_version, producer, source,
  correlation_id, trace_id, partition_key, ts_utc,
  zone, geo_lat, geo_lon, severity, payload
) VALUES (
  gen_random_uuid(),
  'panic.button',
  '1.0',
  'python-sim',
  'simulated',
  gen_random_uuid(),
  gen_random_uuid(),
  'panic_zona10',
  now(),
  'Zona 10',
  14.6091,
  -90.5252,
  'critical',
  '{"tipo_de_alerta": "panico", "identificador_dispositivo": "BTN-001", "user_context": "movil"}'::jsonb
);

-- Verificar que las tablas se crearon correctamente
SELECT 
  table_name,
  (SELECT count(*) FROM information_schema.columns WHERE table_name = t.table_name) as column_count
FROM information_schema.tables t
WHERE table_schema = 'public' 
  AND table_type = 'BASE TABLE'
ORDER BY table_name;

-- Verificar índices
SELECT
  schemaname,
  tablename,
  indexname,
  indexdef
FROM pg_indexes
WHERE schemaname = 'public'
ORDER BY tablename, indexname;

-- Verificar el evento de prueba
SELECT 
  event_id,
  event_type,
  zone,
  severity,
  ts_utc,
  payload->>'tipo_de_alerta' as tipo_alerta
FROM events
ORDER BY ts_utc DESC
LIMIT 5;
