using CommunityToolkit.Maui.Alerts;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;
using RouteQualityTracker.Services;

namespace RouteQualityTracker.Pages;

public partial class SettingsPage : ContentPage
{
    public const string ChevronUpIcon = "chevron_up.png";
    public const string ChevronDownIcon = "chevron_down.png";

    private readonly ISettingsService _settingsService;
    private readonly IServiceManager _serviceManager;

    public SettingsPage()
    {
        InitializeComponent();
        _settingsService = ServiceHelper.GetService<ISettingsService>();
        _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
    }

    protected override void OnAppearing()
    {
        InputOptionsGrid.IsEnabled = !_serviceManager.IsRunning;
        InputDeviceToggleDisabledLabel.IsVisible = _serviceManager.IsRunning;

        if (!UseHeadset.IsEnabled)
        {
            LoadSettings();

            UseHeadset.IsEnabled = true;
            UseMediaControls.IsEnabled = true;
            UseCustomDevice.IsEnabled = true;
            SendSmsSwitch.IsEnabled = true;
            SendEmailsSwitch.IsEnabled = true;
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
        UseHeadset.IsToggled = _settingsService.Settings.UseHeadset;
        UseMediaControls.IsToggled = _settingsService.Settings.UseMediaControls;
        UseCustomDevice.IsToggled = _settingsService.Settings.UseCustomDevice;
        ImportDataFromFile.IsToggled = _settingsService.Settings.ImportDataFromFile;
        ImportFromStrava.IsToggled = _settingsService.Settings.ImportFromStrava;
        SendSmsSwitch.IsToggled = _settingsService.Settings.SendSms;
        SmsNumber.Text = _settingsService.Settings.SmsNumber;
        SendEmailsSwitch.IsToggled = _settingsService.Settings.SendEmail;
        ToEntry.Text = _settingsService.Settings.MailTo;
        UsernameEntry.Text = _settingsService.Settings.Username;
        PasswordEntry.Text = _settingsService.Settings.Password;
        SmtpServerEntry.Text = _settingsService.Settings.SmtpServer;
        SmtpPortEntry.Text = _settingsService.Settings.SmtpPort.ToString();
    }

    private async void OnSaveSettings(object sender, EventArgs e)
    {
        _ = int.TryParse(SmtpPortEntry.Text, out int smtpPort);

        var newSettings = new AppSettings
        {
            UseHeadset = UseHeadset.IsToggled,
            UseMediaControls = UseMediaControls.IsToggled,
            UseCustomDevice = UseCustomDevice.IsToggled,
            ImportDataFromFile = ImportDataFromFile.IsToggled,
            ImportFromStrava = ImportFromStrava.IsToggled,
            SendSms = SendSmsSwitch.IsToggled,
            SmsNumber = SmsNumber.Text,
            SendEmail = SendEmailsSwitch.IsToggled,
            MailTo = ToEntry.Text,
            Username = UsernameEntry.Text,
            Password = PasswordEntry.Text,
            SmtpServer = SmtpServerEntry.Text,
            SmtpPort = smtpPort
        };

        _settingsService.UpdateSettings(newSettings);

        await Toast.Make("Settings saved").Show();
    }

    private void NotificationsExpandedChanged(object sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
    {
        NotificationExpanderIcon.Source = e.IsExpanded ? ImageSource.FromFile(ChevronUpIcon) : ImageSource.FromFile(ChevronDownIcon);
        NotificationDivider.IsVisible = e.IsExpanded;
    }

    private void InputExpandedChanged(object sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
    {
        InputExpanderIcon.Source = e.IsExpanded ? ImageSource.FromFile(ChevronUpIcon) : ImageSource.FromFile(ChevronDownIcon);
        InputDivider.IsVisible = e.IsExpanded;
    }

    private void ImportDataExpandedChanged(object sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
    {
        ImportDataExpanderIcon.Source = e.IsExpanded ? ImageSource.FromFile(ChevronUpIcon) : ImageSource.FromFile(ChevronDownIcon);
        ImportDataDivider.IsVisible = e.IsExpanded;
    }

    private void OnImportDataFileToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            ImportFromStrava.IsToggled = false;
        }
    }
    private void OnImportFromStravaToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            ImportDataFromFile.IsToggled = false;
        }
    }

    private void OnUseHeadsetToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            UseMediaControls.IsToggled = false;
            UseCustomDevice.IsToggled = false;
        }
    }

    private void OnUseMediaControlsToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            UseHeadset.IsToggled = false;
            UseCustomDevice.IsToggled = false;
        }
    }

    private void OnUseCustomDeviceToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            UseMediaControls.IsToggled = false;
            UseHeadset.IsToggled = false;
        }
    }
}