using Android.App;
using Android.Bluetooth;
using Android.Companion;
using Android.Content;
using Android.Content.PM;
using Android.Views;
using Java.Lang;
using RouteQualityTracker.Interfaces;
using RouteQualityTracker.Platforms.Android;
using RouteQualityTracker.Services;

namespace RouteQualityTracker;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private class BluetoothCallback : CompanionDeviceManager.Callback
    {
        private readonly ITrackingButtonsHandler _trackingButtonsHandler;
        private readonly Activity _activity;

        public BluetoothCallback(ITrackingButtonsHandler trackingButtonsHandler, Activity activity) : base()
        {
            _trackingButtonsHandler = trackingButtonsHandler;
            _activity = activity;
        }

        public override void OnDeviceFound(IntentSender intentSender)
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                base.OnDeviceFound(intentSender);
                _activity.StartIntentSenderForResult(intentSender, SELECT_DEVICE_REQUEST_CODE, null, 0, 0, 0);
            }
        }

        public override void OnAssociationPending(IntentSender intentSender)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                base.OnAssociationPending(intentSender);
                _activity.StartIntentSenderForResult(intentSender, SELECT_DEVICE_REQUEST_CODE, null, 0, 0, 0);
            }
        }

        public override void OnAssociationCreated(AssociationInfo associationInfo)
        {
            _trackingButtonsHandler.OnButtonClick($"Associated with {associationInfo?.DisplayName}");

            base.OnAssociationCreated(associationInfo);
        }

        public override void OnFailure(ICharSequence? error)
        {
            _trackingButtonsHandler.OnButtonClick(error?.ToString());
        }
    }

    private readonly IServiceManager _serviceManager;

    private const int SELECT_DEVICE_REQUEST_CODE = 0;

    public MainActivity() : base()
    {
        _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
        _serviceManager.OnServiceStart += OnServiceStart;
        _serviceManager.OnServiceStop += OnServiceStop;
    }

    private void OnServiceStop(object? sender, EventArgs e)
    {
        var intent = new Intent(this, typeof(MediaButtonHandlerService));
        
        StopService(intent);
    }

    private void OnServiceStart(object? sender, EventArgs e)
    {
        var intent = new Intent(this, typeof(MediaButtonHandlerService));
        intent.PutExtra("inputExtra", "Foreground Service Example in Android");

        try
        {
            //var xx = StartService(intent);
            var xx = StartForegroundService(intent);
        }
        catch(Java.Lang.Exception ex)
        {
            (Shell.Current.CurrentPage as ITrackingButtonsHandler).OnButtonClick(ex.Message);
        }
        catch(System.Exception ex)
        {
            (Shell.Current.CurrentPage as ITrackingButtonsHandler).OnButtonClick(ex.Message);
        }
        
    }
    //private void OnServiceStart(object? sender, EventArgs e)
    //{
    //    var deviceManager = (CompanionDeviceManager) GetSystemService(CompanionDeviceService)!;

    //    var deviceFilter = new BluetoothLeDeviceFilter.Builder();
    //    var pairingRequest = new AssociationRequest.Builder()
    //        .AddDeviceFilter(deviceFilter.Build());

    //    deviceManager.Associate(pairingRequest.Build(), new BluetoothCallback((ITrackingButtonsHandler)Shell.Current.CurrentPage, this), null);
    //}

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if(resultCode != Result.Ok) {
            return;
        }
        if (requestCode == SELECT_DEVICE_REQUEST_CODE)
        {
            if (resultCode == Result.Ok && data != null)
            {
                BluetoothDevice deviceToPair = (BluetoothDevice)data.GetParcelableExtra(
                    CompanionDeviceManager.ExtraDevice
                );

                if (deviceToPair != null)
                {
                    deviceToPair.CreateBond();
                    // ... Continue interacting with the paired device.
                }
            }
        }
        else
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }
    }

    public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
    {
        var page = Shell.Current.CurrentPage;
        //KeyCode.Headsethook
        if (page is ITrackingButtonsHandler handler)
        {
            System.Diagnostics.Debug.WriteLine($"Button clicked: {keyCode}");
            handler.OnButtonClick(keyCode.ToString());
            return true;
        }

        return base.OnKeyDown(keyCode, e);
    }
}
