using MiniPipeline.Core;
using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace MiniPipeline.CefGlue
{
    /// <summary>
    /// Grants Chromium's Local Network Access prompt for the app's own WebSocket listener, and nothing else.
    /// Pages come from the scheme handler, so Chromium has no IP for them and treats them as public; a WebSocket from
    /// such a page to the loopback listener needs the LocalNetwork/LoopbackNetwork permission. OSR browsers are Alloy
    /// style, where an unanswered prompt is IGNORE, so without this the socket waits forever before its handshake.
    /// Any other permission, or any other origin, gets CEF's default handling.
    /// </summary>
    public class PipelinePermissionHandler : PermissionHandler
    {
        private const CefPermissionRequestTypes LocalNetworkPermissions =
            CefPermissionRequestTypes.LocalNetwork |
            CefPermissionRequestTypes.LoopbackNetwork |
            CefPermissionRequestTypes.LocalNetworkAccessDeprecated;

        protected override bool OnShowPermissionPrompt(CefBrowser browser, ulong promptId, string requestingOrigin,
            CefPermissionRequestTypes requestedPermissions, CefPermissionPromptCallback callback)
        {
            var onlyLocalNetwork = requestedPermissions != CefPermissionRequestTypes.None
                && (requestedPermissions & ~LocalNetworkPermissions) == CefPermissionRequestTypes.None;

            if (onlyLocalNetwork && IsAppOrigin(requestingOrigin))
            {
                callback.Continue(CefPermissionRequestResult.Accept);
                return true;
            }

            return base.OnShowPermissionPrompt(browser, promptId, requestingOrigin, requestedPermissions, callback);
        }

        private static bool IsAppOrigin(string origin)
        {
            return Uri.TryCreate(origin, UriKind.Absolute, out var requesting)
                && Uri.TryCreate(PipelineCefConfig.BaseAddress, UriKind.Absolute, out var app)
                && Uri.Compare(requesting, app, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
