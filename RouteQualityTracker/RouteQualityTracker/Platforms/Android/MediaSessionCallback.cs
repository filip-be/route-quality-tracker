using Android.Media.Session;
using Android.Views;
using Java.Lang;
using Android.Content;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Platforms.Android;
public class MediaSessionCallback : MediaSession.Callback
{
    private readonly IQualityTrackingService _qualityTrackingService;

    public MediaSessionCallback()
    {
        _qualityTrackingService = ServiceHelper.GetService<IQualityTrackingService>();
    }

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

        if (keyEvent.Action == KeyEventActions.Up)
        {
            _qualityTrackingService.ToggleRouteQuality();
        }

        return true;
    }
}