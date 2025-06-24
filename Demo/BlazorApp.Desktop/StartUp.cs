using System.Diagnostics;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.DependencyInjection;


using MiniPipeline.Core;
using MiniPipeline.WebSocket;
using Microsoft.Extensions.Hosting;

namespace BlazorApp.Desktop
{
    internal static class StartUp
    {

        internal static IApplicationBuilder BuildBlazorWebApp()
        {
            var builder = MiniPipelineBuilder.CreateBuilder();

            builder.Environment.EnvironmentName = Environments.Development;
         
            builder.Services.Configure<PipelineSocketOptions>(config =>
            {
                config.UseSsl = true;
                config.PfxPath = @"D:\localhost.pfx"; 
                config.Password = "1234";
            }).AddSocketPipeline();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                });

            builder.Services.AddAuthorization();

            builder.Services.AddRazorPages();
            builder.Services.AddRazorComponents(options => options.DetailedErrors = true)
                .AddInteractiveServerComponents();

            builder.Services.AddScoped<CircuitHandler, DebugCircuitHandler>();
            var app = builder.Build();
            
            //if (!app.Environment.IsDevelopment())
            //{
            //    app.UseExceptionHandler("/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    // app.UseHsts();
            //}

            app.UseStatusCodePagesWithReExecute("/");

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapRazorComponents<BlazorApp.Desktop.Components.App>()
                    .AddInteractiveServerRenderMode();


            // Middleware login
            app.MapPost("/processlogin", async (HttpContext context, IAntiforgery antiforgery) =>
            {
                await antiforgery.ValidateRequestAsync(context);
                var form = await context.Request.ReadFormAsync();
                var username = form["username"];

                var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                context.Response.Redirect("/auth");
            });


            // Middleware logout
            app.MapPost("/logout", async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/");
            });

            app.MapControllers();

            //Debug tracker for circuit
            //for testing only, is it my pipeline capable for creating circuit
            //app.Use(async (context, next) =>
            //{
            //    var circuitAccessor = context.RequestServices.GetService<CircuitHandler>();
            //    await next();
            //});



            return app;
        }
    }
   
    public class DebugCircuitHandler : CircuitHandler
    {

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            
            Debug.WriteLine($"CIRCUIT OPENED: {circuit.Id},");
            return Task.CompletedTask;
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            Debug.WriteLine($"CIRCUIT CLOSED: {circuit.Id}");
            return Task.CompletedTask;
        }
    }
}