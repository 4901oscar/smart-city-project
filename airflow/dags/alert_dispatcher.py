"""
DAG de Apache Airflow para despachar alertas a entidades correspondientes.

Este DAG consume alertas del topic 'correlated.alerts' de Kafka y las envia
a las entidades apropiadas segun el tipo de emergencia:
- Exceso de velocidad -> Policia de Transito
- Incendio -> Bomberos
- Accidente -> Bomberos Voluntarios
- Robo/Violencia -> Policia Nacional
- Emergencias medicas -> Cruz Roja
"""

from datetime import datetime, timedelta
from airflow import DAG
from airflow.operators.python import PythonOperator
from airflow.providers.apache.kafka.sensors.kafka import AwaitMessageSensor
import json
import requests
import logging

# Configuracion de logging
logger = logging.getLogger(__name__)

# Configuracion de entidades (simuladas con webhooks)
ENTITY_ENDPOINTS = {
    'policia_transito': 'http://backend:8080/dispatch/policia-transito',
    'bomberos': 'http://backend:8080/dispatch/bomberos',
    'bomberos_voluntarios': 'http://backend:8080/dispatch/bomberos-voluntarios',
    'policia_nacional': 'http://backend:8080/dispatch/policia-nacional',
    'cruz_roja': 'http://backend:8080/dispatch/cruz-roja',
    'policia_municipal': 'http://backend:8080/dispatch/policia-municipal'
}

# Mapeo de tipos de alertas a entidades
ALERT_ROUTING = {
    'EXCESO DE VELOCIDAD': ['policia_transito'],
    'EXCESO DE VELOCIDAD PELIGROSO': ['policia_transito', 'policia_nacional'],
    'VELOCIDAD EXCESIVA DETECTADA': ['policia_transito'],
    'VELOCIDAD SOBRE LÍMITE': ['policia_transito'],
    
    'INCENDIO REPORTADO': ['bomberos'],
    'INCENDIO REPORTADO POR CIUDADANO': ['bomberos'],
    
    'ACCIDENTE REPORTADO': ['bomberos_voluntarios', 'cruz_roja'],
    
    'DISPARO DETECTADO': ['policia_nacional'],
    'EXPLOSIÓN DETECTADA': ['policia_nacional', 'bomberos'],
    'VIDRIO ROTO DETECTADO': ['policia_nacional'],
    
    'EMERGENCIA PERSONAL': ['policia_nacional', 'cruz_roja'],
    'EMERGENCIA GENERAL': ['policia_municipal'],
    
    'ALTERCADO REPORTADO': ['policia_municipal'],
    'EVENTO CRÍTICO': ['policia_nacional'],
    
    'REGISTRO VEHICULAR': [],  # Solo informativo, no requiere despacho
    'CONTAMINACIÓN ACÚSTICA EXTREMA': []  # Solo informativo
}

def classify_alert(alert_data):
    """
    Clasifica la alerta y determina a que entidades debe enviarse.
    
    Args:
        alert_data: Diccionario con los datos de la alerta desde Kafka
        
    Returns:
        Lista de entidades a las que debe enviarse la alerta
    """
    entities = set()
    
    if 'alerts' not in alert_data:
        logger.warning(f"Alerta sin campo 'alerts': {alert_data.get('alert_id')}")
        return []
    
    for alert in alert_data['alerts']:
        alert_type = alert.get('type', '')
        level = alert.get('level', '')
        
        # Obtener entidades segun el tipo
        if alert_type in ALERT_ROUTING:
            entities.update(ALERT_ROUTING[alert_type])
        
        # Alertas CRÍTICAS siempre van a Policia Nacional
        if level == 'CRÍTICO' and 'policia_nacional' not in entities:
            entities.add('policia_nacional')
    
    return list(entities)

def dispatch_to_entity(entity_name, alert_data, **context):
    """
    Envia la alerta a una entidad especifica.
    
    Args:
        entity_name: Nombre de la entidad (ej: 'bomberos')
        alert_data: Datos completos de la alerta
        context: Contexto de Airflow
    """
    if entity_name not in ENTITY_ENDPOINTS:
        logger.error(f"Entidad desconocida: {entity_name}")
        return
    
    endpoint = ENTITY_ENDPOINTS[entity_name]
    
    # Preparar payload para la entidad
    payload = {
        'alert_id': alert_data.get('alert_id'),
        'timestamp': alert_data.get('timestamp'),
        'zone': alert_data.get('zone'),
        'coordinates': alert_data.get('coordinates'),
        'event_type': alert_data.get('event_type'),
        'alerts': alert_data.get('alerts', []),
        'dispatched_by': 'airflow-smart-city',
        'dispatched_at': datetime.utcnow().isoformat()
    }
    
    try:
        # Simular envio a la entidad
        # En produccion, esto seria una llamada HTTP real a la API de la entidad
        logger.info(f"Despachando alerta {alert_data.get('alert_id')} a {entity_name}")
        logger.info(f"Endpoint: {endpoint}")
        logger.info(f"Payload: {json.dumps(payload, indent=2)}")
        
        # Descomentar para envios reales:
        # response = requests.post(endpoint, json=payload, timeout=10)
        # response.raise_for_status()
        # logger.info(f"Respuesta de {entity_name}: {response.status_code}")
        
        # Por ahora, guardar en logs
        context['task_instance'].xcom_push(
            key=f'dispatch_{entity_name}',
            value={
                'status': 'success',
                'entity': entity_name,
                'alert_id': alert_data.get('alert_id'),
                'timestamp': datetime.utcnow().isoformat()
            }
        )
        
    except Exception as e:
        logger.error(f"Error despachando a {entity_name}: {str(e)}")
        raise

