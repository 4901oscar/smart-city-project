import { Component, AfterViewInit } from '@angular/core';
import * as L from 'leaflet';

@Component({
  selector: 'app-map',
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css']
})
export class MapComponent implements AfterViewInit {

  private map!: L.Map;

  ngAfterViewInit(): void {
    this.map = L.map('map').setView([14.6091, -90.5252], 15); // Zona 10, Guatemala

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap',
      maxZoom: 19
    }).addTo(this.map);


    // Iconos personalizados (coloca tus imágenes en public/ o src/assets/)
    const panicIcon = L.icon({
      iconUrl: 'assets/icons/panic-button.png',
      iconSize: [32, 32],
      iconAnchor: [16, 32],
      popupAnchor: [0, -32]
    });
    const lprIcon = L.icon({
      iconUrl: 'assets/icons/lpr-camera.png',
      iconSize: [32, 32],
      iconAnchor: [16, 32],
      popupAnchor: [0, -32]
    });
    const speedIcon = L.icon({
      iconUrl: 'assets/icons/speed-motion-sensor.png',
      iconSize: [32, 32],
      iconAnchor: [16, 32],
      popupAnchor: [0, -32]
    });
    const acousticIcon = L.icon({
      iconUrl: 'assets/icons/acoustic-sensor.png',
      iconSize: [32, 32],
      iconAnchor: [16, 32],
      popupAnchor: [0, -32]
    });
    const citizenIcon = L.icon({
      iconUrl: 'assets/icons/citizen-man.png',
      iconSize: [32, 32],
      iconAnchor: [16, 32],
      popupAnchor: [0, -32]
    });

    // Coordenadas y formularios
    const sensors = [
      {
        type: 'Panic Button',
        coords: [14.6100, -90.5250],
        form: `<form><b>Panic Button</b><br>Ubicación: Zona 10<br>Descripción:<br><input type='text' name='desc'><br><button type='submit'>Guardar</button></form>`,
        icon: panicIcon
      },
      {
        type: 'LPR Camera',
        coords: [14.6085, -90.5260],
        form: `<form><b>LPR Camera</b><br>Ubicación: Zona 10<br>Placa:<br><input type='text' name='placa'><br><button type='submit'>Guardar</button></form>`,
        icon: lprIcon
      },
      {
        type: 'Speed Motion Sensor',
        coords: [14.6095, -90.5240],
        form: `<form><b>Speed Motion Sensor</b><br>Ubicación: Zona 10<br>Velocidad:<br><input type='number' name='velocidad'><br><button type='submit'>Guardar</button></form>`,
        icon: speedIcon
      },
      {
        type: 'Acoustic Sensor',
        coords: [14.6105, -90.5265],
        form: `<form><b>Acoustic Sensor</b><br>Ubicación: Zona 10<br>Nivel de ruido:<br><input type='number' name='ruido'><br><button type='submit'>Guardar</button></form>`,
        icon: acousticIcon
      },
      {
        type: 'Citizen Report',
        coords: [14.6080, -90.5255],
        form: `<form><b>Citizen Report</b><br>Ubicación: Zona 10<br>Reporte:<br><input type='text' name='reporte'><br><button type='submit'>Guardar</button></form>`,
        icon: citizenIcon
      }
    ];

    sensors.forEach(sensor => {
      L.marker(sensor.coords as [number, number], { icon: sensor.icon })
        .addTo(this.map)
        .bindPopup(sensor.form);
    });

   
    setTimeout(() => this.map.invalidateSize(), 0);
  }
}
