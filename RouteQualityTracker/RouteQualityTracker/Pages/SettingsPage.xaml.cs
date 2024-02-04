using CommunityToolkit.Maui.Alerts;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;
using RouteQualityTracker.Services;

namespace RouteQualityTracker.Pages;

public partial class SettingsPage : ContentPage
{
    public const string ChevronUpIcon = "chevron_up.png";
    public const string ChevronDownIcon = "chevron_down.png";

    private readonly ISettingsService _settingsService;

    public SettingsPage()
    {
        InitializeComponent();
        _settingsService = ServiceHelper.GetService<ISettingsService>();
    }

    protected override void OnAppearing()
    {
        if (!useHeadset.IsEnabled)
        {
            LoadSettings();

            useHeadset.IsEnabled = true;
            useMediaControls.IsEnabled = true;
            sendSmsSwitch.IsEnabled = true;
            sendEmailsSwitch.IsEnabled = true;
        }
        base.OnAppearing();
    }

    private async void OnLoadSettings(object sender, EventArgs e)
    {
        LoadSettings();

        await Toast.Make("Settings loaded").Show();
    }

    private void LoadSettings()
    {
        useHeadset.IsToggled = _settingsService.Settings.UseHeadset;
        useMediaControls.IsToggled = _settingsService.Settings.UseMediaControls;
        useCustomDevice.IsToggled = _settingsService.Settings.UseCustomDevice;
        sendSmsSwitch.IsToggled = _settingsService.Settings.SendSms;
        smsNumber.Text = _settingsService.Settings.SmsNumber;
        sendEmailsSwitch.IsToggled = _settingsService.Settings.SendEmail;
        toEntry.Text = _settingsService.Settings.MailTo;
        usernameEntry.Text = _settingsService.Settings.Username;
        passwordEntry.Text = _settingsService.Settings.Password;
        smtpServerEntry.Text = _settingsService.Settings.SmtpServer;
        smtpPortEntry.Text = _settingsService.Settings.SmtpPort.ToString();
    }

    private async void OnSaveSettings(object sender, EventArgs e)
    {
        _ = int.TryParse(smtpPortEntry.Text, out int smtpPort);

        var newSettings = new AppSettings
        {
            UseHeadset = useHeadset.IsToggled,
            UseMediaControls = useMediaControls.IsToggled,
            UseCustomDevice = useCustomDevice.IsToggled,
            SendSms = sendSmsSwitch.IsToggled,
            SmsNumber = smsNumber.Text,
            SendEmail = sendEmailsSwitch.IsToggled,
            MailTo = toEntry.Text,
            Username = usernameEntry.Text,
            Password = passwordEntry.Text,
            SmtpServer = smtpServerEntry.Text,
            SmtpPort = smtpPort
        };

        _settingsService.UpdateSettings(newSettings);

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
    
    private void OnUseHeadsetToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            useMediaControls.IsToggled = false;
            useCustomDevice.IsToggled = false;
        }
    }

    private void OnUseMediaControlsToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            useHeadset.IsToggled = false;
            useCustomDevice.IsToggled = false;
        }
    }

    private void OnUseCustomDeviceToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            useMediaControls.IsToggled = false;
            useHeadset.IsToggled = false;
        }
    }
}