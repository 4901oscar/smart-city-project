using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SmartCityBackend.Services;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _username;
    private readonly string _password;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        _fromEmail = configuration["Email:FromEmail"] ?? "noreply@smartcity.com";
        _fromName = configuration["Email:FromName"] ?? "Smart City Alert System";
        _username = configuration["Email:Username"] ?? "";
        _password = configuration["Email:Password"] ?? "";
    }

    public async Task<bool> SendAlertEmailAsync(
        string toEmail,
        string alertType,
        string zone,
        DateTime timestamp,
        double? geoLat,
        double? geoLon,
        string targetEntity,
        string alertDetails)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = $"🚨 ALERTA DE {alertType.ToUpper()}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background: #f5f5f5; 
            padding: 20px; 
            line-height: 1.6; 
        }}
        .email-wrapper {{ 
            max-width: 650px; 
            margin: 0 auto; 
            background: white; 
            border-radius: 15px; 
            overflow: hidden; 
            box-shadow: 0 10px 30px rgba(0,0,0,0.15); 
        }}
        .header {{ 
            background: linear-gradient(135deg, #ff4444 0%, #cc0000 100%);
            color: white; 
            padding: 30px 25px; 
            text-align: center;
            position: relative;
            overflow: hidden;
        }}
        .header::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -50%;
            width: 200%;
            height: 200%;
            background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 70%);
        }}
        .header h1 {{ 
            font-size: 28px; 
            margin-bottom: 8px; 
            font-weight: 700;
            text-shadow: 0 2px 4px rgba(0,0,0,0.2);
            position: relative;
        }}
        .header p {{ 
            font-size: 14px; 
            opacity: 0.95; 
            position: relative;
        }}
        .alert-badge {{
            display: inline-block;
            background: rgba(255,255,255,0.25);
            padding: 8px 20px;
            border-radius: 25px;
            margin-top: 15px;
            font-weight: 600;
            font-size: 16px;
            border: 2px solid rgba(255,255,255,0.4);
        }}
        .content {{ 
            padding: 25px; 
        }}
        .info-card {{ 
            background: linear-gradient(to right, #f8f9fa 0%, #ffffff 100%);
            padding: 18px 20px; 
            margin-bottom: 15px; 
            border-radius: 10px;
            border-left: 5px solid #ff4444;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
            transition: transform 0.2s ease;
        }}
        .info-card:hover {{
            transform: translateX(3px);
        }}
        .info-row {{
            display: flex;
            align-items: center;
            margin: 10px 0;
        }}
        .icon {{
            font-size: 24px;
            margin-right: 12px;
            min-width: 30px;
            text-align: center;
        }}
        .label {{ 
            font-weight: 600; 
            color: #2c3e50;
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 5px;
            display: block;
        }}
        .value {{ 
            color: #34495e;
            font-size: 16px;
            font-weight: 500;
        }}
        .critical-alert {{
            background: linear-gradient(135deg, #ff4444 0%, #cc0000 100%);
            color: white;
            padding: 15px 20px;
            border-radius: 10px;
            text-align: center;
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 20px;
            box-shadow: 0 4px 15px rgba(255,68,68,0.3);
            letter-spacing: 1px;
        }}
        .map-button {{
            display: inline-block;
            background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%);
            color: white;
            padding: 12px 25px;
            border-radius: 25px;
            text-decoration: none;
            font-weight: 600;
            margin-top: 10px;
            box-shadow: 0 4px 12px rgba(76,175,80,0.3);
            transition: all 0.3s ease;
        }}
        .map-button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 18px rgba(76,175,80,0.4);
        }}
        .coordinates {{
            background: #2c3e50;
            color: #ecf0f1;
            padding: 12px 18px;
            border-radius: 8px;
            font-family: 'Courier New', monospace;
            font-size: 14px;
            margin: 10px 0;
            display: inline-block;
        }}
        .entity-badge {{
            background: linear-gradient(135deg, #3498db 0%, #2980b9 100%);
            color: white;
            padding: 10px 20px;
            border-radius: 20px;
            display: inline-block;
            font-weight: 600;
            font-size: 15px;
            margin-top: 5px;
            box-shadow: 0 3px 10px rgba(52,152,219,0.3);
        }}
        .details-box {{
            background: #fff9e6;
            border: 2px dashed #ffd700;
            padding: 15px;
            border-radius: 8px;
            margin-top: 10px;
            color: #8b7500;
            font-size: 14px;
            line-height: 1.8;
        }}
        .footer {{ 
            background: #2c3e50;
            color: #ecf0f1;
            padding: 20px; 
            text-align: center;
        }}
        .footer p:first-child {{
            font-size: 16px;
            font-weight: 600;
            margin-bottom: 8px;
        }}
        .footer p:last-child {{
            font-size: 12px;
            opacity: 0.8;
        }}
        .divider {{
            height: 2px;
            background: linear-gradient(to right, transparent, #ff4444, transparent);
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='header'>
            <h1>🚨 ALERTA DE SEGURIDAD</h1>
            <p>Sistema de Monitoreo Smart City</p>
            <div class='alert-badge'>{alertType.ToUpper()}</div>
        </div>
        
        <div class='content'>
            <div class='critical-alert'>
                ⚠️ REQUIERE ATENCIÓN INMEDIATA ⚠️
            </div>
            
            <div class='info-card'>
                <span class='label'>📍 UBICACIÓN</span>
                <div class='info-row'>
                    <span class='icon'>�️</span>
                    <span class='value'>Zona: <strong>{zone}</strong></span>
                </div>
                <div class='info-row'>
                    <span class='icon'>🕐</span>
                    <span class='value'>{timestamp:dd/MM/yyyy HH:mm:ss} UTC</span>
                </div>
            </div>
            
            <div class='info-card'>
                <span class='label'>🌍 COORDENADAS GPS</span>
                <div class='coordinates'>
                    📍 LAT: {geoLat ?? 0:F6} | LON: {geoLon ?? 0:F6}
                </div>
                <div style='text-align: center;'>
                    <a href='https://www.google.com/maps?q={geoLat ?? 0},{geoLon ?? 0}' 
                       class='map-button' target='_blank'>
                       📍 ABRIR EN GOOGLE MAPS
                    </a>
                </div>
            </div>
            
            <div class='info-card'>
                <span class='label'>🚔 ENTIDAD DESPACHADA</span>
                <div style='text-align: center; margin-top: 10px;'>
                    <span class='entity-badge'>{targetEntity}</span>
                </div>
            </div>
            
            <div class='divider'></div>
            
            <div class='info-card'>
                <span class='label'>📋 INFORMACIÓN ADICIONAL</span>
                <div class='details-box'>
                    {alertDetails}
                </div>
            </div>
        </div>
        
        <div class='footer'>
            <p>🏙️ Smart City Event Processing System</p>
            <p>Alerta generada automáticamente • Respuesta inmediata requerida</p>
        </div>
    </div>
</body>
</html>",
                TextBody = $@"
🚨 ALERTA DE SEGURIDAD - SMART CITY

TIPO DE ALERTA: {alertType}

📍 UBICACIÓN:
   Zona: {zone}
   Hora: {timestamp:yyyy-MM-dd HH:mm:ss} UTC

🌍 GEOLOCALIZACIÓN:
   Latitud: {geoLat ?? 0:F6}
   Longitud: {geoLon ?? 0:F6}
   Google Maps: https://www.google.com/maps?q={geoLat ?? 0},{geoLon ?? 0}

🚔 ENTIDAD DESPACHADA: {targetEntity}

📋 DETALLES:
{alertDetails}

---
Smart City Event Processing System
Alerta generada automáticamente
"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Conectar al servidor SMTP
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            
            // Autenticar (si es necesario)
            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                await client.AuthenticateAsync(_username, _password);
            }
            
            // Enviar email
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation($"✓ Email enviado a {toEmail} - Alerta: {alertType}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"✗ Error enviando email: {ex.Message}");
            return false;
        }
    }
}
