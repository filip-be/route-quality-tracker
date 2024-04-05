using Android.Media.Session;
using Android.Views;
using Java.Lang;
using Android.Content;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Platforms.Android;
public class MediaSessionCallback : MediaSession.Callback
{
    private readonly IQualityTrackingService _qualityTrackingService = ServiceHelper.GetService<IQualityTrackingService>();
    private readonly ISettingsService _settingsService = ServiceHelper.GetService<ISettingsService>();

    public override bool OnMediaButtonEvent(Intent mediaButtonIntent)
    {
        // 2 events triggered
        //[0:] Media button clicked! KeyEvent: Down
        //[0:] Media button clicked! KeyEvent: Up
        KeyEvent keyEvent;
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            keyEvent = (KeyEvent)mediaButtonIntent.GetParcelableExtra(Intent.ExtraKeyEvent, Class.FromType(typeof(KeyEvent)))!;
        }
        else
        {
            keyEvent = (KeyEvent)mediaButtonIntent.GetParcelableExtra(Intent.ExtraKeyEvent)!;
        }

        if (_settingsService.Settings.UseHeadset)
        {
            return HandleHeadsetClick(keyEvent);
        }

        if (_settingsService.Settings.UseMediaControls)
        {
            return HandleMediaControlClick(keyEvent);
        }

        return true;
    }

    private bool HandleMediaControlClick(KeyEvent keyEvent)
    {
        switch (keyEvent.KeyCode)
        {
            case Keycode.MediaPrevious:
                if (keyEvent.Action == KeyEventActions.Up)
                {
                    _qualityTrackingService.SetRouteQuality(RouteQualityEnum.Bad);
                }
                return true;
            case Keycode.MediaPlayPause:
                if (keyEvent.Action == KeyEventActions.Up)
                {
                    _qualityTrackingService.SetRouteQuality(RouteQualityEnum.Standard);
                }
                return true;
            case Keycode.MediaNext:
                if (keyEvent.Action == KeyEventActions.Up)
                {
                    _qualityTrackingService.SetRouteQuality(RouteQualityEnum.Good);
                }
                return true;
            default:
                return false;
        }
    }

    private bool HandleHeadsetClick(KeyEvent keyEvent)
    {
        if (keyEvent.KeyCode != Keycode.MediaPlayPause)
        {
            return false;
        }

        if (keyEvent.Action == KeyEventActions.Up)
        {
            _qualityTrackingService.ToggleRouteQuality();
        }

        return true;
    }
}