using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker;

public partial class MainPage : ContentPage
{
    private readonly IServiceManager _serviceManager;
    private readonly IQualityTrackingService _qualityTrackingService;
    private int _count;

    public MainPage(IServiceManager serviceManager, IQualityTrackingService qualityTrackingService)
    {
        InitializeComponent();

        _serviceManager = serviceManager;
        _qualityTrackingService = qualityTrackingService;
        _serviceManager.OnServiceStarted += OnServiceStarted;
        _serviceManager.OnServiceStopped += OnServiceStopped;
        _serviceManager.OnServiceStartError += OnServiceStartError;
    }

    private void OnServiceStartError(object? sender, Exception ex)
    {
        ToggleServiceBtn.Text = "Start service";
        Toast.Make($"Error starting service: {ex.Message}", ToastDuration.Long);
    }

    private void OnServiceStopped(object? sender, EventArgs e)
    {
        ToggleServiceBtn.Text = "Start service";
        _qualityTrackingService.StopTracking();
        Toast.Make("Service stopped");
    }

    private void OnServiceStarted(object? sender, EventArgs e)
    {
        ToggleServiceBtn.Text = "Stop service";
        _qualityTrackingService.StartTracking();
        Toast.Make("Service started");
    }

    private void OnToggleServiceClicked(object sender, EventArgs e)
    {
        _serviceManager.ToggleService();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        _count++;

        if (_count == 1)
            CounterBtn.Text = $"Clicked {_count} time";
        else
            CounterBtn.Text = $"Clicked {_count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
