using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Java.Util;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Platforms.Android;

[Service(ForegroundServiceType = ForegroundService.TypeConnectedDevice, Exported = true)]
public class BlePositioningDeviceService : Service
{
    private IServiceManager _serviceManager;
    private readonly IAndroidNotificationService _androidNotificationService;
    private readonly IQualityTrackingService _qualityTrackerService;

    private readonly BleScanCallback _bleScanCallback;

    private NotificationManager NotificationManager => (NotificationManager)GetSystemService(NotificationService)!;
    private const int NotificationId = 1010;

    public BlePositioningDeviceService()
    {
        _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
        _androidNotificationService = ServiceHelper.Services.GetService<IAndroidNotificationService>()!;
        _qualityTrackerService = ServiceHelper.Services.GetService<IQualityTrackingService>()!;

        _bleScanCallback = new BleScanCallback(_serviceManager, this);
    }

    public override void OnCreate()
    {
        base.OnCreate();

        _androidNotificationService.CreateNotificationChannel(this, NotificationManager, NotificationId);
    }

    public override IBinder? OnBind(Intent? intent)
    {
        throw new NotImplementedException();
    }

    public override bool StopService(Intent? name)
    {
        _bleScanCallback.Disconnect();
        _qualityTrackerService.OnRouteQualityChanged -= _androidNotificationService.HandleRouteQualityChangeEvent;
        return base.StopService(name);
    }

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        var notification = _androidNotificationService.CreateActivityNotification(this, null);

        // look for BLE Device before starting foreground service

        var bluetoothManager = (BluetoothManager?)GetSystemService(BluetoothService);
        if(bluetoothManager is null)
        {
            _serviceManager.SetStatus(false, new InvalidOperationException("Bluetooth service is not available"));
            return StartCommandResult.NotSticky;
        }

        var bluetoothAdapter = bluetoothManager.Adapter;
        if (bluetoothAdapter is null)
        {
            _serviceManager.SetStatus(false, new InvalidOperationException("Bluetooth adapter is not available"));
            return StartCommandResult.NotSticky;
        }

        var bleScanner = bluetoothAdapter.BluetoothLeScanner;
        if (bleScanner is null)
        {
            _serviceManager.SetStatus(false, new InvalidOperationException("Bluetooth LE scanner is not available"));
            return StartCommandResult.NotSticky;
        }

        _serviceManager.DisplayMessage("Looking for position quality tracking device");
        //bleScanner.StartScan(_bleScanCallback);
        bleScanner.StartScan(new List<ScanFilter> {
            new ScanFilter
                .Builder()
                .SetDeviceName("RouteQualityTracker Device")
                //.SetServiceUuid(
                //    new ParcelUuid(new UUID(0x1800L, -1L)))
                !.Build()! },
            new ScanSettings
                .Builder()
                .Build(),
            _bleScanCallback);

        //var deviceFilter = new BluetoothLeDeviceFilter.Builder()
        //    //.SetNamePattern("RouteQualityTracker Device")
        //    .SetScanFilter(new ScanFilter
        //        .Builder()
        //        .SetServiceUuid(new ParcelUuid(new UUID(0x1801L, -1L)))!
        //        .Build()
        //    )
        //    .Build();

        //var pairingRequest = new AssociationRequest.Builder()
        //    .AddDeviceFilter(deviceFilter)
        //    .SetSingleDevice(true)
        //    .Build();

        //var deviceManager = (CompanionDeviceManager)GetSystemService(CompanionDeviceService)!;



        //if (OperatingSystem.IsAndroidVersionAtLeast(33))
        //{


        //    deviceManager.Associate(pairingRequest, DirectExecutor.Instance, new CompanionDeviceManager.Callback()
        //}
        ////deviceManager.Associate()

        if (OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            StartForeground(NotificationId, notification, ForegroundService.TypeConnectedDevice);
        }
        else
        {
            StartForeground(NotificationId, notification);
        }

        _qualityTrackerService.OnRouteQualityChanged += _androidNotificationService.HandleRouteQualityChangeEvent;

        _serviceManager.SetStatus(true);

        return StartCommandResult.NotSticky;
    }

    class BleScanCallback(IServiceManager serviceManager, Context context) : ScanCallback
    {
        private BluetoothGatt? _bluetoothGatt;

        public void Disconnect()
        {
            _bluetoothGatt?.Disconnect();
            _bluetoothGatt = null;
        }

        public override void OnBatchScanResults(IList<ScanResult>? results)
        {
            base.OnBatchScanResults(results);
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult? result)
        {
            if (result is null || result.Device is null)
            {
                serviceManager.DisplayMessage("No device was found");
                serviceManager.SetStatus(false, new InvalidOperationException("No device was found"));
            }

            _bluetoothGatt = result!.Device!.ConnectGatt(context, true, new CustomBluetoothGattCallback());
            if (_bluetoothGatt is null)
            {
                serviceManager.SetStatus(false, new InvalidOperationException($"Unable to connect to {result.Device.Name}"));
            }
        }

        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            serviceManager.DisplayMessage($"Bluetooth scan failed: {errorCode}");
            base.OnScanFailed(errorCode);
        }
    }

    class CustomBluetoothGattCallback : BluetoothGattCallback
    {
        public override void OnConnectionStateChange(BluetoothGatt? gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            //connectionstatus
            base.OnConnectionStateChange(gatt, status, newState);
        }
    }

}
