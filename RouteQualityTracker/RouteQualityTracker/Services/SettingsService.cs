using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Services;

public class SettingsService : ISettingsService
{
    private readonly AppSettings _settings;
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

            return _settings;
        }
    }

    public SettingsService(AppSettings settings)
    {
        _settings = settings;
    }

    public void UpdateSettings(AppSettings newSettings)
    {
        Preferences.Default.Set(ISettingsService.UseHeadsetProp, newSettings.UseHeadset);
        Preferences.Default.Set(ISettingsService.UseMediaControlsProp, newSettings.UseMediaControls);
        Preferences.Default.Set(ISettingsService.UseCustomDeviceProp, newSettings.UseCustomDevice);
        Preferences.Default.Set(ISettingsService.ImportDataFromFileProp, newSettings.ImportDataFromFile);
        Preferences.Default.Set(ISettingsService.ImportFromStravaProp, newSettings.ImportFromStrava);
        Preferences.Default.Set(ISettingsService.SendSmsProp, newSettings.SendSms);
        Preferences.Default.Set(ISettingsService.SendSmsNumberProp, newSettings.SmsNumber);
        Preferences.Default.Set(ISettingsService.SendEmailsProp, newSettings.SendEmail);
        Preferences.Default.Set(ISettingsService.SendToEmailProp, newSettings.MailTo);
        Preferences.Default.Set(ISettingsService.SendFromEmailProp, newSettings.Username);
        Preferences.Default.Set(ISettingsService.SentToPasswordProp, newSettings.Password);
        Preferences.Default.Set(ISettingsService.SendToSmtpServerProp, newSettings.SmtpServer);
        Preferences.Default.Set(ISettingsService.SentToSmtpPortProp, newSettings.SmtpPort);

        _settings.UseHeadset = newSettings.UseHeadset;
        _settings.UseMediaControls = newSettings.UseMediaControls;
        _settings.UseCustomDevice = newSettings.UseCustomDevice;
        _settings.ImportDataFromFile = newSettings.ImportDataFromFile;
        _settings.ImportFromStrava = newSettings.ImportFromStrava;
        _settings.SendSms = newSettings.SendSms;
        _settings.SmsNumber = newSettings.SmsNumber;
        _settings.SendEmail = newSettings.SendEmail;
        _settings.MailTo = newSettings.MailTo;
        _settings.Username = newSettings.Username;
        _settings.Password = newSettings.Password;
        _settings.SmtpServer = newSettings.SmtpServer;
        _settings.SmtpPort = newSettings.SmtpPort;
    }

    private void InitializeSettings()
    {
        _settings.UseHeadset = Preferences.Default.Get(ISettingsService.UseHeadsetProp, false);
        _settings.UseMediaControls = Preferences.Default.Get(ISettingsService.UseMediaControlsProp, false);
        _settings.UseCustomDevice = Preferences.Default.Get(ISettingsService.UseCustomDeviceProp, false);
        _settings.ImportDataFromFile = Preferences.Default.Get(ISettingsService.ImportDataFromFileProp, true);
        _settings.ImportFromStrava = Preferences.Default.Get(ISettingsService.ImportFromStravaProp, false);
        _settings.SendSms = Preferences.Default.Get(ISettingsService.SendSmsProp, false);
        _settings.SmsNumber = Preferences.Default.Get(ISettingsService.SendSmsNumberProp, string.Empty);
        _settings.SendEmail = Preferences.Default.Get(ISettingsService.SendEmailsProp, false);
        _settings.MailTo = Preferences.Default.Get(ISettingsService.SendToEmailProp, string.Empty);
        _settings.Username = Preferences.Default.Get(ISettingsService.SendFromEmailProp, string.Empty);
        _settings.Password = Preferences.Default.Get(ISettingsService.SentToPasswordProp, string.Empty);
        _settings.SmtpServer = Preferences.Default.Get(ISettingsService.SendToSmtpServerProp, string.Empty);
        _settings.SmtpPort = Preferences.Default.Get(ISettingsService.SentToSmtpPortProp, 0);
    }
}
