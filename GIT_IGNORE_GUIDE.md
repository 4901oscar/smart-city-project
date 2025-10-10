# 📦 Guía de Control de Versiones - .gitignore

## ✅ Archivos que SÍ se suben al repositorio

### Código Fuente
```
✅ js-scripts/producer.js
✅ js-scripts/consumer.js
✅ js-scripts/alert-monitor.js
✅ js-scripts/dlq-monitor.js
✅ backend/**/*.cs (todos los archivos C#)
✅ backend/Schemas/**/*.json (validación de eventos)
```

### Configuración de Dependencias
```
✅ js-scripts/package.json          # npm install usa este archivo
✅ backend/backend.csproj            # dotnet restore usa este archivo
```

### Docker
```
✅ docker-compose.yml                # Orquestación de servicios
✅ backend/Dockerfile                # Build de imagen del backend
```

### Base de Datos
```
✅ database/init-neon.sql            # Schema inicial
```

### Documentación
```
✅ *.md (todos los archivos Markdown)
✅ README.md
✅ TESTING.md
✅ COMPLIANCE.md
✅ ALERTS_PERSISTENCE.md
etc.
```

### Configuración (sin secretos)
```
✅ backend/appsettings.json          # Configuración general
✅ .env (si no tiene secretos)       # Variables de entorno
```

---

## ❌ Archivos que NO se suben (generados automáticamente)

### Node.js
```
❌ js-scripts/node_modules/          # npm install genera esto
❌ package-lock.json                 # Auto-generado por npm
```

### .NET
```
❌ backend/bin/                      # dotnet build genera esto
❌ backend/obj/                      # dotnet build genera esto
❌ *.dll                             # Binarios compilados
❌ *.exe                             # Ejecutables
❌ *.pdb                             # Debug symbols
```

### Docker
```
❌ docker-data/                      # Volúmenes persistentes
❌ *.log                             # Logs de contenedores
```

### IDEs
```
❌ .vscode/                          # Configuración local de VS Code
❌ .vs/                              # Configuración de Visual Studio
❌ .idea/                            # Configuración de JetBrains
```

### Sistema Operativo
```
❌ .DS_Store                         # macOS
❌ Thumbs.db                         # Windows
❌ Desktop.ini                       # Windows
```

---

## 🔄 Flujo de Trabajo

### Al clonar el repositorio

```bash
# 1. Clonar
git clone https://github.com/4901oscar/smart-city-project.git
cd smart-city-project

# 2. Instalar dependencias Node.js
cd js-scripts
npm install                          # Crea node_modules/

# 3. Restaurar dependencias .NET (si trabajas con backend local)
cd ../backend
dotnet restore                       # Descarga NuGet packages

# 4. Iniciar infraestructura
cd ..
docker compose up -d                 # Build + start
```

### Al hacer cambios

```bash
# Solo agregar código fuente y configuración
git add js-scripts/producer.js
git add backend/Controllers/AlertsController.cs
git add package.json

# NO agregar dependencias
# git add node_modules/  ← ❌ NO HACER ESTO
# git add bin/           ← ❌ NO HACER ESTO
```

---

## 📊 Tamaño del Repositorio

### Sin .gitignore (incluyendo dependencias)
```
node_modules/          ~150 MB
backend/bin/           ~50 MB
backend/obj/           ~30 MB
Total:                 ~230 MB+ 😱
```

### Con .gitignore (solo código fuente)
```
Código fuente JS:      ~50 KB
Código fuente .NET:    ~100 KB
Schemas JSON:          ~10 KB
Documentación:         ~500 KB
Docker configs:        ~5 KB
Total:                 ~665 KB ✅
```

**Reducción:** ~99.7% más liviano

---

## 🔍 Verificar qué se subirá

### Ver archivos trackeados
```bash
git status
```

### Ver qué se ignora
```bash
# Verificar si un archivo específico está ignorado
git check-ignore -v node_modules/axios
git check-ignore -v backend/bin/Debug

# Listar todos los archivos ignorados
git status --ignored
```

### Limpiar archivos ignorados (testing)
```bash
# Ver qué se eliminaría (dry run)
git clean -ndX

# Eliminar archivos ignorados (CUIDADO)
git clean -fdX
```

---

## 🚨 Archivos Sensibles

