using System.Text.Json;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using RouteQualityTracker.Core.Gpx;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;
using RouteQualityTracker.Services;

namespace RouteQualityTracker.Pages;

public partial class TrackOperationsPage : ContentPage
{
    private readonly ITrackAnalyzer _trackAnalyzer;
    private readonly ISettingsService _settingsService;

    private IList<RouteQualityRecord>? _routeQualityRecords;
    private FileResult? _gpxFile;
    private GpxData? _gpxData;
    private readonly SemaphoreSlim _updateProgressBarSemaphore = new (1, 1);

    public TrackOperationsPage()
    {
        InitializeComponent();
        _trackAnalyzer = ServiceHelper.GetService<ITrackAnalyzer>();
        _settingsService = ServiceHelper.GetService<ISettingsService>();
    }

    private async void OnLoadRouteQuality(object sender, EventArgs e)
    {
        try
        {
            var fileTypes = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "*/*" } },
                    { DevicePlatform.Android, new[] { "*/*" } },
                    { DevicePlatform.WinUI, new[] { "*/*" } },
                    { DevicePlatform.Tizen, new[] { "*/*" } },
                    { DevicePlatform.macOS, new[] { "*/*" } }
                });

            var options = new PickOptions
            {
                PickerTitle = "Select Route Quality data",
                FileTypes = fileTypes
            };

            var file = await FilePicker.PickAsync(options);
            if(file == null) return;

            await using var stream = await file.OpenReadAsync();

            _routeQualityRecords = JsonSerializer.Deserialize<IList<RouteQualityRecord>>(stream)!;

            LogEditor.Text = $"Loaded {_routeQualityRecords.Count} records from {file.FileName}";

            LoadGpxBtn.IsEnabled = true;
            LoadRouteQualityBtn.IsEnabled = false;
        }
        catch (Exception ex)
        {
            await Toast.Make($"Error loading file: {ex.Message}").Show();
            LogEditor.Text += $"{Environment.NewLine}Error: {ex}";
        }
    }

    private async void OnLoadGpx(object sender, EventArgs e)
    {
        if (_settingsService.Settings.ImportDataFromFile)
        {
            await LoadGpxFile();
            return;
        }

        if (_settingsService.Settings.ImportFromStrava)
        {
            await LoadGpxFromStrava();
            return;
        }

        await Toast.Make("Data import settings are not defined.").Show();
    }

    private static Task LoadGpxFromStrava()
    {
        return Toast.Make("Not implemented yet").Show();
    }

    private async Task LoadGpxFile()
    {
        try
        {
            var fileTypes = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "*.*" } },
                    { DevicePlatform.Android, new[] { "*/*" } },
                    { DevicePlatform.WinUI, new[] { "*.*" } },
                    { DevicePlatform.Tizen, new[] { "*.*" } },
                    { DevicePlatform.macOS, new[] { "*.*" } }
                });

            var options = new PickOptions
            {
                PickerTitle = "Select corresponding GPX track",
                FileTypes = fileTypes
            };

            var file = await FilePicker.PickAsync(options);
            if (file == null) return;

            await using var stream = await file.OpenReadAsync();
            if (!await GpxData.CanRead(stream))
            {
                await Toast.Make($"Unable to read GPX file {file.FileName}").Show();
                return;
            }

            _gpxFile = file;
            
            LogEditor.Text += $"{Environment.NewLine}Loaded gpx file {file.FileName}";

            ProcessFilesBtn.IsEnabled = true;
            LoadGpxBtn.IsEnabled = false;
        }
        catch (Exception ex)
        {
            await Toast.Make($"Error loading file: {ex.Message}").Show();
            LogEditor.Text += $"{Environment.NewLine}Error: {ex}";
        }
    }

    private async void OnProcessFiles(object sender, EventArgs e)
    {
        try
        {
            await using var fileStream = await _gpxFile!.OpenReadAsync();

            LogEditor.Text += $"{Environment.NewLine}Processing track data...";
            SaveTrackBtn.IsEnabled = false;
            ResetBtn.IsEnabled = false;

            LogEditor.IsVisible = false;
            ProgressBar.Progress = 0;
            ProgressBar.IsVisible = true;
            ProcessingLabel.IsVisible = true;
            ProcessFilesBtn.IsEnabled = false;

            _gpxData = await _trackAnalyzer.MarkupTrack(fileStream, _routeQualityRecords!, UpdateProgressAction);

            ProgressBar.IsVisible = false;
            ProcessingLabel.IsVisible = false;
            LogEditor.IsVisible = true;
            LogEditor.Text += $"{Environment.NewLine}Processed files. Found {_gpxData.Tracks.Count} track segments";

            SaveTrackBtn.IsEnabled = true;
            ResetBtn.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await Toast.Make($"Error processing track: {ex.Message}").Show();

            ProgressBar.IsVisible = false;
            ProcessingLabel.IsVisible = false;
            LogEditor.IsVisible = true;
            LogEditor.Text += $"{Environment.NewLine}Error: {ex}";

            ProcessFilesBtn.IsEnabled = true;
            SaveTrackBtn.IsEnabled = true;
            ResetBtn.IsEnabled = true;
        }
    }

    private async Task UpdateProgressAction(double currentProgress)
    {
        try
        {
            await _updateProgressBarSemaphore.WaitAsync();
            await ProgressBar.ProgressTo(currentProgress, 100, Easing.Linear);
        }
        finally
        {
            _updateProgressBarSemaphore.Release();
        }
    }

    private async void OnSaveTrack(object sender, EventArgs e)
    {
        try
        {
            var fileName = $"{Path.GetFileNameWithoutExtension(_gpxFile!.FileName)}-TrackQuality.gpx";

            using var stream = new MemoryStream();
            await _gpxData!.Save(stream);

            stream.Position = 0;
            var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream);
            if (fileSaverResult.IsSuccessful)
            {
                await Toast.Make("Successfully save track with route quality").Show();
                LogEditor.Text +=
                    $"{Environment.NewLine}The file was saved successfully to location: {fileSaverResult.FilePath}";
            }
            else
            {
                await Toast.Make($"Error saving file: {fileSaverResult.Exception.Message}").Show();
                LogEditor.Text += $"{Environment.NewLine}Error: {fileSaverResult.Exception}";
            }
        }
        catch (Exception ex)
        {
            await Toast.Make($"Error saving file: {ex.Message}").Show();
            LogEditor.Text += $"{Environment.NewLine}Error: {ex}";
        }
    }

    private void OnReset(object sender, EventArgs e)
    {
        LogEditor.Text = string.Empty;

        LoadRouteQualityBtn.IsEnabled = true;
        LoadGpxBtn.IsEnabled = false;
        ProcessFilesBtn.IsEnabled = false;
        SaveTrackBtn.IsEnabled = false;

        _routeQualityRecords = null;
        _gpxData = null;
        _gpxFile = null;
    }
}