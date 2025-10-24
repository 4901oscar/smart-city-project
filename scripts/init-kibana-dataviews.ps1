# Script para inicializar Data Views en Kibana
# Ejecutar despues de que Kibana este disponible

$KIBANA_URL = "http://localhost:5601"
$MAX_RETRIES = 30
$RETRY_DELAY = 2

Write-Host "Esperando a que Kibana este disponible..." -ForegroundColor Yellow

# Esperar a que Kibana este listo
$retries = 0
while ($retries -lt $MAX_RETRIES) {
    try {
        $response = Invoke-RestMethod -Uri "$KIBANA_URL/api/status" -Method GET -ErrorAction Stop
        if ($response.status.overall.level -eq "available") {
            Write-Host "OK - Kibana esta listo" -ForegroundColor Green
            break
        }
    } catch {
        $retries++
        Write-Host "Intento $retries/$MAX_RETRIES - Esperando Kibana..." -ForegroundColor Yellow
        Start-Sleep -Seconds $RETRY_DELAY
    }
}

if ($retries -eq $MAX_RETRIES) {
    Write-Host "ERROR: Kibana no respondio despues de $MAX_RETRIES intentos" -ForegroundColor Red
    exit 1
}

Write-Host "`nCreando Data Views en Kibana..." -ForegroundColor Cyan

# Funcion para crear Data View
function Create-DataView {
    param(
        [string]$Id,
        [string]$Title,
        [string]$TimeFieldName,
        [string]$Name
    )
    
    $body = @{
        data_view = @{
            id = $Id
            title = $Title
            timeFieldName = $TimeFieldName
            name = $Name
        }
    } | ConvertTo-Json -Depth 5

    try {
        $headers = @{
            "kbn-xsrf" = "true"
            "Content-Type" = "application/json"
        }
        
        $response = Invoke-RestMethod -Uri "$KIBANA_URL/api/data_views/data_view" `
            -Method POST `
            -Headers $headers `
            -Body $body `
            -ErrorAction Stop
        
        Write-Host "OK - Data View '$Name' creado exitosamente (ID: $Id)" -ForegroundColor Green
        return $true
    } catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
            Write-Host "AVISO - Data View '$Name' ya existe" -ForegroundColor Yellow
            return $true
        } else {
            Write-Host "ERROR - Al crear Data View '$Name': $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
}

# Crear Data View para eventos
Write-Host "`n1. Creando Data View para eventos..." -ForegroundColor Cyan
$eventsCreated = Create-DataView `
    -Id "events-dataview" `
    -Title "events*" `
    -TimeFieldName "timestamp" `
    -Name "Smart City Events"

# Crear Data View para alertas
Write-Host "`n2. Creando Data View para alertas..." -ForegroundColor Cyan
$alertsCreated = Create-DataView `
    -Id "alerts-dataview" `
    -Title "alerts*" `
    -TimeFieldName "@timestamp" `
    -Name "Smart City Alerts"

# Resumen
Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "RESUMEN DE INICIALIZACION DE KIBANA" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan

if ($eventsCreated -and $alertsCreated) {
    Write-Host "OK - Todos los Data Views fueron creados exitosamente" -ForegroundColor Green
    Write-Host "`nAccede a Kibana en: $KIBANA_URL" -ForegroundColor White
    Write-Host "  - Data View Smart City Events -> Index: events*" -ForegroundColor White
    Write-Host "  - Data View Smart City Alerts -> Index: alerts*" -ForegroundColor White
} else {
    Write-Host "AVISO - Algunos Data Views no pudieron ser creados" -ForegroundColor Yellow
}

Write-Host "`nPara ver tus datos:" -ForegroundColor Cyan
Write-Host "  1. Ve a Discover en Kibana" -ForegroundColor White
Write-Host "  2. Selecciona Smart City Events o Smart City Alerts" -ForegroundColor White
Write-Host "  3. Ajusta el rango de tiempo (Last 15 minutes)" -ForegroundColor White
