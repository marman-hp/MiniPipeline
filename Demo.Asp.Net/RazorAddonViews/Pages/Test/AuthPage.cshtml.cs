using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorAddonViews.Pages.Test
{
    using Microsoft.AspNetCore.Authorization;

    [Authorize]
    public class AuthPageModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
