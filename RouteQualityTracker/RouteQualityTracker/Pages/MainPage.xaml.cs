﻿using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;
using System.Text;
using System.Text.Json;

namespace RouteQualityTracker.Pages;

public partial class MainPage : ContentPage
{
    private readonly IServiceManager _serviceManager;
    private readonly IQualityTrackingService _qualityTrackingService;
    private readonly ILoggingService _loggingService;

    public MainPage()
    {
        InitializeComponent();

        _serviceManager = ServiceHelper.GetService<IServiceManager>();
        _qualityTrackingService = ServiceHelper.GetService<IQualityTrackingService>();
        _loggingService = ServiceHelper.GetService<ILoggingService>();

        _serviceManager.OnServiceStarted += OnServiceStarted;
        _serviceManager.OnServiceStopped += OnServiceStopped;
        _serviceManager.OnServiceStartError += OnServiceStartError;
        _serviceManager.OnDisplayMessage += OnDisplayMessage;

        _loggingService.OnLogDebugMessage += (sender, message) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DebugEditor.Text += $"{message}{Environment.NewLine}";
            });
        };
    }

    private void OnDisplayMessage(object? _, string message)
    {
        MainThread.BeginInvokeOnMainThread(() => Toast.Make(message).Show());
    }

    private void OnServiceStartError(object? _, Exception ex)
    {
        MainThread.BeginInvokeOnMainThread(() =>
            Toast.Make($"Error starting service: {ex.Message}", ToastDuration.Long).Show());

    }

    private void OnServiceStopped(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ToggleServiceBtn.IsEnabled = true;

            ToggleServiceBtn.Text = "Start service";
            _qualityTrackingService.StopTracking();

            SaveDataBtn.IsEnabled = true;
            Toast.Make("Service stopped").Show();
        });
    }

    private void OnServiceStarted(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ToggleServiceBtn.IsEnabled = true;

            ToggleServiceBtn.Text = "Stop service";
            _qualityTrackingService.StartTracking();

            SaveDataBtn.IsEnabled = false;
            Toast.Make("Service started").Show();
        });
    }

    private void OnToggleServiceClicked(object sender, EventArgs e)
    {
        _serviceManager.ToggleService();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var routeQualityRecords = _qualityTrackingService.GetRouteQualityRecords();

        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(routeQualityRecords, options);

        var fileName = $"RouteQuality-{DateTime.Today.ToString("yyyy-MM-dd")}.json";
        using var stream = new MemoryStream(Encoding.Default.GetBytes(jsonString));
        var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream);
        if (fileSaverResult.IsSuccessful)
        {
            await Toast.Make($"The file was saved successfully to location: {fileSaverResult.FilePath}").Show();
        }
        else
        {
            await Toast.Make($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}").Show();
        }
    }

    private void OnClearDebugLogClicked(object sender, EventArgs e)
    {
        DebugEditor.Text = string.Empty;
    }
}
