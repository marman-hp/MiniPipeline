using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorAddonViews.Pages.Test
{
    public class LogoutModel : PageModel
    {
        public async Task OnGetAsync()
        {
               await HttpContext.SignOutAsync("SimpleAuthCookie");
               Response.Redirect("/Test/TestPage");
        }
    }
}
