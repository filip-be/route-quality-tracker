using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Services;

public interface ISettingsService
{
    const string SendSmsProp = "SendSms";
    const string SendSmsNumberProp = "SendSmsNumber";
    const string SendEmailsProp = "SendEmails";
    const string SendFromEmailProp = "SendFromEmail";
    const string SendToEmailProp = "SendToEmail";
    const string SentToPasswordProp = "SentToPassword";
    const string SendToSmtpServerProp = "SendToSmtpServer";
    const string SentToSmtpPortProp = "SentToSmtpPort";
    const string UseHeadsetProp = "UseHeadset";
    const string UseMediaControlsProp = "UseMediaControls";
    const string UseCustomDeviceProp = "UseCustomDevice";
    const string ImportDataFromFileProp = "ImportDataFromFile";
    const string ImportFromStravaProp = "ImportFromStrava";
    const string StravaApiCodeProp = "StravaApiCode";

    AppSettings Settings { get; }

    void UpdateSettings(AppSettings newSettings);

    void UpdateStravaApiCode(string? stravaApiCode);
}