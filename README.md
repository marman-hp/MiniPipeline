# MiniPipeline 🚀

Run a **full ASP.NET Core app inside a desktop app — without Kestrel, without a port.**

MiniPipeline drives the real ASP.NET Core pipeline (`HttpContext` + `RequestDelegate`) straight from a Chromium (CEF) custom scheme handler, using [**CefGlue.OSR**](https://github.com/marman-hp/CefGlue.OSR). Think *Electron, but the backend is ASP.NET Core instead of Node.js*: if you know Razor Pages, MVC, minimal APIs or Blazor, you already know how to build a cross-platform desktop app.

---

## Features

- ✅ **No Kestrel, no socket** – requests go from Chromium to your `RequestDelegate` in memory, through a custom scheme (`app://local` by default).
- ✅ **The whole ASP.NET Core stack** – Razor Pages, MVC, minimal APIs, middleware, file upload, cookie auth, session, antiforgery.
- ✅ **Blazor** – Blazor Server (long polling, or WebSocket 🔥experimental) and Blazor WebAssembly.
- ✅ **Static web assets** – `wwwroot` and Razor class library `_content/` are served straight from the build, no copy step.
- ✅ **One DI container** – CefGlue.OSR registers into the same `IServiceCollection` as your app (`AddCefGlue()`).
- ✅ **Desktop UI of your choice** – the demos use Avalonia; CefGlue.OSR also has WinForms, WPF, WinUI 3 and GTK4 controls.

## Status on .NET 10 + CefGlue.OSR (CEF 154)

Checked on Windows x64:

| Sample | What was checked |
|---|---|
| ASP.NET Core (Razor Pages) | pages, CSS/JS from `wwwroot`, MiniProfiler middleware |
| Blazor Server – long polling | Counter with `ProtectedLocalStorage`, JS interop (map), `DynamicComponent`, background work + dispose |
| Blazor Server – WebSocket 🔥 | Counter over a live WebSocket circuit |
| Blazor WebAssembly | boot, Counter, `HttpClient` fetch of `sample-data/weather.json` |

Not re-tested yet on .NET 10: Linux, `dotnet publish` output, macOS.

## Upgrading from 1.0.0-rc1

2.0 is a breaking release. What changes for an app built on 1.0.0-rc1:

**Stack**

| | 1.0.0-rc1 | 2.0.0-preview.1 |
|---|---|---|
| .NET | 8 | **10** (all projects `net10.0`, ASP.NET Core 10 packages) |
| CEF binding | OutSystems `CefGlue.Common` / `CefGlue.Avalonia` 120 | **CefGlue.OSR** `CefGlue.OSR.Common` / `CefGlue.OSR.Avalonia` 154 (see [Requirements](#requirements)) |
| Avalonia | 11 | **12** |

**Code**

- `AvaloniaCefBrowser` → `CefGlueBrowser`. It is not a control itself any more: put `browser.Control` in your layout.
- Register CefGlue in your app's container and create browsers from it (see [Quick Start](#quick-start)):

  ```csharp
  builder.Services.AddCefGlue(cef => cef.UseAvalonia());                                  // startup
  var browser = PipelineCefConfig.Services!.GetRequiredService<CefGlueHost>().CreateBrowser(); // view
  ```
- `browser.Settings` is gone – pass the settings when the browser is created:

  ```csharp
  host.CreateBrowser(new CefGlueBrowserOptions
  {
      SetupCefBrowserSettings = () => new CefBrowserSettings { DefaultEncoding = "utf-8" },
  });
  ```
- CEF 154 handler signatures: `LifeSpanHandler.OnBeforePopup` gets an `int popupId` after `frame`; `DownloadHandler.OnBeforeDownload` returns `bool` (`true` when you handled it).
- `CefInitSchemeHandler(settings, cefArgs)` now passes `cefArgs` to CEF as command-line flags (1.0 ignored them).

**Blazor**

- Add `<RequiresAspNetWebAssets>true</RequiresAspNetWebAssets>` to the desktop project and use this repo's `Directory.Build.targets`, or `blazor.web.js` is missing and nothing is interactive (see [Blazor](#blazor)).
- WebSocket mode: set `browser.PermissionHandler = new PipelinePermissionHandler();` on every browser, or the WebSocket never connects (see [WebSocket Support](#websocket-support-experimental)).

**Avalonia 12 side effects** (only if your app uses these packages)

- `Avalonia.ReactiveUI` → `ReactiveUI.Avalonia` (namespace `ReactiveUI.Avalonia`); `.UseReactiveUI()` now takes a configuration callback, e.g. `.UseReactiveUI(_ => { })`. ReactiveUI's `Unit` became `ReactiveUI.Primitives.RxVoid`, and `System.Reactive` is a separate package reference.
- `Avalonia.Xaml.Interactivity` → `Xaml.Behaviors.Avalonia`; same namespace, but the XAML `assembly=` is now `Xaml.Behaviors.Interactivity`.
- Avalonia 12 compiles bindings by default; XAML written without `x:DataType` needs `<AvaloniaUseCompiledBindingsByDefault>false</AvaloniaUseCompiledBindingsByDefault>`.
- `Avalonia.Diagnostics` has no Avalonia 12 version; remove it.

## Requirements

- .NET 10 SDK, x64 (tested on Windows)
- **[CefGlue.OSR](https://github.com/marman-hp/CefGlue.OSR)** – the CEF binding MiniPipeline runs on. It is **not on nuget.org yet**, so:
  - its NuGet packages ship in this repo, in `localnuget\` (`CefGlue.OSR.Common` and `CefGlue.OSR.Avalonia` 154.8037.94); `nuget.config` restores `CefGlue.OSR.*` from there and everything else from nuget.org. Nothing to build for that.
  - **the CEF binaries are not in those packages – you have to download them yourself** (next section).

### Download CEF (required, manual)

MiniPipeline needs the CEF runtime CefGlue.OSR is built for: **CEF 154.0.33** (Chromium 154.0.8037.94), Windows x64 *minimal* distribution. The desktop demos copy it next to their exe from a CefGlue.OSR checkout **next to** this repo (`..\CefGlue.OSR`):

```powershell
git clone https://github.com/marman-hp/CefGlue.OSR ..\CefGlue.OSR
cd ..\CefGlue.OSR
powershell -ExecutionPolicy Bypass -File .\get-cef.ps1
```

`get-cef.ps1` downloads that exact CEF build from the official [CEF builds](https://cef-builds.spotifycdn.com/index.html) site and unpacks it into `CefGlue.OSR\cef\runtimes\win-x64\native\`. Prefer to download it yourself? Get [cef_binary_154.0.33+ga03e714+chromium-154.0.8037.94_windows64_minimal.tar.bz2](https://cef-builds.spotifycdn.com/cef_binary_154.0.33%2Bga03e714%2Bchromium-154.0.8037.94_windows64_minimal.tar.bz2) (~165 MB), then run `.\get-cef.ps1 -FromArchive <path-to-the-archive>`.

CefGlue.OSR somewhere else? Build with `-p:CefGlueOSRDir=path\to\CefGlue.OSR\`. A build warns when it can't find the CEF runtime.

### Building CefGlue.OSR yourself

Want a newer CefGlue.OSR, or to change it? Build it from source – see its [README](https://github.com/marman-hp/CefGlue.OSR#build):

```powershell
cd ..\CefGlue.OSR
dotnet build CefGlue.OSR.sln -c Release -p:Platform=x64
```

The build packs its packages into `CefGlue.OSR\Nuget\output\`. Copy `CefGlue.OSR.Common.*.nupkg` and `CefGlue.OSR.Avalonia.*.nupkg` from there into this repo's `localnuget\` (and set `CefGlueOSRVersion` in `Directory.Build.props` if the version changed). Same version number as before? Also delete `cefglue.osr.*` from your NuGet packages folder, or the cached copy keeps being used.

## Quick Start

**1. Your ASP.NET Core startup** – the same code you would write for Kestrel:

```csharp
using MiniPipeline.Core;
using Xilium.CefGlue.Avalonia;
using Xilium.CefGlue.Common.Composition;

var builder = MiniPipelineBuilder.CreateBuilder();   // WebApplication.CreateBuilder, minus Kestrel

// CefGlue.OSR shares your container: one set of singletons, CefGlueHost resolvable anywhere.
builder.Services.AddCefGlue(cef => cef.UseAvalonia());

var app = builder.Build();
app.UseRouting();

app.MapGet("/", () => Results.Content("""
    <!DOCTYPE html>
    <html><head><title>Sample</title></head>
    <body>IT JUST WORKS!</body></html>
    """, "text/html"));

return app;
```

**2. Your Avalonia entry point** – register the scheme handler and start CEF:

```csharp
using MiniPipeline.Core;
using MiniPipeline.CefGlue;
using Xilium.CefGlue;

var app = YourStartUpCode();
var cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YourApp");

AppDomain.CurrentDomain.ProcessExit += delegate { app.CefShutdown(); };

// optional, default is app://local
PipelineCefConfig.Scheme = "app";
PipelineCefConfig.Host = "local";

var settings = new CefSettings
{
    RootCachePath = cachePath,
    CachePath = cachePath,
    PersistSessionCookies = true,   // keep ASP.NET session/auth cookies across restarts
};

BuildAvaloniaApp()
    .AfterSetup(_ => app.CefInitSchemeHandler(settings))   // optional 2nd argument: extra CEF command-line flags
    .StartWithClassicDesktopLifetime(args);
```

**3. Your browser view** – create the browser from the app's container and open `PipelineCefConfig.BaseAddress`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using MiniPipeline.Core;
using Xilium.CefGlue.Avalonia;
using Xilium.CefGlue.Common.Composition;

var host = PipelineCefConfig.Services!.GetRequiredService<CefGlueHost>();
var browser = host.CreateBrowser();
browser.Address = PipelineCefConfig.BaseAddress;   // e.g. app://local
browserWrapper.Child = browser.Control;
```

## Project setup

No project template yet. Start from an Avalonia app and change the project file:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">   <!-- Razor SDK: Razor Pages/Blazor, and no console window -->
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <CopyCefRuntime>true</CopyCefRuntime>                    <!-- copies the CEF binaries next to the exe -->
    <RequiresAspNetWebAssets>true</RequiresAspNetWebAssets>  <!-- Blazor only, see below -->
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia.Desktop" Version="$(AvaloniaVersion)" />
    <PackageReference Include="CefGlue.OSR.Avalonia" Version="$(CefGlueOSRVersion)" />
  </ItemGroup>
</Project>
```

`CopyCefRuntime` and the Blazor asset fix come from this repo's `Directory.Build.targets`; copy it into your own solution if you use MiniPipeline outside it.

Running from `bin\` needs no `wwwroot` copy step: MiniPipeline reads the build's static web assets manifest, which also covers Razor class library `_content/`. For `dotnet publish`, the demos still copy `wwwroot` with a `CopyWebAssetsAfterPublish` target (publish hasn't been re-tested on .NET 10).

## Blazor

- **Use long polling.** It runs entirely on the scheme handler: no TCP listener, no certificate, works on `app://local`.

  ```csharp
  PipelineCefConfig.UseBlazorLongPollingTransport = true;   // and don't call AddSocketPipeline()
  ```

  The Blazor demo switches with a command-line argument: `BlazorApp.Desktop.exe --longpolling`.
- **`RequiresAspNetWebAssets`** – since .NET 9, `_framework/blazor.web.js` ships as a static web asset that the Web SDK registers only for console (`Exe`) projects. A desktop app is a `WinExe` on the Razor SDK, so set `<RequiresAspNetWebAssets>true</RequiresAspNetWebAssets>`; `Directory.Build.targets` then registers the script. Without it the page renders but nothing is interactive ("Loading..." forever).
- **`HttpClient` to your own app** doesn't work from Blazor Server components – there is no server listening, only the scheme handler. Call your services directly instead.
- **Blazor WebAssembly** – publish a standard Blazor WebAssembly project and serve its `wwwroot` with `UseDefaultFiles()`, `UseStaticFiles()` and `MapFallbackToFile("index.html")` (see `BuildBlazorWebAssembly` in the Minimal demo).

## WebSocket Support [<span style="color:red">EXPERIMENTAL</span>]

> ⚠️ Experimental, and may be removed in a future version. Prefer long polling for Blazor.

A CEF scheme handler can't serve `ws://`/`wss://`, so `AddSocketPipeline()` starts a background TCP listener on `127.0.0.1` that takes the WebSocket handshake and hands it to your pipeline. That makes these work:

```csharp
app.UseWebSockets();          // plain WebSockets
app.MapHub<YourHub>("/hub");  // SignalR
```

What it needs:

- The scheme becomes `http`/`https` on `localhost:<port>` (default `8080`); for `wss`, a `.pfx` certificate:

  ```csharp
  builder.Services.Configure<PipelineSocketOptions>(config =>
  {
      config.UseSsl = true;
      config.PfxPath = "yourpfxfile.pfx";
      config.Password = "1234";
  }).AddSocketPipeline();
  ```

  For local testing, `dotnet dev-certs https -ep yourpfxfile.pfx -p 1234` exports the ASP.NET Core development certificate (trust it once with `dotnet dev-certs https --trust`).
- **`PipelinePermissionHandler` on every browser.** Pages come from the scheme handler, so Chromium sees no IP address and treats them as public; a WebSocket from such a page to `127.0.0.1` needs Chromium's *Local Network Access* permission. OSR browsers never show that prompt, so without a handler the socket waits forever before its handshake. The handler grants it only for your app's own origin:

  ```csharp
  browser.PermissionHandler = new PipelinePermissionHandler();
  ```
- Any local process can connect to the listener's port – keep that in mind for what your hubs expose.

## Good to know

- **No `Referer` from `app://` pages.** Third-party services that require one block your requests – e.g. `tile.openstreetmap.org` answers 403. The map sample uses [OpenFreeMap](https://openfreemap.org) tiles, which need no key and no Referer.
- **App size** – CEF adds roughly 150–300 MB, much like Electron.

## Limitations

- **Project template** – none yet, see [Project setup](#project-setup).
- **Hot Reload** – not supported at the moment.
- **Platforms** – tested on Windows x64. CefGlue.OSR also runs Avalonia and GTK4 on Linux x64; MiniPipeline hasn't been tried there yet.
- **Docs** – I know the docs aren't great yet — I'll clean them up once I have more time. A test project is also on the way.

## Credits
- **Core Inspiration**  
   [Chromium.AspNetCore.Bridge](https://github.com/ChromiumDotNet/Chromium.AspNetCore.Bridge) – by Alex Maitland  
   Provided the foundation and insight on bridging ASP.NET Core with CEF – this project wouldn't exist without that pioneering work.

- **CEF Integration**  
  [CefGlue.OSR](https://github.com/marman-hp/CefGlue.OSR) – CEF off-screen rendering for WinForms, WPF, WinUI 3, Avalonia and GTK4,  
  based on [OutSystems CefGlue](https://github.com/OutSystems/CefGlue) and Xilium CefGlue.
