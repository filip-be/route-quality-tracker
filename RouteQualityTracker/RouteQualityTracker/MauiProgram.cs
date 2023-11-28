﻿using Microsoft.Extensions.Logging;
using RouteQualityTracker.Interfaces;
using RouteQualityTracker.Services;

namespace RouteQualityTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<IServiceManager, ServiceManager>();

#if ANDROID
        builder.Services.AddSingleton<MainActivity>();
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        ServiceHelper.Initialize(app.Services);

        return app;
    }
}
