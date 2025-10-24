#!/bin/bash
# Script de inicialización de Kibana Data Views (ejecutado dentro de un contenedor)

echo "Esperando a que Kibana esté disponible..."
until curl -s http://kibana:5601/api/status | grep -q "available"; do
  echo "Kibana no está listo, esperando..."
  sleep 5
done

echo "✓ Kibana está listo, creando Data Views..."

# Crear Data View para eventos
echo "Creando Data View: Smart City Events..."
curl -X POST "http://kibana:5601/api/data_views/data_view" \
  -H "kbn-xsrf: true" \
  -H "Content-Type: application/json" \
  -d '{
    "data_view": {
      "id": "events-dataview",
      "title": "events*",
      "timeFieldName": "timestamp",
      "name": "Smart City Events"
    }
  }' 2>/dev/null

echo ""
echo "Creando Data View: Smart City Alerts..."
curl -X POST "http://kibana:5601/api/data_views/data_view" \
  -H "kbn-xsrf: true" \
  -H "Content-Type: application/json" \
  -d '{
    "data_view": {
      "id": "alerts-dataview",
      "title": "alerts*",
      "timeFieldName": "created_at",
      "name": "Smart City Alerts"
    }
  }' 2>/dev/null

echo ""
echo "✓ Data Views creados exitosamente"
