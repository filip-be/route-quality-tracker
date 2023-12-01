using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;

namespace RouteQualityTracker.Platforms.Android;

[Service(ForegroundServiceType = ForegroundService.TypeMediaPlayback, Exported = true)]
[IntentFilter(new[] { Intent.ActionMediaButton })]
public class MediaButtonHandlerService : Service
{
    private const string ChannelId = "MediaButtonHandlerServiceChannel";
    private const int NotificationId = 1000;
    private const string NotificationChannelName = "Media button handler service";

    public override void OnCreate()
    {
        base.OnCreate();

        CreateNotificationChannel();
    }

    internal static NotificationCompat.Action GenerateActionCompat(Context context, int icon, string title, string intentAction)
    {
        Intent intent = new Intent(context, typeof(MediaButtonHandlerService));
        intent.SetAction(intentAction);

        PendingIntentFlags flags = PendingIntentFlags.UpdateCurrent;
        //if (intentAction.Equals(MediaPlayerService.ActionStop))
        //{
        //    flags = PendingIntentFlags.CancelCurrent;
        //}

        flags |= PendingIntentFlags.Mutable;

        PendingIntent pendingIntent = PendingIntent.GetService(context, 1, intent, flags);

        return new NotificationCompat.Action.Builder(icon, title, pendingIntent).Build();
    }

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        var input = intent!.GetStringExtra("inputExtra");

        var notificationBuilder = new NotificationCompat
                .Builder(this, ChannelId)
            .SetAutoCancel(false)
            .SetOngoing(true)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle("Foreground Service")
            .SetContentText(input)
            .AddAction(GenerateActionCompat(this, Microsoft.Maui.Resource.Drawable.ic_call_decline, "Switch", Intent.ActionMediaButton));

        if (OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            StartForeground(NotificationId, notificationBuilder.Build(), ForegroundService.TypeMediaPlayback);
        }
        else
        {
            StartForeground(NotificationId, notificationBuilder.Build());
        }


        return StartCommandResult.NotSticky;
    }

    private void CreateNotificationChannel()
    {
        var serviceChannel = new NotificationChannel(
                ChannelId,
                NotificationChannelName,
                NotificationImportance.Low
        );

        var manager = GetSystemService(NotificationService) as NotificationManager;
        manager!.CreateNotificationChannel(serviceChannel);
    }

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }
}
