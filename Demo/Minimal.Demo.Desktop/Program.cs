using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using Xilium.CefGlue;
using MiniPipeline.CefGlue;

namespace Minimal.BrowserUI.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) { 



        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        var localPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        var rootCachePath = Path.Combine(localPath, "MinimalDemoTemp" );
        var cachePath= Path.Combine(rootCachePath,"cache");

        if (Directory.Exists(rootCachePath))
        {
            Cleanup(rootCachePath);
            Thread.Sleep(1000);
        }

        Directory.CreateDirectory(rootCachePath);
        Directory.CreateDirectory(cachePath);

        var  app = StartUp.BuildBlazorWebAssembly();

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
            LocalesDirPath = Path.Combine(AppContext.BaseDirectory, @"runtimes\win-x64\native\locales"),
            CookieableSchemesList = "https"
             
        };        

        
        //var cefFlags = new Dictionary<string, string>();
        //    cefFlags.Add("ignore-certificate-errors", "1");
        //    cefFlags.Add("allow-insecure-localhost", "1");
        ////flags:cefFlags.ToArray(),
        //flags:new KeyValuePair<string,string>[] {cefFlags.First() }


        BuildAvaloniaApp()
        .AfterSetup(o =>
        {
            app.CefInitSchemeHandler(setting);
        })
        .StartWithClassicDesktopLifetime(args);
    }

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



    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