def process_alert(**context):
    """
    Procesa una alerta del topic de Kafka y la clasifica.
    
    Args:
        context: Contexto de Airflow con el mensaje de Kafka
    """
    # Obtener mensaje de Kafka del sensor
    message = context['task_instance'].xcom_pull(task_ids='wait_for_alert')
    
    if not message:
        logger.warning("No se recibio mensaje de Kafka")
        return
    
    try:
        # Parsear mensaje JSON
        alert_data = json.loads(message.value().decode('utf-8'))
        
        logger.info(f"Procesando alerta: {alert_data.get('alert_id')}")
        logger.info(f"Zona: {alert_data.get('zone')}")
        logger.info(f"Tipo de evento: {alert_data.get('event_type')}")
        
        # Clasificar alerta
        entities = classify_alert(alert_data)
        
        if not entities:
            logger.info(f"Alerta {alert_data.get('alert_id')} es solo informativa, no requiere despacho")
            return
        
        logger.info(f"Alerta debe despacharse a: {', '.join(entities)}")
        
        # Guardar en XCom para tasks posteriores
        context['task_instance'].xcom_push(key='alert_data', value=alert_data)
        context['task_instance'].xcom_push(key='target_entities', value=entities)
        
    except Exception as e:
        logger.error(f"Error procesando alerta: {str(e)}")
        raise

def dispatch_alerts(**context):
    """
    Despacha la alerta a todas las entidades correspondientes.
    
    Args:
        context: Contexto de Airflow
    """
    alert_data = context['task_instance'].xcom_pull(task_ids='process_alert', key='alert_data')
    entities = context['task_instance'].xcom_pull(task_ids='process_alert', key='target_entities')
    
    if not alert_data or not entities:
        logger.warning("No hay datos de alerta o entidades para despachar")
        return
    
    dispatch_summary = {
        'alert_id': alert_data.get('alert_id'),
        'dispatched_to': entities,
        'timestamp': datetime.utcnow().isoformat()
    }
    
    # Despachar a cada entidad
    for entity in entities:
        dispatch_to_entity(entity, alert_data, **context)
    
    logger.info(f"Alerta {alert_data.get('alert_id')} despachada a {len(entities)} entidad(es)")
    
    # Guardar resumen
    context['task_instance'].xcom_push(key='dispatch_summary', value=dispatch_summary)

# Definir argumentos por defecto del DAG
default_args = {
    'owner': 'smart-city',
    'depends_on_past': False,
    'start_date': datetime(2025, 10, 23),
    'email_on_failure': False,
    'email_on_retry': False,
    'retries': 3,
    'retry_delay': timedelta(minutes=1),
}

# Crear DAG
dag = DAG(
    'alert_dispatcher',
    default_args=default_args,
    description='Despacha alertas a entidades correspondientes segun tipo de emergencia',
    schedule_interval=timedelta(seconds=30),  # Revisar cada 30 segundos
    catchup=False,
    tags=['smart-city', 'alerts', 'dispatch'],
)

# Task 1: Esperar por alertas en Kafka
# Nota: Este sensor requiere configuracion adicional en produccion
# Por ahora, usamos PythonOperator con logica personalizada

def consume_kafka_alerts(**context):
    """
    Consume alertas del topic correlated.alerts de Kafka.
    Esta es una version simplificada para demo.
    """
    from kafka import KafkaConsumer
    import time
    
    try:
        consumer = KafkaConsumer(
            'correlated.alerts',
            bootstrap_servers=['kafka:29092'],
            auto_offset_reset='latest',
            enable_auto_commit=True,
            group_id='airflow-dispatcher',
            value_deserializer=lambda x: json.loads(x.decode('utf-8')),
            consumer_timeout_ms=5000  # Timeout de 5 segundos
        )
        
        messages = []
        start_time = time.time()
        
        # Consumir mensajes durante 5 segundos
        for message in consumer:
            messages.append(message.value)
            
            # Procesar solo el primer mensaje
            if len(messages) >= 1:
                break
        
        consumer.close()
        
        if messages:
            logger.info(f"Consumidos {len(messages)} mensaje(s) de Kafka")
            # Guardar primer mensaje para procesamiento
            context['task_instance'].xcom_push(key='kafka_message', value=messages[0])
            return messages[0]
        else:
            logger.info("No hay mensajes nuevos en Kafka")
            return None
            
    except Exception as e:
        logger.error(f"Error consumiendo de Kafka: {str(e)}")
        return None

wait_for_alert = PythonOperator(
    task_id='wait_for_alert',
    python_callable=consume_kafka_alerts,
    dag=dag,
)

# Task 2: Procesar y clasificar alerta
process_alert_task = PythonOperator(
    task_id='process_alert',
    python_callable=process_alert,
    provide_context=True,
    dag=dag,
)

# Task 3: Despachar alerta a entidades
dispatch_alerts_task = PythonOperator(
    task_id='dispatch_alerts',
    python_callable=dispatch_alerts,
    provide_context=True,
    dag=dag,
)

# Definir dependencias
wait_for_alert >> process_alert_task >> dispatch_alerts_task
