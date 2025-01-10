using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using RouteQualityTracker.Core.Extensions;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;
using RouteQualityTracker.Platforms.Android;

namespace RouteQualityTracker;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private readonly IServiceManager _serviceManager;
    private readonly ISettingsService _settingsService;
    private BluetoothGatt? _gattClient;
    private readonly IActivityIntegrationService _activityIntegrationService;
    private readonly ILoggingService _loggingService;

    private const string AppCallbackUri = "https://route-quality-tracker-app.com/StravaAuthorize";

    public MainActivity()
    {
        _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
        _settingsService = ServiceHelper.Services.GetService<ISettingsService>()!;
        _serviceManager.OnServiceStart += OnServiceStart;
        _serviceManager.OnServiceStop += OnServiceStop;

        _activityIntegrationService = ServiceHelper.Services.GetService<IActivityIntegrationService>()!;
        _activityIntegrationService.OnAuthenticateViaStrava += OnAuthenticateViaStrava;

        _loggingService = ServiceHelper.Services.GetService<ILoggingService>()!;
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode != StravaAuthorizeCallbackActivity.ActivityRequestCode) return;

        _loggingService.Debug($"Strava authentication activity has completed: {requestCode}: {resultCode}");
        _activityIntegrationService.NotifyStravaAuthenticationHasCompleted();
    }

    private void OnAuthenticateViaStrava(object? sender, string clientId)
    {
        var stravaAppUri = new Uri("https://www.strava.com/oauth/mobile/authorize")
            .AddParameter("client_id", clientId)
            .AddParameter("redirect_uri", AppCallbackUri)
            .AddParameter("response_type", "code")
            .AddParameter("approval_prompt", "auto")
            .AddParameter("scope", "activity:read_all")
            .ToString();

        _loggingService.Debug("Authenticating to Strava");
        var stravaAppIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(stravaAppUri));
        StartActivityForResult(stravaAppIntent, StravaAuthorizeCallbackActivity.ActivityRequestCode);
    }

    private void OnServiceStop(object? sender, EventArgs e)
    {
        try
        {
            var intent = GetServiceIntent();
            _loggingService.Info("Stopping service..");
            StopService(intent);

            _gattClient?.Close();
            _gattClient = null;
            _serviceManager.SetStatus(false);
        }
        catch (Exception ex)
        {
            _serviceManager.SetStatus(false, ex);
        }
    }

    private void OnServiceStart(object? sender, EventArgs e)
    {
        try
        {
            _loggingService.Info("Starting service...");
            if (_settingsService.Settings.UseCustomDevice)
            {
                _loggingService.Trace("Retrieving BluetoothManager service");
                var bluetoothManager = (BluetoothManager?) GetSystemService(BluetoothService);
                if (bluetoothManager is null)
                {
                    _serviceManager.SetStatus(false,
                        new InvalidOperationException("Bluetooth service is not available"));
                    return;
                }

                _loggingService.Trace("Retrieving Bluetooth Adapter");
                var bluetoothAdapter = bluetoothManager.Adapter;
                if (bluetoothAdapter is null)
                {
                    _serviceManager.SetStatus(false,
                        new InvalidOperationException("Bluetooth adapter is not available"));
                    return;
                }

                _loggingService.Trace("Retrieving bonded devices");
                var trackingDevice = bluetoothAdapter.BondedDevices?.FirstOrDefault(d =>
                    d.Name?.Equals("RouteQualityTracker Device", StringComparison.OrdinalIgnoreCase) == true
                    || d.Name?.Equals("QualityTracker", StringComparison.OrdinalIgnoreCase) == true);

                if (trackingDevice is null)
                {
                    _serviceManager.SetStatus(false,
                        new InvalidOperationException(
                            "Unable to find quality tracking device. Please check if it is connected via Bluetooth"));
                    return;
                }
                _loggingService.Debug($"Quality tracking device found: {trackingDevice.Name}");

                _loggingService.Trace("Connecting to Bluetooth device via Gatt");
                _gattClient = trackingDevice.ConnectGatt(this, true, new RouteQualityGattCallback());
                if (_gattClient is null)
                {
                    _serviceManager.SetStatus(false,
                        new InvalidOperationException("Error connecting to Bluetooth device"));
                    return;
                }
                _loggingService.Trace("Gatt client connected");

                _serviceManager.SetStatus(true);
            }

            _loggingService.Debug("Starting foreground service");
            var intent = GetServiceIntent();
            StartForegroundService(intent);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"Error starting service: {ex.Message}");
            _serviceManager.SetStatus(false, ex);
        }
    }

    private Intent GetServiceIntent()
    {
        if (_settingsService.Settings.UseCustomDevice)
        {
            return new Intent(this, typeof(BlePositioningDeviceService));
        }

        if (_settingsService.Settings.UseHeadset || _settingsService.Settings.UseMediaControls)
        {
            return new Intent(this, typeof(MediaButtonHandlerService));
        }

        throw new NotImplementedException("Please select input device");
    }
}
