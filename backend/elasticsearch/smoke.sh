#!/usr/bin/env bash
# Script de prueba para Elasticsearch + Kibana local
# Uso: chmod +x smoke.sh && ./smoke.sh
ES_URL="http://localhost:9200"
ES_USER="elastic"
ES_PASS="changeme123"

echo "1) Estado del cluster:"
curl -s -u $ES_USER:$ES_PASS "$ES_URL/_cluster/health?pretty"

echo -e "\n2) Creando plantilla (events_alerts_template):"
curl -s -u $ES_USER:$ES_PASS -H 'Content-Type: application/json' -XPUT "$ES_URL/_index_template/events_alerts_template" -d @events_alerts_template.json

echo -e "\n3) Creando pipeline (geo_make_location):"
curl -s -u $ES_USER:$ES_PASS -H 'Content-Type: application/json' -XPUT "$ES_URL/_ingest/pipeline/geo_make_location" -d @pipeline_geo.json

echo -e "\n4) Indexando documento de prueba:"
curl -s -u $ES_USER:$ES_PASS -H 'Content-Type: application/json' -XPOST "$ES_URL/events-000001/_doc?pipeline=geo_make_location" -d '{
  "event_id": "test-1",
  "event_type": "test.event",
  "timestamp": "2025-10-11T12:00:00Z",
  "geo": {"zone":"zone_1","lat":14.634915,"lon":-90.506882},
  "severity": "info",
  "payload": {"message":"prueba de indexacion"}
}'

echo -e "\n5) Buscando documento insertado:"
curl -s -u $ES_USER:$ES_PASS -H 'Content-Type: application/json' -XGET "$ES_URL/events-*/_search?pretty" -d '{
  "query": { "term": { "event_id": { "value": "test-1" } } }
}'

echo -e "\nPrueba completada."
