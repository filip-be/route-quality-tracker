using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Services;

public class SettingsService(AppSettings settings) : ISettingsService
{
    private bool _initialized;

    public AppSettings Settings
    {
        get
        {
            if (!_initialized)
            {
                InitializeSettings();
                _initialized = true;
            }

            return settings;
        }
    }

    public void UpdateSettings(AppSettings newSettings)
    {
        Preferences.Default.Set(ISettingsService.UseHeadsetProp, newSettings.UseHeadset);
        Preferences.Default.Set(ISettingsService.UseMediaControlsProp, newSettings.UseMediaControls);
        Preferences.Default.Set(ISettingsService.UseCustomDeviceProp, newSettings.UseCustomDevice);
        Preferences.Default.Set(ISettingsService.ImportDataFromFileProp, newSettings.ImportDataFromFile);
        Preferences.Default.Set(ISettingsService.ImportFromStravaProp, newSettings.ImportFromStrava);
        Preferences.Default.Set(ISettingsService.DebugProp, newSettings.Debug);
        Preferences.Default.Set(ISettingsService.SendSmsProp, newSettings.SendSms);
        Preferences.Default.Set(ISettingsService.SendSmsNumberProp, newSettings.SmsNumber);
        Preferences.Default.Set(ISettingsService.SendEmailsProp, newSettings.SendEmail);
        Preferences.Default.Set(ISettingsService.SendToEmailProp, newSettings.MailTo);
        Preferences.Default.Set(ISettingsService.SendFromEmailProp, newSettings.Username);
        Preferences.Default.Set(ISettingsService.SentToPasswordProp, newSettings.Password);
        Preferences.Default.Set(ISettingsService.SendToSmtpServerProp, newSettings.SmtpServer);
        Preferences.Default.Set(ISettingsService.SentToSmtpPortProp, newSettings.SmtpPort);

        settings.UseHeadset = newSettings.UseHeadset;
        settings.UseMediaControls = newSettings.UseMediaControls;
        settings.UseCustomDevice = newSettings.UseCustomDevice;
        settings.ImportDataFromFile = newSettings.ImportDataFromFile;
        settings.ImportFromStrava = newSettings.ImportFromStrava;
        settings.Debug = newSettings.Debug;
        settings.SendSms = newSettings.SendSms;
        settings.SmsNumber = newSettings.SmsNumber;
        settings.SendEmail = newSettings.SendEmail;
        settings.MailTo = newSettings.MailTo;
        settings.Username = newSettings.Username;
        settings.Password = newSettings.Password;
        settings.SmtpServer = newSettings.SmtpServer;
        settings.SmtpPort = newSettings.SmtpPort;
    }

    public void UpdateStravaApiCode(string? stravaApiCode)
    {
        Preferences.Default.Set(ISettingsService.StravaApiCodeProp, stravaApiCode ?? string.Empty);
        settings.StravaApiCode = stravaApiCode ?? string.Empty;
    }

    private void InitializeSettings()
    {
        settings.UseHeadset = Preferences.Default.Get(ISettingsService.UseHeadsetProp, false);
        settings.UseMediaControls = Preferences.Default.Get(ISettingsService.UseMediaControlsProp, false);
        settings.UseCustomDevice = Preferences.Default.Get(ISettingsService.UseCustomDeviceProp, false);
        settings.ImportDataFromFile = Preferences.Default.Get(ISettingsService.ImportDataFromFileProp, true);
        settings.ImportFromStrava = Preferences.Default.Get(ISettingsService.ImportFromStravaProp, false);
        settings.StravaApiCode = Preferences.Default.Get(ISettingsService.StravaApiCodeProp, string.Empty);
        settings.Debug = Preferences.Default.Get(ISettingsService.DebugProp, false);
        settings.SendSms = Preferences.Default.Get(ISettingsService.SendSmsProp, false);
        settings.SmsNumber = Preferences.Default.Get(ISettingsService.SendSmsNumberProp, string.Empty);
        settings.SendEmail = Preferences.Default.Get(ISettingsService.SendEmailsProp, false);
        settings.MailTo = Preferences.Default.Get(ISettingsService.SendToEmailProp, string.Empty);
        settings.Username = Preferences.Default.Get(ISettingsService.SendFromEmailProp, string.Empty);
        settings.Password = Preferences.Default.Get(ISettingsService.SentToPasswordProp, string.Empty);
        settings.SmtpServer = Preferences.Default.Get(ISettingsService.SendToSmtpServerProp, string.Empty);
        settings.SmtpPort = Preferences.Default.Get(ISettingsService.SentToSmtpPortProp, 0);
    }
}
