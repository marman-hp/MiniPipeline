using Xilium.CefGlue;

namespace MiniPipeline.CefGlue
{
    public class CefCallbackWrapper : ICefCallback
    {
        private readonly CefCallback _callback;

        public CefCallbackWrapper(CefCallback callback)
        {
            _callback = callback;
        }

        public void Continue() => _callback.Continue();

        public void Cancel() => _callback.Cancel();

        public void Dispose()
        {
            _callback.Dispose();
        }
    }
}
