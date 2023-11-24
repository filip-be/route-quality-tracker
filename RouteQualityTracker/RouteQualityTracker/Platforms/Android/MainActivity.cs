using Android.App;
using Android.Content.PM;
using Android.Views;
using RouteQualityTracker.Interfaces;

namespace RouteQualityTracker;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
    {
        var page = Shell.Current.CurrentPage;
        if (page is ITrackingButtonsHandler handler &&
            keyCode is >= Keycode.Num0 and <= Keycode.Num9)
        {
            handler.OnButtonClick(keyCode.ToString());
            return true;
        }

        return base.OnKeyDown(keyCode, e);
    }
}
