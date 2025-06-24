using System;
using System.IO;
using Avalonia;
using Avalonia.ReactiveUI;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;
using MiniPipeline.CefGlue;
using MiniPipeline.Core;
using System.Diagnostics;
using System.Threading;

namespace AvaloniaDemo.Desktop;

class Program
{


    static string? rootCachePath;
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        //Issue with MAC when lunch app , the current directory must be set
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        var localPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        rootCachePath = Path.Combine(localPath, "TempCefRootApp" );
        var cachePath= Path.Combine(rootCachePath,"Cache");
        if (Directory.Exists(rootCachePath))
        {
            Cleanup(rootCachePath);
            Thread.Sleep(1000);
        }
        Directory.CreateDirectory(rootCachePath);
        Directory.CreateDirectory(cachePath);

        //default app://local
        //PipelineCefConfig.Scheme = "https";
        //PipelineCefConfig.Host =    "localhost";

        var app = StartUp.Build();

        AppDomain.CurrentDomain.ProcessExit += delegate { 
            app.CefShutdown();
            Cleanup(rootCachePath);
        };

        var setting = new CefSettings()
        {
            RootCachePath = rootCachePath,
            CachePath = rootCachePath,
            WindowlessRenderingEnabled = false,
            PersistSessionCookies = true,
            //cefglue does not setup the folder locales files
            //on mac you need copied the locales files with your own desire folder

            LocalesDirPath = Path.Combine(AppContext.BaseDirectory, @"runtimes\win-x64\native\locales"),
            CookieableSchemesList = PipelineCefConfig.Scheme 
        };




        BuildAvaloniaApp()
        .AfterSetup(o =>
        {
            app.CefInitSchemeHandler(setting);

        }).StartWithClassicDesktopLifetime(args);

        return 0;
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();


    private static void Cleanup(string cachePath)
    {
        if (string.IsNullOrWhiteSpace(cachePath) || !Directory.Exists(cachePath))
            return;

        const int maxRetries = 5;
        const int delayBetweenRetries = 100; // milliseconds

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var dirInfo = new DirectoryInfo(cachePath);
                dirInfo.Attributes = FileAttributes.Normal;

                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    file.Attributes = FileAttributes.Normal;
                }

                dirInfo.Delete(true);
                Debug.WriteLine("Cache directory deleted successfully.");
                return;
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"[Attempt {attempt + 1}] IO Exception: {ex.Message}");
                Thread.Sleep(delayBetweenRetries);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"[Attempt {attempt + 1}] Unauthorized: {ex.Message}");
                Thread.Sleep(delayBetweenRetries);
            }
        }

    }




}
