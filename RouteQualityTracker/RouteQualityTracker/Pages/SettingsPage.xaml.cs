using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Pages;

public partial class SettingsPage : ContentPage
{
    public const string SendSmsProp = "SendSms";
    public const string SendSmsNumberProp = "SendSmsNumber";
    public const string SendEmailsProp = "SendEmails";
    public const string SendFromEmailProp = "SendFromEmail";
    public const string SendToEmailProp = "SendToEmail";
    public const string SentToPasswordProp = "SentToPassword";
    public const string SendToSmtpServerProp = "SendToSmtpServer";
    public const string SentToSmtpPortProp = "SentToSmtpPort";

    private readonly NotificationSettings _notificationSettings;

    public SettingsPage()
    {
        InitializeComponent();
        _notificationSettings = ServiceHelper.GetService<NotificationSettings>();
        UpdateNotificationSettings();
    }

    private void UpdateNotificationSettings()
    {
        _notificationSettings.SendSms = Preferences.Default.Get(SendSmsProp, false);
        _notificationSettings.SmsNumber = Preferences.Default.Get(SendSmsNumberProp, string.Empty);
        _notificationSettings.SendEmail = Preferences.Default.Get(SendEmailsProp, false);
        _notificationSettings.MailTo = Preferences.Default.Get(SendToEmailProp, string.Empty);
        _notificationSettings.Username = Preferences.Default.Get(SendFromEmailProp, string.Empty);
        _notificationSettings.Password = Preferences.Default.Get(SentToPasswordProp, string.Empty);
        _notificationSettings.SmtpServer = Preferences.Default.Get(SendToSmtpServerProp, string.Empty);
        _notificationSettings.SmtpPort = Preferences.Default.Get(SentToSmtpPortProp, 0);
    }

    private async void OnLoadSettings(object sender, EventArgs e)
    {
        UpdateNotificationSettings();

        sendSmsSwitch.IsToggled = _notificationSettings.SendSms;
        smsNumber.Text = _notificationSettings.SmsNumber;
        sendEmailsSwitch.IsToggled = _notificationSettings.SendEmail;
        toEntry.Text = _notificationSettings.MailTo;
        usernameEntry.Text = _notificationSettings.Username;
        passwordEntry.Text = _notificationSettings.Password;
        smtpServerEntry.Text = _notificationSettings.SmtpServer;
        smtpPortEntry.Text = _notificationSettings.SmtpPort.ToString();

        sendSmsSwitch.IsEnabled = true;
        sendEmailsSwitch.IsEnabled = true;

        await Toast.Make("Settings loaded").Show();
    }

    private async void OnSaveSettings(object sender, EventArgs e)
    {
        Preferences.Default.Set(SendSmsProp, sendSmsSwitch.IsToggled);
        Preferences.Default.Set(SendSmsNumberProp, smsNumber.Text);
        Preferences.Default.Set(SendEmailsProp, sendEmailsSwitch.IsToggled);
        Preferences.Default.Set(SendToEmailProp, toEntry.Text);
        Preferences.Default.Set(SendFromEmailProp, usernameEntry.Text);
        Preferences.Default.Set(SentToPasswordProp, passwordEntry.Text);
        Preferences.Default.Set(SendToSmtpServerProp, smtpServerEntry.Text);

        _ = int.TryParse(smtpPortEntry.Text, out int smtpPort);
        Preferences.Default.Set(SentToSmtpPortProp, smtpPort);

        UpdateNotificationSettings();

        await Toast.Make("Settings saved").Show();
    }
}