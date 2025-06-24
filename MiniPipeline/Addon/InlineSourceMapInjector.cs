using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniPipeline.Core
{
    public class InlineSourceMapInjector : IPipelineAddon
    {
        public int Order => 1002;
        public async Task OnAfterExecute(PipelineResponse response, CancellationToken cancellationToken)
        {
            if ( !response.Cancel && 
                 (IsCssContent(response.ContentType) || IsJsContent(response.ContentType)) )
                 await TryInjectSourceMapAsync(response.Stream,response.RequestPath,PipelineCefConfig.wwwrootFolder,cancellationToken);

            return;
        }

        public Task OnBeforeExecute(HttpContext context) => Task.CompletedTask;

        private async Task TryInjectSourceMapAsync(Stream cefStream, PathString urlPath, string webRootPath, CancellationToken token)
        {
            int mode = urlPath.Value.Contains(".css") ? 0 : 1;

            cefStream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(cefStream, Encoding.UTF8, leaveOpen: true);
            string content = await reader.ReadToEndAsync(token);

            if (!isThereMapFileAttached(content, urlPath, mode))
            {
                cefStream.Seek(0, SeekOrigin.Begin);
                return;
            }

            var mapPath = Helper.GetSourceMapPhysicalPath(webRootPath, urlPath);
            if (!File.Exists(mapPath))
                return;

            byte[] mapBytes = await File.ReadAllBytesAsync(mapPath, token);
            string base64Map = Convert.ToBase64String(mapBytes);

            string inlineMap = mode == 0 ? $"/*# sourceMappingURL=data:application/json;base64,{base64Map} */" : $"//# sourceMappingURL=data:application/json;base64,{base64Map}";

            string updatedContent = mode == 0 ? Regex.Replace(content, @"\/\*# sourceMappingURL=.*?\*\/", inlineMap) : Regex.Replace(content, @"\/\/# sourceMappingURL=.*", inlineMap);

            cefStream.SetLength(0);
            cefStream.Seek(0, SeekOrigin.Begin);

            using var writer = new StreamWriter(cefStream, Encoding.UTF8, leaveOpen: true);
            await writer.WriteAsync(updatedContent.AsMemory(), token);
            await writer.FlushAsync(token);

            cefStream.Seek(0, SeekOrigin.Begin);
        }

        private bool IsCssContent(string? contentType)
        => contentType?.Contains("css", StringComparison.OrdinalIgnoreCase) == true;

        private bool IsJsContent(string? contentType)
        => contentType?.Contains("javascript", StringComparison.OrdinalIgnoreCase) == true;

        bool isThereMapFileAttached(string content, string requestPath, int mode = 0) //mode = 0 for css else for js
        {
            string prefix = mode == 0 ? "/*# sourceMappingURL=" : "//# sourceMappingURL=";
            string suffix = " */";

            int start = content.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);

            if (start < 0)
                return false;

            if (mode == 0)
            {

                int end = content.IndexOf(suffix, start);
                if (end < 0)
                    return false; // Format broken, skip

                string full = content.Substring(start, end + suffix.Length - start);

                // Split dan replace
                var parts = full.Split('=');
                if (parts.Length < 2)
                    return false;
            }
            return true;
        }

    }
}
