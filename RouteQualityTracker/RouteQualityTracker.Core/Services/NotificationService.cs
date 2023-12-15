using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using System.Net;
using System.Net.Mail;

namespace RouteQualityTracker.Core.Services;

public class NotificationService : INotificationService
{
    private readonly AppSettings _appSettings;
    private readonly IServiceManager _serviceManager;

    public NotificationService(AppSettings appSettings, IServiceManager serviceManager)
    {
        _appSettings = appSettings;
        _serviceManager = serviceManager;
    }

    public bool IsSendEmailEnabled => _appSettings.SendEmail;

    public async Task SendEmail(RouteQualityEnum routeQuality)
    {
        using var smtpClient = new SmtpClient(_appSettings.SmtpServer, _appSettings.SmtpPort);
        smtpClient.EnableSsl = true;
        smtpClient.Credentials = new NetworkCredential(_appSettings.Username, _appSettings.Password);

        var mail = new MailMessage(_appSettings.Username, _appSettings.MailTo)
        {
            Subject = $"Route quality: {routeQuality}",
            Body = string.Empty
        };

        try
        {
            await smtpClient.SendMailAsync(mail);
        }
        catch(Exception ex)
        {
            _serviceManager.SetStatus(false, ex);
        }
    }
}
