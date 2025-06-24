using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace RazorAddonViews.Pages.Test
{

    public class TestPageModel : PageModel
    {

        [BindProperty]
        public TestInputModel InputData { get; set; }

        [BindProperty]
        public TestSessionModel SessionData { get; set; }

        [BindProperty]
        public UploadedFileInfoModel UploadFilesData { get; set; }

        [BindProperty]
        public LoginModel LoginData { get; set; }

        
        public void OnGet()
        {

        }


        public async Task<IActionResult> OnPostInputDataAsync()
        {
            ModelState.SkipUnrelatedModels("InputData");
            if (!ModelState.IsValid)
                return Page();

            TempData["MessageForm1"] = $"Value 1 = {InputData.Input1}, Value 2 = {InputData.Input2}, VALID = {ModelState.IsValid}";
            return RedirectToPage();
        }

        [ValidateAntiForgeryToken]
        public IActionResult OnPostSessionData()
        {
            if (!string.IsNullOrEmpty(SessionData.InputSession))
                HttpContext.Session.SetString("SessionData", SessionData.InputSession);

            return RedirectToPage("/Test/SessionPage");
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


        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostFilesDataAsync()
        {
            ModelState.SkipUnrelatedModels("UploadFilesData");

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("UploadFilesData.Files", "Please select at least one file.");
                return Page();
            }

            var uploaded = new List<UploadedFileInfo>();

            string uploadDirectory = GetWwwRoot();


            foreach (var file in UploadFilesData.Files ?? Enumerable.Empty<IFormFile>())
            {
                if (file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadDirectory,"wwwroot","uploads", fileName);

                    // Save the file to local folder 
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    uploaded.Add(new UploadedFileInfo
                    {
                        FileName = file.FileName,
                        Size = file.Length
                    });
                }
            }

            var resultObject = new
            {
                UploadFilesData.Description,
                Files = uploaded
            };


            var json = System.Text.Json.JsonSerializer.Serialize(resultObject);
            HttpContext.Session.SetString("UploadResult", json);


            return RedirectToPage("/Test/FileUploadResult");
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAuthAsync()
        {
            ModelState.SkipUnrelatedModels("LoginData");

            if (!ModelState.IsValid)
            {
                TempData["LoginError"] = "Authentication Failed, User = admin , Password = secret ";
                return Page();
            }

            if (LoginData.Username == "admin" && LoginData.Password == "secret")
            {
               var claims = new List<Claim>
              {
                new Claim(ClaimTypes.Name, LoginData.Username),
                new Claim("Role", "Admin")
              };

                var identity = new ClaimsIdentity(claims, "SimpleAuthCookie");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("SimpleAuthCookie", principal);

                return RedirectToPage("/Test/AuthPage");
            }

            TempData["LoginError"] = "Authentication Failed, User = admin , Password = secret ";
            return Page();
        }

    }

    //Mark as nullable ALL properties in your ALL classes that not marked as required, 
    //Not required in not nullable fields leads IsValid always false since .NET 6
    public class TestInputModel
    {
        [Display(Name = "Value 1")]
        [Required]
        public string Input1 { get; set; }

        [Display(Name = "Value 2")]
        public string? Input2 { get; set; }
    }

    public class TestSessionModel
    {
        [Display(Name = "Session Value")]
        public string? InputSession { get; set; }

    }

    public class UploadedFileInfoModel
    {
        [Required]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select at least one file")]
        public List<IFormFile> Files { get; set; }
    }

    public class UploadedFileInfo
    {
        public string FileName { get; set; }
        public long Size { get; set; }
    }

    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    //when work with multiple forms and use multiple models in 1 page ,
    //there is issue in ModelState and its normal by design. this extension will skip unrelated model 
    public static class ModelStateExtensions
    {
        public static ModelStateDictionary MarkAllFieldsAsSkipped(this ModelStateDictionary modelState)
        {
            foreach (var state in modelState.Select(x => x.Value))
            {
                state.Errors.Clear();
                state.ValidationState = ModelValidationState.Skipped;
            }
            return modelState;
        }

        public static void SkipUnrelatedModels(this ModelStateDictionary modelState, params string[] allowedPrefixes)
        {
            foreach (var kvp in modelState)
            {
                if (!allowedPrefixes.Any(prefix => kvp.Key.StartsWith(prefix)))
                {
                    kvp.Value.Errors.Clear();
                    kvp.Value.ValidationState = ModelValidationState.Skipped;
                }
            }
        }
    }
}
