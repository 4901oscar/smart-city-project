# Kibana Persistence Guide - Smart City Project

## 📋 Resumen
Este documento explica cómo persistir las configuraciones de Kibana (Data Views, Dashboards, Visualizaciones, Saved Objects) para evitar perderlas al reiniciar los contenedores.

---

## 🎯 Problema
Por defecto, si eliminas los contenedores de Docker (`docker compose down -v`), pierdes todas las configuraciones manuales de Kibana:
- ❌ Data Views
- ❌ Dashboards
- ❌ Visualizaciones
- ❌ Saved Searches
- ❌ Index Patterns

---

## ✅ Soluciones Implementadas

### **1. Volumen Persistente de Kibana** (CONFIGURADO)

El `docker-compose.yml` ahora incluye un volumen para Kibana que persiste los datos:

```yaml
kibana:
  image: docker.elastic.co/kibana/kibana:8.10.3
  volumes:
    - kibana_data:/usr/share/kibana/data  # ← Persistencia automática
  # ...

volumes:
  kibana_data:
    driver: "local"
```

**Ventajas**:
- ✅ Persistencia automática de TODOS los Saved Objects
- ✅ No requiere intervención manual después de la primera configuración
- ✅ Sobrevive a `docker compose down` (sin `-v`)

**Limitaciones**:
- ❌ Se pierde con `docker compose down -v` (porque borra volumes)
- ❌ No versionable en Git

---

### **2. Script de Inicialización Automática** (RECOMENDADO)

Script PowerShell que crea Data Views automáticamente vía API de Kibana:

```powershell
# Crear Data Views automáticamente
.\scripts\init-kibana-dataviews.ps1
```

**Data Views creados**:
1. **Smart City Events** → Index pattern: `events*`
   - Time field: `timestamp`
   - ID: `events-dataview`

2. **Smart City Alerts** → Index pattern: `alerts*`
   - Time field: `@timestamp`
   - ID: `alerts-dataview`

**Ventajas**:
- ✅ Idempotente (se puede ejecutar múltiples veces)
- ✅ Versionable en Git
- ✅ Automatizable en CI/CD
- ✅ Funciona después de `docker compose down -v`

**Cuándo ejecutar**:
```powershell
# Después de levantar el sistema desde cero
docker compose up -d
.\scripts\init-elasticsearch.ps1    # 1. Crear índices
.\scripts\init-kibana-dataviews.ps1 # 2. Crear Data Views
```

---

### **3. Export/Import de Saved Objects** (MANUAL)

Kibana permite exportar/importar configuraciones manualmente.

#### **Exportar configuraciones actuales**:

1. En Kibana UI, ve a: **Stack Management → Saved Objects**
2. Selecciona los objetos que quieres exportar
3. Click en **Export** → Descarga archivo `.ndjson`
4. Guarda el archivo en `kibana/saved-objects/`

**O usando la API**:
```powershell
# Exportar todos los Data Views
Invoke-RestMethod -Uri "http://localhost:5601/api/saved_objects/_export" `
    -Method POST `
    -Headers @{"kbn-xsrf"="true"; "Content-Type"="application/json"} `
    -Body '{"type":"index-pattern"}' `
    -OutFile "kibana/saved-objects/dataviews-backup.ndjson"
```

#### **Importar configuraciones**:

```powershell
# Importar desde archivo
$file = Get-Content "kibana/saved-objects/dataviews.ndjson" -Raw
Invoke-RestMethod -Uri "http://localhost:5601/api/saved_objects/_import" `
    -Method POST `
    -Headers @{"kbn-xsrf"="true"} `
    -Form @{file=$file}
```

**Ventajas**:
- ✅ Backup completo de configuraciones
- ✅ Versionable en Git
- ✅ Portátil entre entornos

**Limitaciones**:
- ❌ Proceso manual
- ❌ Requiere re-importar después de cada reset

---

## 🚀 Workflow Recomendado

### **Inicio Limpio (Primera vez)**
```powershell
# 1. Levantar infraestructura
docker compose up -d

# 2. Inicializar Elasticsearch
.\scripts\init-elasticsearch.ps1

