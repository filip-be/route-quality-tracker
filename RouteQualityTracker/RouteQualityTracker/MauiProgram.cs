using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;
using RouteQualityTracker.Pages;
using RouteQualityTracker.Services;

namespace RouteQualityTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<AppSettings>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IServiceManager, ServiceManager>();
        builder.Services.AddSingleton<IActivityIntegrationService, ActivityIntegrationService>();
        builder.Services.AddSingleton<IQualityTrackingService, QualityTrackingService>();

        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<ITrackAnalyzer, TrackAnalyzer>();

#if ANDROID
        builder.Services.AddSingleton<MainActivity>();
        builder.Services.AddScoped<RouteQualityTracker.Platforms.Android.IAndroidNotificationService, 
            RouteQualityTracker.Platforms.Android.AndroidNotificationService>();
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        ServiceHelper.Initialize(app.Services);

        return app;
    }
}
