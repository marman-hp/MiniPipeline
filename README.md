# MiniPipeline 🚀


A blazing-fast, lightweight, in-memory ASP.NET Core pipeline designed to run **without Kestrel** and integrate directly with **desktop UI frameworks** like **Avalonia** using [**CefGlue.OSR**](https://github.com/marman-hp/CefGlue.OSR) (Chromium Embedded Framework, off-screen rendering). Ideal for hybrid desktop/web GUI apps, internal tools, and also support cross platform. 

---

## Features

- ✅ No Kestrel – In-memory, stream-based HTTP pipeline using pure `HttpContext` and `RequestDelegate`.
- ✅ Full `HttpContext` support: GET, POST, FILE UPLOAD, SESSION, AUTH.
- ✅ Razor Pages rendering without server.
- ✅ `IHttpResponseFeature`, `IHttpResponseBodyFeature`, and custom feature system.
- ✅ Custom scheme handler (`yourscheme://yourhost`) via [**CefGlue.OSR**](https://github.com/marman-hp/CefGlue.OSR).
- ✅ Lightweight DI and configuration (alt to `WebApplication.CreateBuilder`).
- ✅ Designed for Avalonia Desktop apps.
- ✅ Compatible with **websocket**, **signalr**, and **blazor** (`🔥EXPERIMENTAL`).

---
 **Asp.Net.core Demo**
![Asp.Net Core demo](20250628122359.gif)
 **Blazor server Demo**
![Blazor Server/App demo](blazor-web-app-svr.gif)


## How About Running Blazor Without WebSocket?

Starting from **RC1**, MiniPipeline now supports **longPolling** transport for Blazor Server.


If you don't want to use WebSocket, simply **skip calling** `AddSocketPipeline()`.  
This will cause the SignalR transport to **fall back to longPolling** automatically.
with longpolling transport you can use custome scheme (`yourscheme://yourhost`).

To explicitly enable it in Avalonia, set this flag at your program entry point:

```csharp
PipelineCefConfig.UseBlazorLongPollingTransport = true;
```

## Requirements

- .NET 10 SDK, x64
- [CefGlue.OSR](https://github.com/marman-hp/CefGlue.OSR) (CEF 154) cloned **next to** this repo, i.e. `..\CefGlue.OSR`:
  - build it once; it packs its NuGet packages into `CefGlue.OSR\Nuget\output`, which `nuget.config` here uses as a package source.
  - download the CEF binaries with its `get-cef.ps1`; the desktop demos copy `CefGlue.OSR\cef\runtimes\<rid>\native` next to their exe.
- Different location? Pass `-p:CefGlueOSRDir=path\to\CefGlue.OSR\` and change the path in `nuget.config`.

## Limitation
- **Project Template**  
  No official template yet.  
  For now, use an Avalonia project as the base and modify the project header:

  ```xml
  <Project Sdk="Microsoft.NET.Sdk.Razor"> 
  and add on the top PropertyGroup
  <AddRazorSupportForMvc>true</AddRazorSupportForMvc>	
  ```

  To ensure assets are copied correctly, add the following to your .csproj file:  
  ```xml
  <Target Name="CopyWebAssetsAfterBuild" AfterTargets="Build">
			<ItemGroup>
				<wwwrootFilesToOutput Include="wwwroot/**/*" />
			</ItemGroup>
			<Copy SourceFiles="@(wwwrootFilesToOutput)" DestinationFolder="$(OutputPath)wwwroot\%(RecursiveDir)" SkipUnchangedFiles="true" />
	</Target>
  ```

  To copy wwwroot to your publish output folder , add the following to your .csproj file:
  ```xml  
 	<Target Name="CopyWebAssetsAfterPublish" AfterTargets="Publish">
		<ItemGroup>
			<wwwrootFilesToPublish Include="$(ProjectDir)wwwroot\**\*" />
		</ItemGroup>

		<Copy SourceFiles="@(wwwrootFilesToPublish)" DestinationFolder="$(PublishDir)wwwroot\%(wwwrootFilesToPublish.RecursiveDir)" SkipUnchangedFiles="true" />
	</Target>
  ```

- **WebSocket Support [<span style="color:red">EXPERIMENTAL</span>]**  
  Since CEF bypasses the native `ws://` and `wss://` schemes in scheme handler, MiniPipeline uses a background TCP listener to capture WebSocket requests and signal the handshake.
  This makes it compatible with:

  ```c#
  //for standard websocket
  app.UseWebSockets();

  //for standard signalr
  app.UseSignalR()
  app.MapHub<YourHub>("/hub");
  ```  



- **The Blazor Support [<span style="color:red">EXPERIMENTAL</span>]**  
   Tested with Blazor on .NET 8; not yet re-tested since the move to .NET 10.  
   in **blazor web server** mode you can not use **httpclient** to access your own host, because its not server just **scheme handler**.

- **The Blazor WebAssembly**  
  - Create a standard Blazor WebAssembly project
  - Publish the static output
  - Embed it into an Avalonia desktop project
  
- **Hot Reload**  
   Not supported at the moment.    

- **Other Limitation**  
   I know the docs aren’t great yet — I’ll clean them up once I have more time. A test project is also on the way.

## Quick Start 
On your StartUp code :
```csharp

using MiniPipeline.Core;
using MiniPipeline.WebSocket; //If using WebSocket

var builder = MiniPipelineBuilder.CreateBuilder();


// If using WebSocket:
// Blazor web app/server default need websocket, it should be enabled
// with RC1 now you can skip this if you don't need to work with websocket 

//this only for websocket handshake 
// builder.Services.Configure<PipelineSocketOptions>(config =>
// {
//    config.UseSsl = true;
//    config.PfxPath = @"yourpfxfile.pfx"; 
//    config.Password = "1234";
// }).AddSocketPipeline();

var app = builder.Build();   
app.UseRouting();



app.MapGet("/", () =>
{
    const string html = """
    <!DOCTYPE html>
    <html>
    <head>
        <title>Sample</title>
    </head>
    <body>
        IT JUST WORKS!
    </body>
    </html>
    """;

    return Results.Content(html, "text/html");
});

return app;

```

On your Avalonia program entry point:
```csharp
  
  using MiniPipeline.Core;
  using MiniPipeline.CefGlue;
 

   var  app = YourStartUpCode();

   AppDomain.CurrentDomain.ProcessExit += delegate { 
      app.CefShutdown();
   };


   // If you're using WebSocket: 
   // The scheme name will become a standard `http` or `https` depending on your socket options.
   // and the host name must be standard hostname (e.g localhost)
   // For SSL, you'll need a `.pfx` certificate.

   // Example using non standard scheme:
      PipelineCefConfig.Scheme = "app";
      PipelineCefConfig.Host = "local";
        
   //use PipelineCefConfig.BaseAddress to get base address (e.g https://localhost:8080 default)

   var setting = new CefSettings()
   {
      CookieableSchemesList = PipelineCefConfig.Scheme
      .......
   };        


   BuildAvaloniaApp().AfterSetup(o =>
   {
      app.CefInitSchemeHandler(setting);
   })
   .StartWithClassicDesktopLifetime(args);


```
## Credits
- **Core Inspiration**  
   [Chromium.AspNetCore.Bridge](https://github.com/ChromiumDotNet/Chromium.AspNetCore.Bridge) – by Alex Maitland  
   Provided the foundation and insight on bridging ASP.NET Core with CEF – this project wouldn't exist without that pioneering work.

- **CEF Integration**  
  [CefGlue.OSR](https://github.com/marman-hp/CefGlue.OSR) – CEF off-screen rendering for WinForms, WPF, WinUI 3, Avalonia and GTK4,  
  based on [OutSystems CefGlue](https://github.com/OutSystems/CefGlue) and Xilium CefGlue. 