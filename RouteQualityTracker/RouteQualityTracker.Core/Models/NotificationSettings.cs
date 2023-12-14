using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteQualityTracker.Core.Models;

public class NotificationSettings
{
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
}
