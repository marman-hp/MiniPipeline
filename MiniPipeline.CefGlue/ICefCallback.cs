namespace MiniPipeline.CefGlue
{
    public interface ICefCallback : IDisposable
    {
        void Continue();
        void Cancel();
    }
}
