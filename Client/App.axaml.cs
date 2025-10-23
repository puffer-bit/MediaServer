using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Client.Services.Other;
using Client.Services.Other.AppInfrastructure;
using Client.Services.Other.AudioPlayerService;
using Client.Services.Other.FrameProcessor;
using Client.Services.Other.ScreenCastService;
using Client.Services.Other.ScreenCastService.XdgDesktopPortalClient;
using Client.Services.Server.Coordinator;
using Client.Services.Server.Factories.CoordinatorFactory;
using Client.Services.Server.Factories.VideoSessionFactory;
using Client.ViewModels.MainWindow;
using Client.ViewModels.MainWindow.ConnectWindow;
using Client.Views;
using Microsoft.Extensions.DependencyInjection;
using Client.Services.Other.ScreenCastService.Linux.PipeWireService;
using Client.Services.Other.ScreenCastService.Windows.D3D11ScreenCaptureSrcService;
using Client.Services.Other.ScreenCastService.Windows.Win32PortalClient;
using System.Runtime.InteropServices;
using Avalonia.Styling;

namespace Client;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        if (Debugger.IsAttached)
        {
            //this.AttachDevTools();
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // Disable light theme
        Application.Current!.ActualThemeVariantChanged += (_, _) => {
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        };
        services.AddSingleton<ClientTools>();
        services.AddSingleton<AppInitializer>();
        services.AddSingleton<ICoordinatorFactory, CoordinatorFactory>();
        services.AddSingleton<IVideoSessionFactory, VideoSessionFactory>();
        services.AddSingleton<AppSettingsManager>();


        services.AddTransient<IFrameProcessor, FrameProcessor>();
        services.AddTransient<ICoordinatorSession, CoordinatorSession>();
        services.AddTransient<IAudioPlayerService, AudioPlayerService>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ConnectWindowViewModel>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            services.AddTransient<IGStreamerService, PipeWireService>();
            services.AddTransient<IScreenCastClient, XdgDesktopPortalClient>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.AddTransient<IGStreamerService, WindowsScreenCaptureService>();
            services.AddTransient<IScreenCastClient, WindowsPortalClient>();
            services.AddTransient<NativeWindowHelper>();
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported OS platform");
        }

        Services = services.BuildServiceProvider();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>(),
            };
            desktop.Exit += (_, _) =>
            {
                Services.GetRequiredService<AppSettingsManager>().SaveSettings();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
