Sistema de Monitoreo Perimetral Inteligente
Proyecto académico para simular sensores urbanos con Kafka, PostgreSQL y APIs en .NET.
Configuración Rápida

Copia .env con tus creds de DB.
Instala dependencias:
Backend: cd backend && dotnet restore
JS: cd js-scripts && npm install


Levanta con Docker: docker-compose up -d
Prueba APIs: Postman a http://localhost:5000/events
Ejecuta scripts: cd js-scripts && npm run producer (envía eventos), npm run consumer (lee alertas).

Pruebas con Postman

POST /events: Envía JSON del envelope (ver Schemas).
GET /health: Verifica estado.

Consejos

Para alertas: Consumer simula correlación en Zona 10.
Docker: Usa para ingeniero—docker-compose up levanta todo.
Listo hoy: Sí, con Docker. Prueba producer para generar eventos.

Fecha: 28 de septiembre de 2025.