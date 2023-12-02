using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;

namespace RouteQualityTracker.Pages;

public partial class SettingsPage : ContentPage
{
    public const string SendEmailsProp = "SendEmails";
    public const string SendToEmailProp = "SendToEmail";
    public const string SentToPasswordProp = "SentToPassword";
    public const string SendToSmtpServerProp = "SendToSmtpServer";
    public const string SentToSmtpPortProp = "SentToSmtpPort";

    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void OnLoadSettings(object sender, EventArgs e)
    {
        sendEmailsSwitch.IsToggled = Preferences.Default.Get(SendEmailsProp, false);
        emailEntry.Text = Preferences.Default.Get(SendToEmailProp, string.Empty);
        passwordEntry.Text = Preferences.Default.Get(SentToPasswordProp, string.Empty);
        smtpServerEntry.Text = Preferences.Default.Get(SendToSmtpServerProp, string.Empty);
        smtpPortEntry.Text = Preferences.Default.Get(SentToSmtpPortProp, 0).ToString();

        sendEmailsSwitch.IsEnabled = true;

        await Toast.Make("Settings loaded").Show();
    }

    private async void OnSaveSettings(object sender, EventArgs e)
    {
        Preferences.Default.Set(SendEmailsProp, sendEmailsSwitch.IsToggled);
        Preferences.Default.Set(SendToEmailProp, emailEntry.Text);
        Preferences.Default.Set(SentToPasswordProp, passwordEntry.Text);
        Preferences.Default.Set(SendToSmtpServerProp, smtpServerEntry.Text);

        _ = int.TryParse(smtpPortEntry.Text, out int smtpPort);
        Preferences.Default.Set(SentToSmtpPortProp, smtpPort);

        await Toast.Make("Settings saved").Show();
    }
}