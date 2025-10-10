# 🚀 Script de Inicio Rápido - Smart City

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Smart City - Inicialización del Sistema" -ForegroundColor Cyan  
Write-Host "═══════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# 1. Verificar que Docker esté corriendo
Write-Host "[1/5] Verificando Docker..." -ForegroundColor Yellow
$dockerStatus = docker ps 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Docker no está corriendo. Inicia Docker Desktop primero.`n" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Docker está corriendo`n" -ForegroundColor Green

# 2. Iniciar infraestructura
Write-Host "[2/5] Iniciando infraestructura (Kafka, Zookeeper, Backend)..." -ForegroundColor Yellow
docker compose up -d
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Infraestructura iniciada`n" -ForegroundColor Green
} else {
    Write-Host "✗ Error iniciando infraestructura`n" -ForegroundColor Red
    exit 1
}

# 3. Esperar a que el backend esté listo
Write-Host "[3/5] Esperando a que el backend esté listo (10 segundos)..." -ForegroundColor Yellow
Start-Sleep -Seconds 10
Write-Host "✓ Backend listo`n" -ForegroundColor Green

# 4. Verificar topics de Kafka
Write-Host "[4/5] Verificando topics de Kafka..." -ForegroundColor Yellow
$topics = docker exec kafka kafka-topics --bootstrap-server localhost:9092 --list 2>&1
if ($topics -match "events.standardized" -and $topics -match "correlated.alerts" -and $topics -match "events.dlq") {
    Write-Host "✓ Topics de Kafka configurados correctamente:" -ForegroundColor Green
    Write-Host "  • events.standardized" -ForegroundColor Cyan
    Write-Host "  • correlated.alerts" -ForegroundColor Cyan
    Write-Host "  • events.dlq`n" -ForegroundColor Cyan
} else {
    Write-Host "⚠ Algunos topics no están creados. Ejecuta los comandos de creación.`n" -ForegroundColor Yellow
}

# 5. Verificar endpoint de alertas
Write-Host "[5/5] Verificando endpoint de alertas..." -ForegroundColor Yellow
try {
    $stats = Invoke-RestMethod -Uri "http://localhost:5000/alerts/stats" -TimeoutSec 5
    Write-Host "✓ Endpoint /alerts funcionando correctamente" -ForegroundColor Green
    Write-Host "  Total alertas en BD: $($stats.total)`n" -ForegroundColor Cyan
} catch {
    Write-Host "⚠ El endpoint de alertas no responde. El backend puede estar iniciando.`n" -ForegroundColor Yellow
}

# Resumen
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Sistema Listo - Próximos Pasos" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "📊 OPCIÓN 1: Modo Completo (4 terminales)" -ForegroundColor Green
Write-Host "   Terminal 1: cd js-scripts; npm run producer" -ForegroundColor White
Write-Host "   Terminal 2: cd js-scripts; npm run consumer" -ForegroundColor White
Write-Host "   Terminal 3: cd js-scripts; npm run alert-monitor" -ForegroundColor White
Write-Host "   Terminal 4: cd js-scripts; npm run dlq-monitor`n" -ForegroundColor White

Write-Host "🔧 OPCIÓN 2: Modo Básico (2 terminales)" -ForegroundColor Green
Write-Host "   Terminal 1: cd js-scripts; npm run producer" -ForegroundColor White
Write-Host "   Terminal 2: cd js-scripts; npm run consumer`n" -ForegroundColor White

Write-Host "📈 VERIFICAR ALERTAS:" -ForegroundColor Green
Write-Host "   Ver estadísticas: Invoke-RestMethod http://localhost:5000/alerts/stats" -ForegroundColor White
Write-Host "   Ver últimas 10:   Invoke-RestMethod http://localhost:5000/alerts?take=10`n" -ForegroundColor White

Write-Host "═══════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan
