using System.IO;
using System.Text.Json;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using RouteQualityTracker.Core.Gpx;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Pages;

public partial class TrackOperationsPage : ContentPage
{
    private readonly ITrackAnalyzer _trackAnalyzer;

    private IList<RouteQualityRecord>? _routeQualityRecords;
    private FileResult? _gpxFile;
    private GpxData? _gpxData;
    private SemaphoreSlim _updateProgressBarSemaphore = new (1, 1);

    public TrackOperationsPage()
    {
        InitializeComponent();
        _trackAnalyzer = ServiceHelper.GetService<ITrackAnalyzer>();
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

            logEditor.Text = $"Loaded {_routeQualityRecords.Count} records from {file.FileName}";

            loadGpxBtn.IsEnabled = true;
            loadRouteQualityBtn.IsEnabled = false;
        }
        catch (Exception ex)
        {
            await Toast.Make($"Error loading file: {ex.Message}").Show();
            logEditor.Text += $"{Environment.NewLine}Error: {ex}";
        }
    }

    private async void OnLoadGpx(object sender, EventArgs e)
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
            
            logEditor.Text += $"{Environment.NewLine}Loaded gpx file {file.FileName}";

            processFilesBtn.IsEnabled = true;
            loadGpxBtn.IsEnabled = false;
        }
        catch (Exception ex)
        {
            await Toast.Make($"Error loading file: {ex.Message}").Show();
            logEditor.Text += $"{Environment.NewLine}Error: {ex}";
        }
    }
    private async void OnProcessFiles(object sender, EventArgs e)
    {
        try
        {
            await using var fileStream = await _gpxFile!.OpenReadAsync();

            logEditor.Text += $"{Environment.NewLine}Processing track data...";
            saveTrackBtn.IsEnabled = false;
            resetBtn.IsEnabled = false;

            logEditor.IsVisible = false;
            progressBar.Progress = 0;
            progressBar.IsVisible = true;
            processingLabel.IsVisible = true;
            processFilesBtn.IsEnabled = false;

            _gpxData = await _trackAnalyzer.MarkupTrack(fileStream, _routeQualityRecords!, UpdateProgressAction);

            progressBar.IsVisible = false;
            processingLabel.IsVisible = false;
            logEditor.IsVisible = true;
            logEditor.Text += $"{Environment.NewLine}Processed files. Found {_gpxData.Tracks.Count} track segments";

            saveTrackBtn.IsEnabled = true;
            resetBtn.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await Toast.Make($"Error processing track: {ex.Message}").Show();

            progressBar.IsVisible = false;
            processingLabel.IsVisible = false;
            logEditor.IsVisible = true;
            logEditor.Text += $"{Environment.NewLine}Error: {ex}";

            processFilesBtn.IsEnabled = true;
            saveTrackBtn.IsEnabled = true;
            resetBtn.IsEnabled = true;
        }
    }

    private async Task UpdateProgressAction(double currentProgress)
    {
        try
        {
            await _updateProgressBarSemaphore.WaitAsync();
            await progressBar.ProgressTo(currentProgress, 100, Easing.Linear);
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
                logEditor.Text +=
                    $"{Environment.NewLine}The file was saved successfully to location: {fileSaverResult.FilePath}";
            }
            else
            {
                await Toast.Make($"Error saving file: {fileSaverResult.Exception.Message}").Show();
                logEditor.Text += $"{Environment.NewLine}Error: {fileSaverResult.Exception}";
            }
        }
        catch (Exception ex)
        {
            await Toast.Make($"Error saving file: {ex.Message}").Show();
            logEditor.Text += $"{Environment.NewLine}Error: {ex}";
        }
    }

    private void OnReset(object sender, EventArgs e)
    {
        logEditor.Text = String.Empty;

        loadRouteQualityBtn.IsEnabled = true;
        loadGpxBtn.IsEnabled = false;
        processFilesBtn.IsEnabled = false;
        saveTrackBtn.IsEnabled = false;

        _routeQualityRecords = null;
        _gpxData = null;
        _gpxFile = null;
    }
}