# 3. Crear Data Views automáticamente
.\scripts\init-kibana-dataviews.ps1

# 4. Iniciar consumer y producer
cd js-scripts
npm run consumer  # Terminal 1
npm run producer  # Terminal 2
```

### **Reinicio Completo (Borrando volúmenes)**
```powershell
# 1. Detener y limpiar todo
docker compose down -v

# 2. Levantar de nuevo
docker compose up -d

# 3. Re-inicializar (scripts son idempotentes)
.\scripts\init-elasticsearch.ps1
.\scripts\init-kibana-dataviews.ps1
```

### **Reinicio Normal (Conservando datos)**
```powershell
# 1. Detener SIN borrar volúmenes
docker compose down

# 2. Levantar de nuevo
docker compose up -d

# ✅ Data Views se conservan automáticamente (volumen kibana_data)
```

---

## 📁 Archivos Relacionados

| Archivo | Propósito |
|---------|-----------|
| `scripts/init-kibana-dataviews.ps1` | Script para crear Data Views automáticamente |
| `kibana/saved-objects/dataviews.ndjson` | Backup manual de Data Views (ejemplo) |
| `docker-compose.yml` | Configuración de volumen `kibana_data` |

---

## 🔍 Verificación

### **Verificar que los Data Views existen**:
```powershell
# Listar Data Views vía API
Invoke-RestMethod -Uri "http://localhost:5601/api/data_views" `
    -Headers @{"kbn-xsrf"="true"}
```

### **Verificar volumen persistente**:
```powershell
# Listar volúmenes de Docker
docker volume ls | Select-String "kibana"

# Debería mostrar: smart-city-project_kibana_data
```

### **Verificar datos en Kibana UI**:
1. Abre http://localhost:5601
2. Ve a **Management → Data Views**
3. Deberías ver:
   - ✅ Smart City Events (events*)
   - ✅ Smart City Alerts (alerts*)

---

## 🛠️ Troubleshooting

### **Problema: Data Views no aparecen después de `docker compose down -v`**

**Causa**: El flag `-v` borra TODOS los volúmenes, incluyendo `kibana_data`.

**Solución**:
```powershell
# Re-ejecutar el script de inicialización
.\scripts\init-kibana-dataviews.ps1
```

---

### **Problema: Script falla con "Kibana no disponible"**

**Causa**: Kibana tarda ~30-60 segundos en estar listo.

**Solución**:
```powershell
# Esperar más tiempo y volver a intentar
Start-Sleep -Seconds 30
.\scripts\init-kibana-dataviews.ps1
```

---

### **Problema: Error 409 "Data View ya existe"**

**Causa**: El Data View ya fue creado anteriormente.

**Solución**: ✅ **No es un error**, el script es idempotente y lo detecta automáticamente.

---

## 📊 Comparación de Métodos

| Método | Automatizable | Versionable | Sobrevive a `down` | Sobrevive a `down -v` |
|--------|---------------|-------------|--------------------|-----------------------|
| Volumen (`kibana_data`) | ✅ | ❌ | ✅ | ❌ |
| Script de inicialización | ✅ | ✅ | ✅ | ✅ (re-ejecutar) |
| Export/Import manual | ❌ | ✅ | ✅ | ❌ (re-importar) |

**Recomendación**: Usar **Script de inicialización** + **Volumen persistente** para máxima resiliencia.

---

## 🎯 Best Practices

1. **Siempre usa `docker compose down` sin `-v`** a menos que quieras empezar desde cero
2. **Ejecuta `init-kibana-dataviews.ps1` después de cada reset completo**
3. **Exporta Saved Objects importantes** antes de hacer cambios grandes
4. **Versiona los scripts de inicialización** en Git (ya hecho)
5. **Documenta dashboards personalizados** con exports en `kibana/saved-objects/`

---

## 🔗 Referencias

- [Kibana Data Views API](https://www.elastic.co/guide/en/kibana/current/data-views-api.html)
- [Kibana Saved Objects API](https://www.elastic.co/guide/en/kibana/current/saved-objects-api.html)
- [Docker Volumes](https://docs.docker.com/storage/volumes/)
