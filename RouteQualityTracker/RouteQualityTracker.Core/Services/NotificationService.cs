using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using System.Net;
using System.Net.Mail;

namespace RouteQualityTracker.Core.Services;

public class NotificationService : INotificationService
{
    private readonly NotificationSettings _notificationSettings;
    private readonly IServiceManager _serviceManager;

    public NotificationService(NotificationSettings notificationSettings, IServiceManager serviceManager)
    {
        _notificationSettings = notificationSettings;
        _serviceManager = serviceManager;
    }

    public bool IsSendEmailEnabled => _notificationSettings.SendEmail;

    public async Task SendEmail(RouteQualityEnum routeQuality)
    {
        using var smtpClient = new SmtpClient(_notificationSettings.SmtpServer, _notificationSettings.SmtpPort);
        smtpClient.EnableSsl = true;
        smtpClient.Credentials = new NetworkCredential(_notificationSettings.Username, _notificationSettings.Password);

        var mail = new MailMessage(_notificationSettings.Username, _notificationSettings.MailTo)
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
