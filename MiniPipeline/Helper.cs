using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace MiniPipeline.Core
{
    public static class MimeTypeHelper
{
    private static readonly Dictionary<string, string> _mimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Text
        [".html"] = "text/html",
        [".htm"] = "text/html",
        [".css"] = "text/css",
        [".csv"] = "text/csv",
        [".txt"] = "text/plain",
        [".xml"] = "application/xml",

        // JavaScript & JSON
        [".js"] = "application/javascript",
        [".mjs"] = "application/javascript",
        [".json"] = "application/json",

        // Images
        [".bmp"] = "image/bmp",
        [".gif"] = "image/gif",
        [".ico"] = "image/x-icon",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".svg"] = "image/svg+xml",
        [".tiff"] = "image/tiff",
        [".webp"] = "image/webp",

        // Fonts
        [".woff"] = "font/woff",
        [".woff2"] = "font/woff2",
        [".ttf"] = "font/ttf",
        [".otf"] = "font/otf",
        [".eot"] = "application/vnd.ms-fontobject",

        // Archives
        [".zip"] = "application/zip",
        [".gz"] = "application/gzip",
        [".tar"] = "application/x-tar",
        [".rar"] = "application/vnd.rar",
        [".7z"] = "application/x-7z-compressed",

        // Documents
        [".pdf"] = "application/pdf",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xls"] = "application/vnd.ms-excel",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".ppt"] = "application/vnd.ms-powerpoint",
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",

        // Audio/Video
        [".mp3"] = "audio/mpeg",
        [".wav"] = "audio/wav",
        [".ogg"] = "audio/ogg",
        [".mp4"] = "video/mp4",
        [".webm"] = "video/webm",
        [".avi"] = "video/x-msvideo",
        [".mov"] = "video/quicktime",

        // WebAssembly / Blazor
        [".wasm"] = "application/wasm",
        [".dll"] = "application/octet-stream",
        [".dat"] = "application/octet-stream",
        [".blat"] = "application/octet-stream",
        [".pdb"] = "application/octet-stream",
        [".br"] = "application/brotli",
        [".ttc"] = "font/collection",

        // Misc
        [".exe"] = "application/vnd.microsoft.portable-executable",
        [".apk"] = "application/vnd.android.package-archive",
        [".deb"] = "application/vnd.debian.binary-package",
        [".rpm"] = "application/x-rpm"
    };

    public static string GetMimeType(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return ext != null && _mimeTypes.TryGetValue(ext, out var mime)
            ? mime
            : "application/octet-stream"; // fallback for unknowns
    }
}

    internal static class Helper
    {

        public static IHeaderDictionary Clone(this IHeaderDictionary original)
        {
            var clone = new HeaderDictionary();
            foreach (var kvp in original)
            {
                clone[kvp.Key] = new StringValues(kvp.Value.ToArray());
            }
            return clone;
        }

        private static readonly Dictionary<int, string> StatusCodeToStatusTextMapping = new Dictionary<int, string>
        {
            {100, "Continue" },
            {101, "Switching Protocols" },
            {102, "Processing" },
            {103, "Early Hints" },

            {200, "OK"},
            {201, "Created"},
            {202, "Accepted"},
            {203, "Non-Authoritative Information" },
            {204, "No Content"},
            {205, "Reset Content"},
            {206, "Partial Content"},
            {207, "Multi-Status"},
            {208, "Already Reported"},

            {300, "Multiple Choices"},
            {301, "Moved Permanently"},
            {302, "Found" },
            {303, "See Other" },
            {304, "Not Modified"},
            {305, "Use Proxy"},
            {306, "unused"},
            {307, "Temporary Redirect"},
            {308, "ResumeIncomplete" },


            {400, "Bad Request" },
            {401, "Unauthorized" },
            {402, "Payment Required" },
            {403, "Forbidden" },
            {404, "Not Found"},
            {405, "Method Not Allowed"},
            {406, "Not Acceptable"},
            {407, "Proxy Authentication Required"},
            {408, "Request Timeout"},
            {409, "Conflict"},
            {410, "Gone"},
            {411, "Length Required"},
            {412, "Precondition Failed"},
            {413, "Payload Too Large"},
            {414, "URI Too Long"},
            {415, "Unsupported Media Type"},
            {416, "Range Not Satisfiable"},
            {417, "Expectation Failed"},
            {418, "I'm a teapot"},

            {421, "Misdirected Request"},
            {422, "Unprocessable Content"},
            {423, "Locked"},
            {424, "Failed Dependency"},
            {425, "Too Early"},
            {426, "Upgrade Required"},
            {428, "Precondition Required"},
            {429, "Too Many Requests"},
            {431, " Request Header Fields Too Large"},
            {451, "Unavailable For Legal Reasons"},


            {500,"Internal Error"},
            {501,"Not Implemented"},
            {502,"Bad Gateway"},
            {503,"Service Unavailable"},
            {504,"Gateway Timeout"},
            {505,"HTTP Version Not Supported"},
            {506,"Variant Also Negotiates"},
            {507,"Insufficient Storage"},
            {508,"Loop Detected"},
            {510,"Not Extended"},
            {511,"Network Authentication Required" }
        };



        public static string GetStatus(int statusCode) => StatusCodeToStatusTextMapping[statusCode];


        public static bool IsRunningInVs()
        {
            return Debugger.IsAttached &&
                   Environment.GetEnvironmentVariable("VisualStudioEdition") != null;
        }
        public static string GetWebRootPath(IWebHostEnvironment env)
        {
            string wwwrootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");

            if (IsRunningInVs())
            {
                //var env = context.RequestServices.GetService<IWebHostEnvironment>();
                if (string.IsNullOrEmpty(env.WebRootPath))
                {
                    Assembly entryAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.EntryPoint != null);

                    var runtimejson = Path.Combine(AppContext.BaseDirectory, $"{entryAssembly.GetName().Name}.staticwebassets.runtime.json");
                    if (runtimejson != null && File.Exists(runtimejson))
                    {
                        using var json = JsonDocument.Parse(File.ReadAllText(runtimejson));
                        wwwrootPath = json.RootElement.GetProperty("ContentRoots")[0].GetString();
                    }

                }


            }

            return wwwrootPath;
        }




        public static string GetSourceMapPhysicalPath(string webRootPath, string urlPath)
        {
            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new ArgumentNullException(nameof(webRootPath));

            if (string.IsNullOrWhiteSpace(urlPath))
                throw new ArgumentNullException(nameof(urlPath));

            // Normalize: buang slash depan, replace '/' jadi path separator sesuai OS
            var relativePath = urlPath.TrimStart('/')
                                       .Replace('/', Path.DirectorySeparatorChar);

            // Tambah .map
            var finalPath = relativePath + ".map";

            return Path.Combine(webRootPath, finalPath);
        }



    }
}