### NUNCA subir
```
❌ appsettings.Production.json       # Secrets de producción
❌ .env.production                   # Variables secretas
❌ *.key                             # Llaves privadas
❌ *.pem                             # Certificados
❌ secrets/                          # Carpeta de secretos
```

### Alternativa: Variables de Entorno
```bash
# En lugar de subir secrets, documentar qué variables necesitan:

# .env.example (SÍ subir)
BACKEND_URL=http://localhost:5000
KAFKA_BROKERS=localhost:9092

# .env (NO subir)
BACKEND_URL=http://localhost:5000
DB_PASSWORD=super_secret_password_123  ← Secret real
```

---

## 📝 Checklist antes de Commit

- [ ] ¿Agregaste solo archivos de código fuente?
- [ ] ¿No incluiste `node_modules/`?
- [ ] ¿No incluiste `bin/` o `obj/`?
- [ ] ¿No incluiste archivos `.dll` o `.exe`?
- [ ] ¿No incluiste secretos o passwords?
- [ ] ¿Incluiste `package.json` si agregaste dependencias?
- [ ] ¿Incluiste `.csproj` si agregaste NuGet packages?
- [ ] ¿Documentaste cambios en README si es necesario?

---

## 🔧 Comandos Útiles

### Ver tamaño del repositorio
```bash
git count-objects -vH
```

### Ver archivos más grandes
```bash
git ls-files | xargs du -h | sort -rh | head -20
```

### Remover archivo ya commiteado
```bash
# Si subiste node_modules/ por error:
git rm -r --cached node_modules/
git commit -m "Remove node_modules from git"
git push
```

### Limpiar historial (si ya subiste archivos grandes)
```bash
# CUIDADO: Reescribe historia
git filter-branch --tree-filter 'rm -rf node_modules' HEAD
```

---

## 📦 Estructura Recomendada del Commit

```
smart-city-project/
├── .gitignore                    ✅ Subir
├── README.md                     ✅ Subir
├── docker-compose.yml            ✅ Subir
├── package.json                  ✅ Subir (raíz si es monorepo)
│
├── js-scripts/
│   ├── package.json              ✅ Subir
│   ├── producer.js               ✅ Subir
│   ├── consumer.js               ✅ Subir
│   ├── alert-monitor.js          ✅ Subir
│   ├── dlq-monitor.js            ✅ Subir
│   └── node_modules/             ❌ NO subir (generado)
│
├── backend/
│   ├── backend.csproj            ✅ Subir
│   ├── Program.cs                ✅ Subir
│   ├── Dockerfile                ✅ Subir
│   ├── Controllers/              ✅ Subir todos los .cs
│   ├── Models/                   ✅ Subir todos los .cs
│   ├── Services/                 ✅ Subir todos los .cs
│   ├── Schemas/                  ✅ Subir todos los .json
│   ├── bin/                      ❌ NO subir (generado)
│   └── obj/                      ❌ NO subir (generado)
│
└── database/
    └── init-neon.sql             ✅ Subir
```

---

## 🎯 Resumen

### ¿Qué subir?
✅ **Código fuente** (.js, .cs)  
✅ **Configuración de dependencias** (package.json, .csproj)  
✅ **Schemas y configs** (sin secretos)  
✅ **Documentación** (.md)  
✅ **Docker configs** (docker-compose.yml, Dockerfile)  
✅ **Scripts SQL** (init-neon.sql)  

### ¿Qué NO subir?
❌ **Dependencias** (node_modules/, NuGet packages)  
❌ **Binarios compilados** (bin/, obj/, .dll, .exe)  
❌ **Configuración de IDE** (.vscode/, .vs/, .idea/)  
❌ **Logs** (*.log)  
❌ **Archivos de sistema** (.DS_Store, Thumbs.db)  
❌ **Secretos** (passwords, API keys)  

### ¿Por qué?
- ✅ **Repo liviano** (KB en vez de MB)
- ✅ **Clones rápidos**
- ✅ **Sin conflictos** de dependencias entre desarrolladores
- ✅ **Seguridad** (sin secretos expuestos)
- ✅ **Cross-platform** (cada dev instala lo que necesita)

---

**Creado:** 9 de octubre de 2025  
**Propósito:** Mantener el repositorio limpio y eficiente  
**Beneficio:** Repositorio ~99% más liviano
