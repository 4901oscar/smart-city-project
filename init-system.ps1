# ═══════════════════════════════════════════════════════════════
# Script de Inicialización Automática del Sistema Smart City
# ═══════════════════════════════════════════════════════════════
# Ejecuta: docker-compose up + inicialización de Kibana
# ═══════════════════════════════════════════════════════════════

Write-Host "`n═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SMART CITY - INICIALIZACIÓN COMPLETA DEL SISTEMA" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# [PASO 1] Verificar Docker
Write-Host "[1/5] Verificando Docker..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "✓ Docker está corriendo`n" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker no está corriendo. Inicia Docker Desktop primero.`n" -ForegroundColor Red
    exit 1
}

# [PASO 2] Levantar infraestructura
Write-Host "[2/5] Iniciando infraestructura (Docker Compose)..." -ForegroundColor Yellow
docker-compose up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Contenedores iniciados`n" -ForegroundColor Green
} else {
    Write-Host "✗ Error iniciando contenedores`n" -ForegroundColor Red
    exit 1
}

# [PASO 3] Esperar a que servicios estén listos
Write-Host "[3/5] Esperando a que servicios estén listos..." -ForegroundColor Yellow
Write-Host "   • Backend (10 segundos)" -ForegroundColor Gray
Start-Sleep -Seconds 10

# Verificar si Kafka está corriendo (problema común)
$kafkaStatus = docker ps --filter "name=kafka" --format "{{.Status}}" | Select-String "Up"
if (-not $kafkaStatus) {
    Write-Host "   ⚠ Kafka no está corriendo, reiniciando..." -ForegroundColor Yellow
    docker start smart-city-project-kafka-1 | Out-Null
    Start-Sleep -Seconds 10
}

Write-Host "   • Elasticsearch (15 segundos)" -ForegroundColor Gray
Start-Sleep -Seconds 15
Write-Host "   • Kibana (20 segundos)" -ForegroundColor Gray
Start-Sleep -Seconds 20

Write-Host "✓ Servicios inicializados`n" -ForegroundColor Green

# [PASO 4] Crear topics de Kafka
Write-Host "[4/5] Creando topics de Kafka..." -ForegroundColor Yellow
$topicsExist = docker exec kafka kafka-topics --bootstrap-server localhost:9092 --list 2>&1 | Select-String "events.standardized"

if (-not $topicsExist) {
    Write-Host "   • Creando events.standardized..." -ForegroundColor Gray
    docker exec kafka kafka-topics --create --bootstrap-server localhost:9092 --topic events.standardized --partitions 3 --replication-factor 1 2>&1 | Out-Null
    
    Write-Host "   • Creando correlated.alerts..." -ForegroundColor Gray
    docker exec kafka kafka-topics --create --bootstrap-server localhost:9092 --topic correlated.alerts --partitions 2 --replication-factor 1 2>&1 | Out-Null
    
    Write-Host "   • Creando events.dlq..." -ForegroundColor Gray
    docker exec kafka kafka-topics --create --bootstrap-server localhost:9092 --topic events.dlq --partitions 1 --replication-factor 1 2>&1 | Out-Null
    
    Write-Host "✓ Topics creados`n" -ForegroundColor Green
} else {
    Write-Host "✓ Topics ya existen`n" -ForegroundColor Green
}

# [PASO 5] Inicializar Kibana Data Views
Write-Host "[5/5] Inicializando Kibana Data Views..." -ForegroundColor Yellow
try {
    & "$PSScriptRoot\scripts\init-kibana-dataviews.ps1"
    Write-Host "✓ Kibana Data Views creados`n" -ForegroundColor Green
} catch {
    Write-Host "⚠ Error al crear Data Views (puedes ejecutar manualmente: .\scripts\init-kibana-dataviews.ps1)`n" -ForegroundColor Yellow
}

# [RESUMEN]
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ✅ SISTEMA LISTO PARA USAR" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "🌐 ENDPOINTS DISPONIBLES:" -ForegroundColor Cyan
Write-Host "   • Backend API:     http://localhost:5000 (Swagger)" -ForegroundColor White
Write-Host "   • Kafka UI:        http://localhost:8080" -ForegroundColor White
Write-Host "   • Airflow:         http://localhost:8090 (admin/admin)" -ForegroundColor White
Write-Host "   • Elasticsearch:   http://localhost:9200" -ForegroundColor White
Write-Host "   • Kibana:          http://localhost:5601 ← Data Views creados aquí" -ForegroundColor White
Write-Host "   • Grafana:         http://localhost:3000 (admin/admin)" -ForegroundColor White
Write-Host "   • Prometheus:      http://localhost:9090`n" -ForegroundColor White

Write-Host "📊 PRÓXIMOS PASOS:" -ForegroundColor Cyan
Write-Host "   1. Terminal 1: cd js-scripts; npm run producer" -ForegroundColor White
Write-Host "   2. Terminal 2: cd js-scripts; npm run consumer" -ForegroundColor White
Write-Host "   3. Abrir Kibana → Discover → Seleccionar 'Smart City Alerts'" -ForegroundColor White
Write-Host "   4. Airflow ejecutará DAG cada 1 minuto automáticamente`n" -ForegroundColor White

Write-Host "═══════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan
