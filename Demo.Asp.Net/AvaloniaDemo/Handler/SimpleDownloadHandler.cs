using Avalonia.Threading;
using System;
using System.IO;
using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace AvaloniaDemo.Handler
{
    //Need to create simple download manager
    public class SimpleDownloadHandler : DownloadHandler
    {
        private readonly string _downloadsPath;
        private string _fileName;
        
        public SimpleDownloadHandler(Func<string>  downloadpath)
        {
            _downloadsPath = downloadpath();
            _fileName = "";
        }
        protected override void OnBeforeDownload(
            CefBrowser browser,
            CefDownloadItem downloadItem,
            string suggestedName,
            CefBeforeDownloadCallback callback)
        {
            if (callback == null)
                return;

            _fileName = suggestedName;
            var fullname = GetUniqueFileName(_downloadsPath, suggestedName);
            var id = downloadItem.Id;
            var url =downloadItem.Url;
            var totalbyte = downloadItem.TotalBytes;

            Dispatcher.UIThread.Post(() => {
                (App.Current as App)?.ShowDownloadManager();
                (App.Current as App)?.AddDownloadInfo(id,url,suggestedName,totalbyte);
            });


            callback.Continue(fullname, showDialog: false);


        }

        protected override void OnDownloadUpdated(CefBrowser browser, CefDownloadItem downloadItem, CefDownloadItemCallback callback)
        {

            var received = downloadItem.ReceivedBytes;
            var total = downloadItem.TotalBytes;
            var percent = downloadItem.PercentComplete;
            var complete = percent == 100;
            var canceled = downloadItem.IsCanceled || downloadItem.InterruptReason != CefDownloadInterruptReason.None;
            var id = downloadItem.Id;
            var url = downloadItem.Url;

            var reason = MapInterruptReason(downloadItem.InterruptReason);

            Dispatcher.UIThread.Post(() =>
            {
                (Avalonia.Application.Current as App)?.UpdateDownloadInfo(id, received, total, complete, canceled,reason);
            }, DispatcherPriority.Normal);

        }

        private string GetUniqueFileName(string folderPath, string fileName)
        {
            string filePath = Path.Combine(folderPath, fileName);
            int counter = 1;

            while (File.Exists(filePath))
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);
                filePath = Path.Combine(folderPath, $"{fileNameWithoutExtension} ({counter}){extension}");
                counter++;
            }

            return filePath;
        }

        private string MapInterruptReason(CefDownloadInterruptReason reason)
{
    return reason switch
    {
        CefDownloadInterruptReason.None => "",
        CefDownloadInterruptReason.UserCanceled => "Canceled By User",
        CefDownloadInterruptReason.NetworkDisconnected => "Disconected",
        CefDownloadInterruptReason.NetworkTimeout => "Timeout",
        CefDownloadInterruptReason.FileNoSpace => "Disk No-space",
        CefDownloadInterruptReason.FileAccessDenied => "Access Denied",
        CefDownloadInterruptReason.ServerBadContent => "Server Bad Content",
        CefDownloadInterruptReason.NetworkFailed => "Failed",
        _ => $"{reason}"
    };
}



    }

}