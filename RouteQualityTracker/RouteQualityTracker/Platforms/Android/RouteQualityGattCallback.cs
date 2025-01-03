using Android.Bluetooth;
using Java.Util;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;
using InvalidOperationException = System.InvalidOperationException;

namespace RouteQualityTracker.Platforms.Android;

public class RouteQualityGattCallback : BluetoothGattCallback
{
    private readonly IServiceManager _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
    private readonly IQualityTrackingService _qualityTrackingService = ServiceHelper.GetService<IQualityTrackingService>();
    private readonly ILoggingService _loggingService = ServiceHelper.GetService<ILoggingService>();

    private const long PositionQualityCharacteristicUuuid = 0x2A6900001000;

    public override void OnServicesDiscovered(BluetoothGatt? gatt, GattStatus status)
    {
        _loggingService.LogDebugMessage($"OnServicesDiscovered: {status}");
        if (status == GattStatus.Success)
        {
            _loggingService.LogDebugMessage("GattStatus is Success");
            if (gatt is null)
            {
                _serviceManager.SetStatus(false, new InvalidOperationException("Connection succeeded but Bluetooth service is empty"));
                return;
            }

            if (gatt.Services?.Any() != true)
            {
                gatt.Disconnect();
                _serviceManager.SetStatus(false, new InvalidOperationException("There are no services available on remote device"));
                return;
            }

            var allCharacteristics = gatt.Services
                .SelectMany(s => s.Characteristics ?? new List<BluetoothGattCharacteristic>())
                .ToList();

            if (allCharacteristics.Count == 0)
            {
                gatt.Disconnect();
                _serviceManager.SetStatus(false, new InvalidOperationException("There are no characteristics available on remote device"));
                return;
            }

            _loggingService.LogDebugMessage($"Gatt characteristics: {string.Join(Environment.NewLine, allCharacteristics.Select(c => c.Uuid))}");
       
            var qualityCharacteristic = allCharacteristics.Find(c => c.Uuid?.MostSignificantBits == PositionQualityCharacteristicUuuid);
            if (qualityCharacteristic is null)
            {
                gatt.Disconnect();
                _serviceManager.SetStatus(false, new InvalidOperationException("Remote device does not have position characteristic defined"));
                return;
            }

            var result = gatt.SetCharacteristicNotification(qualityCharacteristic, true);
            _loggingService.LogDebugMessage($"Characteristic {qualityCharacteristic.Uuid?.MostSignificantBits:X} notification set: {result}");

            qualityCharacteristic.Descriptors
                ?.ToList()
                .ForEach(desc => _loggingService.LogDebugMessage($"Descriptor UUID: {desc.Uuid}"));

            var configDescriptorUuid = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
            var desc = qualityCharacteristic.GetDescriptor(configDescriptorUuid);
            if (desc is null)
            {
                _serviceManager.SetStatus(false, new InvalidOperationException("Config descriptor not available for characteristic"));
                return;
            }

            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                gatt.WriteDescriptor(desc, BluetoothGattDescriptor.EnableNotificationValue!.ToArray());
            }
            else
            {
                desc.SetValue(BluetoothGattDescriptor.EnableNotificationValue!.ToArray());
                gatt.WriteDescriptor(desc);
            }

            return;
        }

        base.OnServicesDiscovered(gatt, status);
    }

    public override void OnConnectionStateChange(BluetoothGatt? gatt, GattStatus status, ProfileState newState)
    {
        _loggingService.LogDebugMessage($"OnConnectionStateChange: {status}");
        if (status == GattStatus.Success)
        {
            if (gatt is null)
            {
                _serviceManager.SetStatus(false, new InvalidOperationException("Connection succeeded but Bluetooth service is empty"));
                return;
            }

            if (!gatt.DiscoverServices())
            {
                _serviceManager.SetStatus(false, new InvalidOperationException("Unable to discover services"));
                return;
            }

            return;
        }

        base.OnConnectionStateChange(gatt, status, newState);
    }
    
    public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, byte[] value)
    {
        if (value.Length == 0)
        {
            _serviceManager.SetStatus(false,
                new InvalidOperationException("Empty route quality received from device"));
            return;
        }

        var characteristicValue = value[0];
        _loggingService.LogDebugMessage($"Received {characteristic.Uuid}: {characteristicValue:X}");

        if (!Enum.TryParse(characteristicValue.ToString(), out RouteQualityEnum routeQuality))
        {
            _serviceManager.SetStatus(false,
                new InvalidOperationException("Invalid route quality received from device"));
            return;
        }

        _qualityTrackingService.SetRouteQuality(routeQuality);
    }

    public override void OnCharacteristicChanged(BluetoothGatt? gatt, BluetoothGattCharacteristic? characteristic)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            _loggingService.LogDebugMessage("Charactericts received, but android version is higher than 33");
            return;
        }

        if (characteristic is null)
        {
            _loggingService.LogDebugMessage("Received empty characteristic!");
            return;
        }

        var characteristicValue = characteristic.GetIntValue(GattFormat.Sint32, 0)!;
        _loggingService.LogDebugMessage($"Received {characteristic.Uuid}: {characteristicValue}");

        if (!Enum.TryParse(characteristicValue.ToString(), out RouteQualityEnum routeQuality))
        {
            _serviceManager.SetStatus(false,
                new InvalidOperationException("Invalid route quality received from device"));
            return;
        }

        _qualityTrackingService.SetRouteQuality(routeQuality);
    }
}