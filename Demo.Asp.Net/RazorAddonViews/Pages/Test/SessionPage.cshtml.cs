using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorAddonViews.Pages.Test
{
    public class SessionPageModel : PageModel
    {
        public void OnGet()
        {

            var myValue = HttpContext.Session.GetString("SessionData");

            TempData["SessionData1"] = myValue ?? "No Sessions are made";
            TempData["SessionData2"] = string.IsNullOrEmpty(myValue) ? "Check Session setting" : "The Session work as expected!";

            if(!string.IsNullOrEmpty(myValue))
              HttpContext.Session.Remove("SessionData");
        }
    }
}
