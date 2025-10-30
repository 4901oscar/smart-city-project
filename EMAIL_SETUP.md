# 📧 Configuración de Email para Alertas

## ⚠️ IMPORTANTE: Configuración Requerida

Para que el sistema pueda enviar emails de alertas a **oscarrivera4901@gmail.com**, necesitas configurar una **Contraseña de Aplicación de Gmail**.

---

## 🔧 Pasos para Configurar Gmail

### 1. **Activa la Verificación en 2 Pasos**
   - Ve a: https://myaccount.google.com/security
   - Busca "Verificación en 2 pasos"
   - Actívala si aún no está habilitada

### 2. **Genera una Contraseña de Aplicación**
   - Ve a: https://myaccount.google.com/apppasswords
   - Selecciona "Correo" como tipo de app
   - Selecciona "Otro (nombre personalizado)" y escribe: **Smart City Alerts**
   - Haz clic en "Generar"
   - **Copia** la contraseña de 16 caracteres (sin espacios)

### 3. **Configura el archivo `.env`**
   Edita el archivo `.env` en la raíz del proyecto y completa:

   ```properties
   # Email (Gmail SMTP)
   EMAIL_SMTP_HOST=smtp.gmail.com
   EMAIL_SMTP_PORT=587
   EMAIL_FROM=smartcity.alerts@gmail.com
   EMAIL_USERNAME=tu-email-personal@gmail.com    # ← TU EMAIL
   EMAIL_PASSWORD=xxxx-xxxx-xxxx-xxxx            # ← CONTRASEÑA DE APLICACIÓN
   EMAIL_ALERT_RECIPIENT=oscarrivera4901@gmail.com
   ```

   **Ejemplo completo:**
   ```properties
   EMAIL_USERNAME=juanperez@gmail.com
   EMAIL_PASSWORD=abcd efgh ijkl mnop  # (sin espacios en realidad)
   ```

### 4. **Reconstruye el Backend**
   ```powershell
   docker-compose build backend
   docker-compose up -d backend
   ```

---

## 📬 Formato del Email de Alerta

Cuando una alerta sea despachada, recibirás un email con:

**Asunto:** 🚨 ALERTA DE [TIPO]

**Contenido:**
- **Tipo de Alerta**: DISPARO DETECTADO, INCENDIO REPORTADO, etc.
- **📍 Zona**: Zona 10, Centro Histórico, etc.
- **🕐 Hora**: Timestamp UTC de la alerta
- **🌍 Geolocalización**: Latitud, Longitud + link a Google Maps
- **🚔 Entidad Despachada**: Policía Nacional (PNC), Bomberos, Cruz Roja, etc.
- **📋 Detalles**: JSON con información adicional del evento

---

## 🧪 Prueba del Sistema

### Opción 1: Prueba Manual
```powershell
# Enviar evento de prueba
cd js-scripts
node producer.js
```

El producer generará eventos aleatorios, algunos activarán alertas críticas que serán despachadas por Airflow.

### Opción 2: Verificar Endpoint de Dispatch
```powershell
curl -X GET http://localhost:5000/dispatch/status
```

Deberías ver:
```json
{
  "service": "Smart City Dispatch System",
  "status": "operational",
  "emailRecipient": "oscarrivera4901@gmail.com",
  "entities": [...]
}
```

---

## 🔍 Verificar Logs de Email

Para ver si los emails se están enviando:

```powershell
docker-compose logs backend -f | grep -i email
```

Deberías ver mensajes como:
```
✓ Email enviado a oscarrivera4901@gmail.com - Alerta: DISPARO DETECTADO
```

---

## ⚡ Flujo Completo del Sistema

```
1. Producer genera eventos → Backend API → PostgreSQL
2. Consumer detecta patrones → Genera alertas → PostgreSQL
3. Airflow (cada 1 min) → Lee alertas → Llama a /dispatch/{entity}
4. DispatchController → Envía email a oscarrivera4901@gmail.com
5. Recibes email con toda la información de la alerta 📧
```

---

## 🚨 Solución de Problemas

### Error: "Authentication failed"
- ✅ Verifica que usaste una **Contraseña de Aplicación**, NO tu contraseña normal de Gmail
- ✅ Verifica que la verificación en 2 pasos esté activa
- ✅ Copia la contraseña sin espacios

### Error: "SMTP connection failed"
- ✅ Verifica que el puerto sea `587` (no 465 ni 25)
- ✅ Verifica que `EMAIL_SMTP_HOST=smtp.gmail.com`

### No recibo emails
- ✅ Verifica la carpeta de SPAM
- ✅ Verifica que `EMAIL_ALERT_RECIPIENT=oscarrivera4901@gmail.com` esté correcto
- ✅ Revisa los logs del backend: `docker-compose logs backend --tail 50`

---

## 📞 Soporte

Si tienes problemas, revisa:
1. Los logs del backend: `docker-compose logs backend -f`
2. El estado de Airflow: http://localhost:8090
3. El endpoint de health: http://localhost:5000/health

---

**¡Listo!** Una vez configurado, cada alerta crítica llegará automáticamente a **oscarrivera4901@gmail.com** 🎉
