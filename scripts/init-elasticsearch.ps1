# Script de inicializacion de Elasticsearch para Smart City
Write-Host "Inicializando Elasticsearch..." -ForegroundColor Cyan

# Esperar a que Elasticsearch este listo
Write-Host "Esperando a que Elasticsearch este disponible..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$esReady = $false

while (-not $esReady -and $attempt -lt $maxAttempts) {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:9200/_cluster/health" -Method GET -ErrorAction Stop
        if ($response.status -eq "green" -or $response.status -eq "yellow") {
            $esReady = $true
            Write-Host "Elasticsearch esta listo (status: $($response.status))" -ForegroundColor Green
        }
    }
    catch {
        $attempt++
        Write-Host "Intento $attempt/$maxAttempts - Esperando..." -ForegroundColor Yellow
        Start-Sleep -Seconds 2
    }
}

if (-not $esReady) {
    Write-Host "Elasticsearch no esta disponible despues de $maxAttempts intentos" -ForegroundColor Red
    exit 1
}

# Crear indice para eventos
Write-Host "`nCreando indice events..." -ForegroundColor Cyan

$eventsMapping = @{
    mappings = @{
        properties = @{
            event_id = @{ type = "keyword" }
            event_type = @{ type = "keyword" }
            event_version = @{ type = "keyword" }
            producer = @{ type = "keyword" }
            source = @{ type = "keyword" }
            timestamp = @{ type = "date"; format = "strict_date_optional_time||epoch_millis" }
            partition_key = @{ type = "keyword" }
            severity = @{ type = "keyword" }
            geo = @{
                properties = @{
                    zone = @{ type = "keyword" }
                    lat = @{ type = "float" }
                    lon = @{ type = "float" }
                    location = @{ type = "geo_point" }
                }
            }
            payload = @{ type = "object"; enabled = $true }
        }
    }
    settings = @{
        number_of_shards = 1
        number_of_replicas = 0
        "index.refresh_interval" = "5s"
    }
} | ConvertTo-Json -Depth 10

try {
    Invoke-RestMethod -Uri "http://localhost:9200/events" -Method PUT -Body $eventsMapping -ContentType "application/json" -ErrorAction Stop
    Write-Host "Indice events creado exitosamente" -ForegroundColor Green
}
catch {
    if ($_.Exception.Message -like "*resource_already_exists_exception*") {
        Write-Host "Indice events ya existe, continuando..." -ForegroundColor Yellow
    }
    else {
        Write-Host "Error creando indice events: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Crear indice para alertas
Write-Host "`nCreando indice alerts..." -ForegroundColor Cyan

$alertsMapping = @{
    mappings = @{
        properties = @{
            alert_id = @{ type = "keyword" }
            correlation_id = @{ type = "keyword" }
            source_event_id = @{ type = "keyword" }
            event_type = @{ type = "keyword" }
            zone = @{ type = "keyword" }
            timestamp = @{ type = "date" }
            "@timestamp" = @{ type = "date" }
            coordinates = @{
                properties = @{
                    lat = @{ type = "float" }
                    lon = @{ type = "float" }
                }
            }
            geo_location = @{ type = "geo_point" }
            alerts = @{ type = "nested" }
        }
    }
    settings = @{
        number_of_shards = 1
        number_of_replicas = 0
        "index.refresh_interval" = "5s"
    }
} | ConvertTo-Json -Depth 10

try {
    Invoke-RestMethod -Uri "http://localhost:9200/alerts" -Method PUT -Body $alertsMapping -ContentType "application/json" -ErrorAction Stop
    Write-Host "Indice alerts creado exitosamente" -ForegroundColor Green
}
catch {
    if ($_.Exception.Message -like "*resource_already_exists_exception*") {
        Write-Host "Indice alerts ya existe, continuando..." -ForegroundColor Yellow
    }
    else {
        Write-Host "Error creando indice alerts: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Verificar indices creados
Write-Host "`nVerificando indices creados:" -ForegroundColor Cyan
try {
    $indices = Invoke-RestMethod -Uri "http://localhost:9200/_cat/indices?v" -Method GET
    Write-Host $indices -ForegroundColor Gray
}
catch {
    Write-Host "No se pudo listar indices" -ForegroundColor Yellow
}

Write-Host "`nInicializacion de Elasticsearch completada" -ForegroundColor Green
Write-Host "`nURLs importantes:" -ForegroundColor Cyan
Write-Host "  - Elasticsearch: http://localhost:9200/" -ForegroundColor White
Write-Host "  - Kibana: http://localhost:5601/" -ForegroundColor White
