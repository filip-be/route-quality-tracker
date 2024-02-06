using Android.App;
using Android.App.Slices;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Companion;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Concurrent.Futures;
using Java.Lang;
using Java.Util;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;
using RouteQualityTracker.Platforms.Android;
using RouteQualityTracker.Services;
using static AndroidX.Core.App.ShareCompat;

namespace RouteQualityTracker;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private readonly IServiceManager _serviceManager;
    private readonly ISettingsService _settingsService;


    const int SELECT_DEVICE_REQUEST_CODE = 0;

    public MainActivity()
    {
        _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
        _settingsService = ServiceHelper.Services.GetService<ISettingsService>()!;
        _serviceManager.OnServiceStart += OnServiceStart;
        _serviceManager.OnServiceStop += OnServiceStop;
    }

    private void OnServiceStop(object? sender, EventArgs e)
    {
        try
        {
            var intent = GetServiceIntent();
            StopService(intent);
            _serviceManager.SetStatus(false);
        }
        catch (System.Exception ex)
        {
            _serviceManager.SetStatus(false, ex);
        }
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        // ToDo: Handle callback activities
        // https://developer.android.com/develop/connectivity/bluetooth/companion-device-pairing
        if (requestCode == SELECT_DEVICE_REQUEST_CODE && data != null)
        {
            // ToDo: update code not to use obsolete method
            var deviceToPair = (ScanResult) data.GetParcelableExtra(CompanionDeviceManager.ExtraDevice);

            // ToDo:
            // Get device from BluetoothAdapter or scan result
            // create bond
            // listen for characteristics change
            // start empty foreground service if still needed            

            if (deviceToPair != null)
            {
                //deviceToPair.CreateBond();
                // Continue to interact with the paired device.
            }
        }
        else
        {

            base.OnActivityResult(requestCode, resultCode, data);
        }
    }

    private void OnServiceStart(object? sender, EventArgs e)
    {
        try
        {
            if (_settingsService.Settings.UseCustomDevice)
            {
                var deviceFilter = new BluetoothLeDeviceFilter.Builder()
                //.SetNamePattern("RouteQualityTracker Device")
                .SetScanFilter(new ScanFilter
                    .Builder()
                    //.SetServiceUuid(new ParcelUuid(new UUID(0x1800L, -1L)))!
                    .Build()
                )
                .Build();

                var pairingRequest = new AssociationRequest.Builder()
                    .AddDeviceFilter(deviceFilter)
                    .SetSingleDevice(false)
                    .Build();

                var deviceManager = (CompanionDeviceManager)GetSystemService(CompanionDeviceService)!;

                if (OperatingSystem.IsAndroidVersionAtLeast(33))
                {
                    deviceManager.Associate(pairingRequest, DirectExecutor.Instance!, new AssociationCallback(_serviceManager, this));
                }
                else
                {
                    deviceManager.Associate(pairingRequest, new AssociationCallback(_serviceManager, this), null);
                }
            }
            else
            {
                var intent = GetServiceIntent();
                StartForegroundService(intent);
            }
        }
        catch(System.Exception ex)
        {
            _serviceManager.SetStatus(false, ex);
        }
    }

    private Intent GetServiceIntent()
    {
        if(_settingsService.Settings.UseCustomDevice)
        {
            return new Intent(this, typeof(BlePositioningDeviceService));
        }

        if(_settingsService.Settings.UseHeadset || _settingsService.Settings.UseMediaControls)
        {
            return new Intent(this, typeof(MediaButtonHandlerService));
        }

        throw new NotImplementedException("Please select input device");
    }

    class AssociationCallback(IServiceManager serviceManager, Activity activity) : CompanionDeviceManager.Callback
    {

        public override void OnDeviceFound(IntentSender intentSender)
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                base.OnDeviceFound(intentSender);
            }

            activity.StartIntentSenderForResult(intentSender, SELECT_DEVICE_REQUEST_CODE, null, 0, 0, 0);
        }

        public override void OnFailure(ICharSequence? error)
        {
            // ToDo: sent failed operation result intent
            //activity.StartIntentSenderForResult(intentSender, SELECT_DEVICE_REQUEST_CODE, null, 0, 0, 0);
            //serviceManager.SetStatus(false, new System.OperationCanceledException(error?.ToString()));
        }
    }
}
