import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class EventSimulationService {
  generatePanicButtonEvent() {
    return {
      timestamp: new Date().toISOString(),
      zona_geografica: 'Zona 10', // O coordenadas: { lat: 14.6091 + Math.random() * 0.01, lng: -90.5252 + Math.random() * 0.01 }
      tipo_de_alerta: 'panico', // Random: ['panico', 'emergencia',(Math.random() * 3)]
      identificador_del_dispositivo_simulado: 'device-' + Math.random().toString(36).substring(7),
      user_context: 'app_movil'
    };
  }

  // Para LPR (Semana 2)
  generateLprEvent() {
    return {
      placa_vehicular: 'XYZ' + Math.floor(Math.random() * 1000),
      ubicacion_sensor: 'Zona 10',
      velocidad_estimada: Math.floor(Math.random() * 100) + 50, // 50-150 km/h
      timestamp: new Date().toISOString(),
      modelo_color_vehiculo: 'Rojo/Sedan'
    };
  }

  // Similar para acoustic y citizen reports (agrega random para sound/decibels, etc.)
}