﻿using Android.Bluetooth;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;
using InvalidOperationException = System.InvalidOperationException;

namespace RouteQualityTracker.Platforms.Android;

public class RouteQualityGattCallback : BluetoothGattCallback
{
    private readonly IServiceManager _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
    private readonly IQualityTrackingService _qualityTrackingService = ServiceHelper.GetService<IQualityTrackingService>();

    public override void OnServicesDiscovered(BluetoothGatt? gatt, GattStatus status)
    {
        if (status == GattStatus.Success)
        {
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

            Console.WriteLine($"Characteristics: {string.Join(Environment.NewLine, allCharacteristics.Select(c => c.Uuid))}");
       
            var qualityCharacteristic = allCharacteristics.Find(c => c.Uuid?.MostSignificantBits == 0x2A6900001000);
            if (qualityCharacteristic is null)
            {
                gatt.Disconnect();
                _serviceManager.SetStatus(false, new InvalidOperationException("Remote device does not have position characteristic defined"));
                return;
            }

            gatt.SetCharacteristicNotification(qualityCharacteristic, true);

            return;
        }

        base.OnServicesDiscovered(gatt, status);
    }

    public override void OnConnectionStateChange(BluetoothGatt? gatt, GattStatus status, ProfileState newState)
    {
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
        Console.WriteLine($"Received {characteristic.Uuid}: {characteristicValue}");

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
            return;
        }

        if (characteristic is null)
        {
            Console.WriteLine("Received empty characteristic!");
            return;
        }

        var characteristicValue = characteristic.GetIntValue(GattFormat.Sint32, 0);
        Console.WriteLine($"Received {characteristic.Uuid}: {characteristicValue}");
    }
}