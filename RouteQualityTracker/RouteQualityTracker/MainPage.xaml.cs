using RouteQualityTracker.Interfaces;

namespace RouteQualityTracker;

public partial class MainPage : ContentPage, ITrackingButtonsHandler
{
    private readonly IServiceManager _foregroundService;
    private int _count;

    public MainPage(IServiceManager foregroundService)
    {
        _foregroundService = foregroundService;
        InitializeComponent();
    }

    private void OnToggleServiceClicked(object sender, EventArgs e)
    {
        var isRunning = _foregroundService.ToggleService();

        ToggleServiceBtn.Text = isRunning ? "Stop service" : "Start service";
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

    public void OnButtonClick(string text)
    {
        CounterBtn.Text = text;
    }
}
