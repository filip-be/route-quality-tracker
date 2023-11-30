using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using static Android.Renderscripts.ScriptGroup;

namespace RouteQualityTracker.Platforms.Android;

[Service]
public class MediaButtonHandlerService : Service
{
    private const string CHANNEL_ID = "MediaButtonHandlerServiceChannel";

    public override void OnCreate()
    {
        base.OnCreate();

        CreateNotificationChannel();
    }

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        var input = intent!.GetStringExtra("inputExtra");
        var notificationIntent = new Intent(this, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable);

        var notification = new NotificationCompat
            .Builder(this, CHANNEL_ID)
                .SetContentTitle("Foreground Service")
                .SetContentText(input)
                .SetSmallIcon(Resource.Drawable.material_ic_keyboard_arrow_next_black_24dp)
                //.SetSmallIcon(R.drawable.ic_stat_name)
                .SetContentIntent(pendingIntent)
                .Build();

        StartForeground(1, notification);

        return StartCommandResult.NotSticky;
    }

    private void CreateNotificationChannel()
    {
        var serviceChannel = new NotificationChannel(
                CHANNEL_ID,
                "Foreground Service Channel",
                NotificationImportance.Default
        );

        var manager = GetSystemService(Context.NotificationService) as NotificationManager;
        manager.CreateNotificationChannel(serviceChannel);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override IBinder? OnBind(Intent? intent)
    {
        throw new NotImplementedException();
    }
}
