using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiniPipeline.Core;
using MiniPipeline.WebSocket;
using Microsoft.AspNetCore.StaticFiles;

namespace Minimal.BrowserUI.Desktop
{
    internal static class StartUp
    {

        internal static IApplicationBuilder BuildWebSocket()
        {
            var builder = MiniPipelineBuilder.CreateBuilder();

            builder.Services.Configure<PipelineSocketOptions>(config =>
            {
                config.UseSsl = true;
                config.PfxPath = @"yourpfxfile";
                config.Password = "1234";
            }).AddSocketPipeline();

            var app = builder.Build();

            app.UseWebSockets();

            app.UseRouting();

            // WebSocket endpoint
            app.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var requestedProtocols = context.WebSockets.WebSocketRequestedProtocols;
                    var acceptedProtocol = requestedProtocols.Contains("json.protocol") ? "json.protocol" : null;

                    using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync(acceptedProtocol);
                    Debug.WriteLine("WebSocket connected!");

                    var buffer = new byte[1024 * 4];
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var hold = false;

                    while (!result.CloseStatus.HasValue)
                    {
                        var receivedText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Debug.WriteLine($"Received via WS: {receivedText}");

                        // Echo back
                        var outgoingBuffer = Encoding.UTF8.GetBytes("Echo: " + receivedText);
                        await webSocket.SendAsync(new ArraySegment<byte>(outgoingBuffer), WebSocketMessageType.Text, true, CancellationToken.None);

                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }

                    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                    Debug.WriteLine("WebSocket closed.");
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            });
            // HTTP GET endpoint
            app.MapGet("/", () =>
             {
                 const string html = """
    <!DOCTYPE html>
    <html>
    <head>
        <title>WebSocket Test</title>
        <style>
            #log {
                border: 1px solid #ccc;
                padding: 10px;
                height: 200px;
                overflow-y: scroll;
                background: #f9f9f9;
                font-family: monospace;
            }
        </style>
    </head>
    <body>
        <h1>WebSocket Echo Demo</h1>
        <input id="messageInput" type="text" placeholder="Type a message..." />
        <button onclick="sendMessage()">Send</button>
        <div id="log"></div>

        <script>

            const ws = new WebSocket("wss://localhost:8080/ws","json.protocol");
       
            const logDiv = document.getElementById("log");

            function log(message) {
                const entry = document.createElement("div");
                entry.textContent = message;
                logDiv.appendChild(entry);
                logDiv.scrollTop = logDiv.scrollHeight;
            }

            ws.onopen = () => log("WebSocket connected");

            ws.onmessage = event => log("Received: " + event.data);

            ws.onclose = () => log("WebSocket disconnected");

            function sendMessage() {
                const input = document.getElementById("messageInput");
                const message = input.value;
                ws.send(message);
                log("Sent: " + message);
                input.value = "";
            }
        </script>
    </body>
    </html>
    """;
                 return Results.Content(html, "text/html");
             });



            return app;
        }


        internal static IApplicationBuilder BuildSignalR()
        {
            var builder = MiniPipelineBuilder.CreateBuilder();


            builder.Services.Configure<PipelineSocketOptions>(config =>
            {
                config.UseSsl = true;
                config.PfxPath = @"yourpfxfile";
                config.Password = "1234";
            }).AddSocketPipeline();

            builder.Services.AddSignalR();

            var app = builder.Build();

            app.UseRouting();

            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Incoming: {context.Request.Method} {context.Request.Path}");
                await next();
            });

            app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <title>SignalR Chat</title>
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js""></script>
</head>
<body>
    <input type=""text"" id=""userInput"" placeholder=""Your name"" />
    <input type=""text"" id=""messageInput"" placeholder=""Your message"" />
    <button onclick=""sendMessage()"">Send</button>

    <ul id=""messagesList""></ul>

    <script>
        const connection = new signalR.HubConnectionBuilder()
        .withUrl(""https://localhost:8080/chatHub"", {
             transport: signalR.HttpTransportType.WebSockets,
             skipNegotiation: false
        })
        .build();

    connection.on(""ReceiveMessage"", function (user, message) {
        const li = document.createElement(""li"");
        li.textContent = `${user}: ${message}`;
        document.getElementById(""messagesList"").appendChild(li);
    });

    connection.start().catch(err => console.error(err.toString()));

    function sendMessage() {
        const user = document.getElementById(""userInput"").value;
        const message = document.getElementById(""messageInput"").value;
        connection.invoke(""SendMessage"", user, message)
            .catch(err => console.error(err.toString()));
    }
    </script>
</body>
</html>
    ");
});
            app.MapHub<ChatHub>("/chatHub");
            return app;
        }

        internal static IApplicationBuilder BuildBlazorWebAssembly()
        {
            //before run this, you need publish the BlazorApp.Client to root project folder of BlazorApp.Desktop
            var builder = MiniPipelineBuilder.CreateBuilder();

            var app = builder.Build();
            app.UseRouting();
            app.UseDefaultFiles();

            var provider = new FileExtensionContentTypeProvider(); provider.Mappings[".dat"] = "application/octet-stream";
            app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapFallbackToFile("index.html");
            });


            return app;
        }


    }
    class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, "Echo :" + message);

        }

    }
}
