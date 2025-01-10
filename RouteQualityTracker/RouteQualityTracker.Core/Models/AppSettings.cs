using Microsoft.Extensions.Logging;

namespace RouteQualityTracker.Core.Models;

public class AppSettings
{
    public LogLevel LogLevel { get; set; }

    public bool SendEmail { get; set; }

    public bool SendSms { get; set; }

    public string SmsNumber { get; set; }

    public string MailTo { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string SmtpServer { get; set; }

    public int SmtpPort { get; set; }

    public bool UseHeadset { get; set; }

    public bool UseMediaControls { get; set; }

    public bool UseCustomDevice { get; set; }

    public bool ImportDataFromFile { get; set; }

    public bool ImportFromStrava { get; set; }

    public string StravaApiCode { get; set; }

    public const string StravaClientId = "123539";
}
