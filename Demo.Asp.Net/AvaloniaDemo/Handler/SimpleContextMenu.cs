using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace AvaloniaDemo.Handler
{
    public class SimpleContextMenu : ContextMenuHandler
    {
        public delegate void OnOpenNewTab(object sender, string url);
        public event OnOpenNewTab OpenNewTab;
        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams state, CefMenuModel model)
        {
            
            model.AddSeparator();
            model.AddItem(26501+1,"Show DevTools..");
            model.AddItem(26501+2,"Open in new Tab..");

            if(string.IsNullOrEmpty(state.LinkUrl))
                model.SetEnabled(26501+2,false);
        }

        protected override bool OnContextMenuCommand(CefBrowser browser, CefFrame frame, CefContextMenuParams state, int commandId, CefEventFlags eventFlags)
        {
            if(commandId < 26501 )
                return false;
            
              if(commandId == 26501+1)
                (Avalonia.Application.Current as App)?.ShowDevTools();

              if(commandId == 26501+2)
                if(!string.IsNullOrEmpty(state.LinkUrl))
                   (Avalonia.Application.Current as App)?.AddTab(state.LinkUrl);
             
            return true;
        }
    }
}
