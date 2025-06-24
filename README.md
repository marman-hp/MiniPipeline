# MiniPipeline 🚀


A blazing-fast, lightweight, in-memory ASP.NET Core pipeline designed to run **without Kestrel** and integrate directly with **desktop UI frameworks** like **Avalonia** using [**OutSystems Avalonia CefGlue**](https://github.com/OutSystems/CefGlue) (Chromium Embedded Framework). Ideal for hybrid desktop/web GUI apps, internal tools, and also support cross platform. 

---

## Features

- ✅ No Kestrel – In-memory, stream-based HTTP pipeline using pure `HttpContext` and `RequestDelegate`.
- ✅ Full `HttpContext` support: GET, POST, FILE UPLOAD, SESSION, AUTH.
- ✅ Razor Pages rendering without server.
- ✅ `IHttpResponseFeature`, `IHttpResponseBodyFeature`, and custom feature system.
- ✅ Custom scheme handler (`yourscheme://yourhost`) via [**OutSystems Avalonia CefGlue**](https://github.com/OutSystems/CefGlue).
- ✅ Lightweight DI and configuration (alt to `WebApplication.CreateBuilder`).
- ✅ Designed for Avalonia Desktop apps.
- ✅ Compatible with **websocket**, **signalr**, and **blazor .NET8 only** (`🔥EXPERIMENTAL`).

---


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
   Only tested with blazor  .NET 8.  
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
// Blazor web app/server need websocket, it should be enabled

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
  [OutSystems Avalonia CefGlue](https://github.com/OutSystems/CefGlue) – maintained by the OutSystems communities  
  Enables Chromium Embedded Framework to run seamlessly in Avalonia UI. 