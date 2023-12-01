using Android.Content;

namespace RouteQualityTracker.Platforms.Android;

[BroadcastReceiver]
public class MediaButtonReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        System.Diagnostics.Debug.WriteLine(intent?.Action ?? "unkown");
    }
}
