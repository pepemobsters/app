﻿using BTCPayApp.Core;
using BTCPayApp.Core.Contracts;
using BTCPayApp.Maui.Services;
using BTCPayApp.UI;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace BTCPayApp.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddBTCPayAppUIServices();
        builder.Services.ConfigureBTCPayAppCore();
        builder.Services.AddLogging();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<IDataDirectoryProvider, XamarinDataDirectoryProvider>();
        builder.Services.AddSingleton<IConfigProvider, XamarinEssentialsConfigProvider>();
        builder.Services.AddSingleton<ISecureConfigProvider, XamarinEssentialsSecureConfigProvider>();
        builder.Services.AddSingleton<ISystemThemeProvider, XamarinSystemThemeProvider>();
        builder.ConfigureLifecycleEvents(events =>
        {
            // https://learn.microsoft.com/de-de/dotnet/maui/fundamentals/app-lifecycle#platform-lifecycle-events
#if ANDROID
            events.AddAndroid(android => android
                .OnStart((activity) => LogEvent(nameof(AndroidLifecycle.OnStart)))
                .OnCreate((activity, bundle) => LogEvent(nameof(AndroidLifecycle.OnCreate)))
                .OnStop((activity) => LogEvent(nameof(AndroidLifecycle.OnStop))));
#endif
#if IOS
                events.AddiOS(ios => ios
                    .OnActivated((app) => LogEvent(nameof(iOSLifecycle.OnActivated)))
                    .OnResignActivation((app) => LogEvent(nameof(iOSLifecycle.OnResignActivation)))
                    .DidEnterBackground((app) => LogEvent(nameof(iOSLifecycle.DidEnterBackground)))
                    .WillTerminate((app) => LogEvent(nameof(iOSLifecycle.WillTerminate))));
#endif
            static bool LogEvent(string eventName, string type = null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Lifecycle event: {eventName}{(type == null ? string.Empty : $" ({type})")}");
                return true;
            }
        });
        return builder.Build();
    }
}
