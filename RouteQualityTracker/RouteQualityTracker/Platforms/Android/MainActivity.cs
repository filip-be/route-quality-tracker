﻿using Android.App;
using Android.Bluetooth;
using Android.Companion;
using Android.Content;
using Android.Content.PM;
using Android.Views;
using Java.Lang;
using RouteQualityTracker.Interfaces;
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
    }

    private void OnServiceStart(object? sender, EventArgs e)
    {
        var deviceManager = (CompanionDeviceManager) GetSystemService(CompanionDeviceService)!;
        
        var deviceFilter = new BluetoothLeDeviceFilter.Builder();
        var pairingRequest = new AssociationRequest.Builder()
            .AddDeviceFilter(deviceFilter.Build());

        deviceManager.Associate(pairingRequest.Build(), new BluetoothCallback((ITrackingButtonsHandler)Shell.Current.CurrentPage, this), null);
        
    }

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
        if (page is ITrackingButtonsHandler handler &&
            keyCode is >= Keycode.Num0 and <= Keycode.Num9)
        {
            handler.OnButtonClick(keyCode.ToString());
            return true;
        }

        return base.OnKeyDown(keyCode, e);
    }
}
