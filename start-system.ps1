# Script de inicio completo del sistema Smart City
# Levanta Docker Compose e inicializa Elasticsearch y Kibana

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "SMART CITY - INICIO COMPLETO DEL SISTEMA" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan

# 1. Levantar servicios con Docker Compose
Write-Host "`n[1/3] Levantando servicios con Docker Compose..." -ForegroundColor Yellow
docker-compose up -d

# 2. Esperar a que los servicios estén disponibles
Write-Host "`n[2/3] Esperando a que los servicios inicien (30 segundos)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# 3. Ejecutar inicialización
Write-Host "`n[3/3] Ejecutando inicialización de Elasticsearch y Kibana..." -ForegroundColor Yellow
& "$PSScriptRoot\scripts\init-all.ps1"

Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "SISTEMA LISTO PARA USAR" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Cyan
