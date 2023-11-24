using RouteQualityTracker.Interfaces;

namespace RouteQualityTracker;

public partial class MainPage : ContentPage, ITrackingButtonsHandler
{
    private readonly IForegroundService _foregroundService;
    private int _count;

    public MainPage(IForegroundService foregroundService)
    {
        _foregroundService = foregroundService;
        InitializeComponent();
    }

    private void OnToggleServiceClicked(object sender, EventArgs e)
    {
        var isRunning = _foregroundService.ToggleService();

        ToggleServiceBtn.Text = isRunning ? "Start service" : "Stop service";
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
