using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace RazorAddonViews.Pages.Test
{
    public class FileResultPageModel : PageModel
    {
        public string Description { get; set; }
        public List<UploadedFileInfo> Files { get; set; }

        public void OnGet()
        {
            var json = HttpContext.Session.GetString("UploadResult");
            if (!string.IsNullOrEmpty(json))
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<UploadResultData>(json);
                Description = data.Description;
                Files = data.Files;
            }
        }
        string GetWwwRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "wwwroot")))
            {
                dir = dir.Parent;
            }

            return dir?.FullName ?? AppContext.BaseDirectory;
        }
        public IActionResult OnGetDownloadFile(string fileName)
        {
            var rootfolder = GetWwwRoot();
            var filePath = Path.Combine(rootfolder, "wwwroot", "uploads", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileStream = System.IO.File.OpenRead(filePath);
            var contentType = "application/octet-stream";
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = fileName,
                Inline = false
            };

            Response.Headers["Content-Disposition"] = cd.ToString();
            Response.Headers["Content-Length"] = new FileInfo(filePath).Length.ToString();
            Response.Headers["Content-Type"] = contentType;

            return File(fileStream, contentType);
        }


    }

    class UploadResultData
    {
        public string Description { get; set; }
        public List<UploadedFileInfo> Files { get; set; }
    }
}
