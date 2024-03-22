using Android.App;
using Android.Content;

namespace RouteQualityTracker.Platforms.Android;

[Activity(Label = "Callback", Exported = true, MainLauncher = true)]
[IntentFilter([Intent.ActionView], Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataScheme = "https", DataHost = "route-quality-tracker-app.com", DataPath = "/TrackOperations")]
public class CallbackActivity : Activity
{
    protected override void OnResume()
    {
        Console.WriteLine(@"CallbackActivity started");
        base.OnResume();
        
        if(Intent?.Data is null) return;

        Console.WriteLine($@"URI: {Intent.Data.ToString()}");
        Console.WriteLine($@"Path: {Intent.Data.Path}");
        Console.WriteLine($@"Code: {Intent.Data.GetQueryParameter("code")}");
    }
}