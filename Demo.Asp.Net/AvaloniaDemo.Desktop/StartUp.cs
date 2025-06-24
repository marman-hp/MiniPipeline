
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiniPipeline.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
namespace AvaloniaDemo.Desktop
{
    public class StartUp
    {
        public static IApplicationBuilder Build()
        {
            var razorAsm = Assembly.LoadFrom("RazorAddonViews.dll"); //for testing only


            
            var builder = MiniPipelineBuilder.CreateBuilder();

            builder.Services.AddLogging();
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
            }).AddApplicationPart(razorAsm);

            builder.Services.AddAuthentication("SimpleAuthCookie")
            .AddCookie("SimpleAuthCookie", options =>
            {
                options.LoginPath = "/Test/TestPage";
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = false;


            });

            #region "MiniProfiler"
            // Add MiniProfiler services
            // If using Entity Framework Core, add profiling for it as well (see the end)
            // Note .AddMiniProfiler() returns a IMiniProfilerBuilder for easy IntelliSense
            builder.Services.AddMiniProfiler(options =>
            {
                // ALL of this is optional. You can simply call .AddMiniProfiler() for all defaults
                // Defaults: In-Memory for 30 minutes, everything profiled, every user can see

                // Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                // Control storage - the default is 30 minutes
                //(options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);
                //options.Storage = new SqlServerStorage("Data Source=.;Initial Catalog=MiniProfiler;Integrated Security=True;");

                // Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter();

                // To control authorization, you can use the Func<HttpRequest, bool> options:
                options.ResultsAuthorize = _ => true;
                //options.ResultsListAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
                //options.ResultsAuthorizeAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfiler;
                //options.ResultsAuthorizeListAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfilerLists;

                // To control which requests are profiled, use the Func<HttpRequest, bool> option:
                //options.ShouldProfile = request => MyShouldThisBeProfiledFunction(request);

                // Profiles are stored under a user ID, function to get it:
                //options.UserIdProvider =  request => MyGetUserIdFunction(request);

                // Optionally swap out the entire profiler provider, if you want
                // The default handles async and works fine for almost all applications
                //options.ProfilerProvider = new MyProfilerProvider();

                // Optionally disable "Connection Open()", "Connection Close()" (and async variants).
                //options.TrackConnectionOpenClose = false;

                // Optionally use something other than the "light" color scheme.
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

                // Optionally change the number of decimal places shown for millisecond timings.
                options.PopupDecimalPlaces = 2;

                // Enabled sending the Server-Timing header on responses
                options.EnableServerTimingHeader = true;

                // Optionally disable MVC filter profiling
                //options.EnableMvcFilterProfiling = false;
                // Or only save filters that take over a certain millisecond duration (including their children)
                //options.MvcFilterMinimumSaveMs = 1.0m;

                // Optionally disable MVC view profiling
                //options.EnableMvcViewProfiling = false;
                // Or only save views that take over a certain millisecond duration (including their children)
                //options.MvcViewMinimumSaveMs = 1.0m;

                // This enables debug mode with stacks and tooltips when using memory storage
                // It has a lot of overhead vs. normal profiling and should only be used with that in mind
                //options.EnableDebugMode = true;

                // Optionally listen to any errors that occur within MiniProfiler itself
                //options.OnInternalError = e => MyExceptionLogger(e);

                //options.IgnoredPaths.Add("/lib");
                //options.IgnoredPaths.Add("/css");
                //options.IgnoredPaths.Add("/js");
            });

            #endregion

            var app = builder.Build();

            app.UseMiniProfiler();
            app.UseStatusCodePages();

            if (app.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();


            app.UseStaticFiles();
            //app.UseAntiforgery();

            app.UseRouting();
            //app.UseCors();
            app.UseAuthentication();

            app.UseAuthorization();


            app.UseCookiePolicy();
            app.UseSession();

            app.MapGet("/Hello", contex =>
            {
                contex.Response.OnStarting(() =>
                {
                    Debug.WriteLine("OnStartin Called from test route.");
                    return Task.CompletedTask;
                });

                contex.Response.OnCompleted(() =>
                {
                    Debug.WriteLine("OnCompleted Called from test route.");
                    return Task.CompletedTask;
                });



                return contex.Response.WriteAsync("Hello World");
            });

            app.MapRazorPages();


            return app;
        }
    }


  
}
