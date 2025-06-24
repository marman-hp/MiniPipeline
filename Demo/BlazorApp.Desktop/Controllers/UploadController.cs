using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlazorApp.Desktop.Controllers
{
    [Route("[controller]")]
    [ValidateAntiForgeryToken] 

    public class UploadController : Controller
    {
        [HttpPost]
        [Route("/uploadfile")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected.");

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(uploadPath); // ensure folder exists

            var filePath = Path.Combine(uploadPath, Path.GetFileName(file.FileName));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Redirect("/upload?success=1&filename=" + Uri.EscapeDataString(file.FileName)); 
                //Ok($"File uploaded: {file.FileName}");
        }
    }

}
