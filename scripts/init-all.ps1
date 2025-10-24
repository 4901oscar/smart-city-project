# Script maestro para inicializar todo el sistema Smart City
# Ejecuta en secuencia: Elasticsearch -> Kibana Data Views

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "SMART CITY - INICIALIZACION COMPLETA DEL SISTEMA" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan

# Verificar que Docker esta corriendo
Write-Host "`n[1/4] Verificando Docker..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "OK - Docker esta corriendo" -ForegroundColor Green
} catch {
    Write-Host "ERROR - Docker no esta corriendo o no esta instalado" -ForegroundColor Red
    exit 1
}

# Verificar que los servicios estan levantados
Write-Host "`n[2/4] Verificando servicios..." -ForegroundColor Yellow
$services = docker ps --format "{{.Names}}" | Select-String -Pattern "kafka|elasticsearch|kibana|backend"
if ($services.Count -lt 4) {
    Write-Host "AVISO - Algunos servicios no estan corriendo" -ForegroundColor Yellow
    Write-Host "Levantando servicios con docker compose..." -ForegroundColor Yellow
    docker compose up -d
    Write-Host "Esperando 30 segundos para que los servicios inicien..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
} else {
    Write-Host "OK - Servicios principales detectados" -ForegroundColor Green
}

# Inicializar Elasticsearch
Write-Host "`n[3/4] Inicializando Elasticsearch..." -ForegroundColor Yellow
try {
    & "$PSScriptRoot\init-elasticsearch.ps1"
    Write-Host "OK - Elasticsearch inicializado correctamente" -ForegroundColor Green
} catch {
    Write-Host "ERROR - Fallo al inicializar Elasticsearch: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Inicializar Kibana Data Views
Write-Host "`n[4/4] Inicializando Kibana Data Views..." -ForegroundColor Yellow
try {
    & "$PSScriptRoot\init-kibana-dataviews.ps1"
    Write-Host "OK - Kibana Data Views creados correctamente" -ForegroundColor Green
} catch {
    Write-Host "ERROR - Fallo al crear Kibana Data Views: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Resumen final
Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "SISTEMA INICIALIZADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Cyan

Write-Host "`nServicios disponibles:" -ForegroundColor White
Write-Host "  - Backend API:       http://localhost:5000" -ForegroundColor White
Write-Host "  - Kafka UI:          http://localhost:8080" -ForegroundColor White
Write-Host "  - Elasticsearch:     http://localhost:9200" -ForegroundColor White
Write-Host "  - Kibana:            http://localhost:5601" -ForegroundColor White
Write-Host "  - Grafana:           http://localhost:3000" -ForegroundColor White
Write-Host "  - Prometheus:        http://localhost:9090" -ForegroundColor White

Write-Host "`nProximos pasos:" -ForegroundColor Cyan
Write-Host "  1. Iniciar consumer:  cd js-scripts && npm run consumer" -ForegroundColor White
Write-Host "  2. Iniciar producer:  cd js-scripts && npm run producer" -ForegroundColor White
Write-Host "  3. Abrir Kibana y explorar los Data Views creados" -ForegroundColor White
Write-Host "  4. Ver dashboards en Grafana (credenciales: admin/admin)" -ForegroundColor White

Write-Host "`nDocumentacion:" -ForegroundColor Cyan
Write-Host "  - ELASTICSEARCH_GUIDE.md" -ForegroundColor White
Write-Host "  - KIBANA_PERSISTENCE.md" -ForegroundColor White
Write-Host "  - GRAFANA_KAFKA_GUIDE.md" -ForegroundColor White
