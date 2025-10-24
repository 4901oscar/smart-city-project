from datetime import datetime, timedelta
from airflow import DAG
from airflow.operators.python import PythonOperator
import json
import requests
import logging

# Configuración
BACKEND_URL = 'http://backend:8080'

# Mapeo de tipos de alerta a entidades
ALERT_ROUTING = {
    'EXCESO DE VELOCIDAD': ['policia-transito'],
    'EXCESO DE VELOCIDAD PELIGROSO': ['policia-transito', 'policia-nacional'],
    'INCENDIO REPORTADO': ['bomberos'],
    'INCENDIO REPORTADO POR CIUDADANO': ['bomberos'],
    'ACCIDENTE REPORTADO': ['bomberos-voluntarios', 'cruz-roja'],
    'ALTERCADO REPORTADO': ['policia-municipal'],
    'DISPARO DETECTADO': ['policia-nacional'],
    'EXPLOSIÓN DETECTADA': ['policia-nacional', 'bomberos'],
    'VIDRIO ROTO DETECTADO': ['policia-nacional'],
    'RUIDO EXCESIVO': ['policia-municipal'],
    'CONTAMINACIÓN ACÚSTICA EXTREMA': ['policia-municipal'],
    'EMERGENCIA PERSONAL': ['policia-nacional', 'cruz-roja'],
    'EMERGENCIA GENERAL': ['policia-municipal'],
    'VELOCIDAD PELIGROSA RADAR': ['policia-transito'],
    'VELOCIDAD SOBRE LÍMITE': ['policia-transito'],
    'REGISTRO VEHICULAR': ['policia-transito'],
    'ANOMALÍA DE TRÁFICO': ['policia-transito']
}

def classify_alert(alert_data):
    """Clasifica la alerta y determina las entidades destino"""
    alert_type = alert_data.get('type')
    
    # Buscar coincidencia exacta primero
    entities = ALERT_ROUTING.get(alert_type, []).copy()
    
    # Si no hay coincidencia exacta, buscar coincidencia parcial
    if not entities:
        for routing_type, routing_entities in ALERT_ROUTING.items():
            if routing_type in alert_type or alert_type in routing_type:
                entities.extend(routing_entities)
                break
    
    # Remover duplicados
    entities = list(set(entities))
    
    # Fallback: si no se encuentra ninguna entidad, usar policía municipal
    return entities if entities else ['policia-municipal']

def dispatch_to_entity(entity, alert_data):
    """Despacha una alerta a una entidad específica"""
    try:
        # Transformar el formato de la alerta para el endpoint de dispatch
        dispatch_payload = {
            "alert_id": alert_data.get("alertId"),
            "correlation_id": alert_data.get("correlationId"),
            "type": alert_data.get("type"),
            "score": alert_data.get("score"),
            "zone": alert_data.get("zone"),
            "window_start": alert_data.get("windowStart"),
            "details": alert_data.get("details", {})
        }
        
        url = f"{BACKEND_URL}/dispatch/{entity}"
        response = requests.post(url, json=dispatch_payload, timeout=5)
        logging.info(f"✓ Despachado a {entity}: {response.status_code}")
        return {'entity': entity, 'success': True, 'status': response.status_code}
    except Exception as e:
        logging.error(f"✗ Error despachando a {entity}: {str(e)}")
        return {'entity': entity, 'success': False, 'error': str(e)}

def fetch_alerts_from_backend(**context):
    """Obtiene alertas pendientes del backend"""
    try:
        # Obtener las últimas 10 alertas del backend
        response = requests.get(f"{BACKEND_URL}/alerts?take=10", timeout=10)
        alerts = response.json()
        
        logging.info(f"Obtenidas {len(alerts)} alertas del backend")
        
        # Guardar en XCom para el siguiente task
        context['task_instance'].xcom_push(key='alerts', value=alerts)
        return len(alerts)
    except Exception as e:
        logging.error(f"Error obteniendo alertas: {str(e)}")
        return 0

def process_and_dispatch_alerts(**context):
    """Procesa y despacha las alertas obtenidas"""
    # Obtener alertas del XCom
    alerts = context['task_instance'].xcom_pull(key='alerts', task_ids='fetch_alerts')
    
    if not alerts:
        logging.info("No hay alertas para procesar")
        return
    
    total_dispatched = 0
    
    for alert_data in alerts:
        logging.info(f"Procesando alerta: {alert_data.get('alert_id', 'unknown')}")
        
        # Clasificar y obtener entidades destino
        target_entities = classify_alert(alert_data)
        logging.info(f"Entidades destino: {', '.join(target_entities)}")
        
        # Despachar a cada entidad
        for entity in target_entities:
            result = dispatch_to_entity(entity, alert_data)
            if result['success']:
                total_dispatched += 1
    
    logging.info(f"Total de despachos exitosos: {total_dispatched}")
    return total_dispatched

# Definir el DAG
default_args = {
    'owner': 'smart-city',
    'depends_on_past': False,
    'start_date': datetime(2025, 10, 23),
    'email_on_failure': False,
    'email_on_retry': False,
    'retries': 1,
    'retry_delay': timedelta(minutes=1),
}

dag = DAG(
    'alert_dispatch_pipeline',
    default_args=default_args,
    description='Pipeline de despacho de alertas a entidades de emergencia',
    schedule_interval=timedelta(minutes=1),  # Cada minuto
    catchup=False,
    tags=['alerts', 'dispatch', 'emergency']
)

# Task 1: Obtener alertas del backend
fetch_task = PythonOperator(
    task_id='fetch_alerts',
    python_callable=fetch_alerts_from_backend,
    provide_context=True,
    dag=dag
)

# Task 2: Procesar y despachar alertas
dispatch_task = PythonOperator(
    task_id='dispatch_alerts',
    python_callable=process_and_dispatch_alerts,
    provide_context=True,
    dag=dag
)

# Definir dependencias
fetch_task >> dispatch_task
