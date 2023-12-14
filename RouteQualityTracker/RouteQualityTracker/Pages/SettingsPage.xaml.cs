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
    public const string UseHeadsetProp = "UseHeadset";
    public const string UseMediaControlsProp = "UseMediaControls";

    public const string ChevronUpIcon = "chevron_up.png";
    public const string ChevronDownIcon = "chevron_down.png";

    private readonly NotificationSettings _notificationSettings;

    public SettingsPage()
    {
        InitializeComponent();
        _notificationSettings = ServiceHelper.GetService<NotificationSettings>();
        UpdateNotificationSettings();
    }

    private void UpdateNotificationSettings()
    {
        _notificationSettings.UseHeadset = Preferences.Default.Get(UseHeadsetProp, false);
        _notificationSettings.UseMediaControls = Preferences.Default.Get(UseMediaControlsProp, false);
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

        useHeadset.IsToggled = _notificationSettings.UseHeadset;
        useMediaControls.IsToggled = _notificationSettings.UseMediaControls;
        sendSmsSwitch.IsToggled = _notificationSettings.SendSms;
        smsNumber.Text = _notificationSettings.SmsNumber;
        sendEmailsSwitch.IsToggled = _notificationSettings.SendEmail;
        toEntry.Text = _notificationSettings.MailTo;
        usernameEntry.Text = _notificationSettings.Username;
        passwordEntry.Text = _notificationSettings.Password;
        smtpServerEntry.Text = _notificationSettings.SmtpServer;
        smtpPortEntry.Text = _notificationSettings.SmtpPort.ToString();

        useHeadset.IsEnabled = true;
        useMediaControls.IsEnabled = true;
        sendSmsSwitch.IsEnabled = true;
        sendEmailsSwitch.IsEnabled = true;

        await Toast.Make("Settings loaded").Show();
    }

    private async void OnSaveSettings(object sender, EventArgs e)
    {
        Preferences.Default.Set(UseHeadsetProp, useHeadset.IsToggled);
        Preferences.Default.Set(UseMediaControlsProp, useMediaControls.IsToggled);
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

    private void NotificationsExpandedChanged(object sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
    {
        notificationExpanderIcon.Source = e.IsExpanded ? ImageSource.FromFile(ChevronUpIcon) : ImageSource.FromFile(ChevronDownIcon);
        notificationDivider.IsVisible = e.IsExpanded;
    }

    private void InputExpandedChanged(object sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
    {
        inputExpanderIcon.Source = e.IsExpanded ? ImageSource.FromFile(ChevronUpIcon) : ImageSource.FromFile(ChevronDownIcon);
        inputDivider.IsVisible = e.IsExpanded;
    }
}