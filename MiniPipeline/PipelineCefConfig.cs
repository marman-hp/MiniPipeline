using Microsoft.AspNetCore.Builder;
using System.ComponentModel;

namespace MiniPipeline.Core
{
    public static class PipelineCefConfig
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        internal static string baseAddress = string.Empty;

        internal static string wwwrootfolder = string.Empty;
        public static string Scheme { get; set; } = "app";
        public static string Host { get; set; } = "local";

        public static string BaseAddress => baseAddress;

        public static string wwwrootFolder => wwwrootfolder;

        public static bool UseBlazorLongPollingTransport { get; set; } = false;

        internal static IServiceProvider? services;

        /// <summary>
        /// The app's service provider, set by CefInitSchemeHandler. UI code that the container doesn't create
        /// (XAML views) resolves CefGlueHost and other app services from here.
        /// </summary>
        public static IServiceProvider? Services => services;
    }

}
