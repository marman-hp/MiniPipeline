using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaDemo.Desktop.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Handler - Token Cancelling: {Status}", cancellationToken.IsCancellationRequested);
            //var token = HttpContext.Items["RequestTimeoutToken"] as CancellationToken? ?? CancellationToken.None;

            //await Task.Delay(10000, cancellationToken); // Delay 10s
            //_logger.LogInformation("Handler - Token Cancelled: {Status}", cancellationToken.IsCancellationRequested);
        }
    }

}
