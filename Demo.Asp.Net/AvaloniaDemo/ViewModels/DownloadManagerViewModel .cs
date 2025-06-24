using Avalonia;
using Avalonia.Threading;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;

namespace AvaloniaDemo.ViewModels
{
    public class DownloadManagerViewModel : ReactiveObject
    {
        private readonly ObservableCollection<DownloadItemViewModel> _downloadsItems;
        public ObservableCollection<DownloadItemViewModel> DownloadItems => _downloadsItems;

        public ReactiveCommand<Unit, Unit> ClearCompletedDownloads { get; }

        public DownloadManagerViewModel()
        {
            _downloadsItems = new ObservableCollection<DownloadItemViewModel>();



            ClearCompletedDownloads = ReactiveCommand.Create(() =>
            {
                var completedDownloads = DownloadItems.Where(d => d.Status == "Completed").ToList();
                //foreach (var download in completedDownloads)
                //{
                //    //ActiveDownloads.Remove(download);
                //}
            });
        }

        public void Add(uint id, string url, string FileSuggetion, long totalbytes)
        {
            DownloadItemViewModel downloadItemViewModel = new DownloadItemViewModel()
            {
                Id = id,
                Url = url,
                FileName = FileSuggetion,
                ReceivedBytes = 0,
                TotalBytes = totalbytes
            };
            _downloadsItems.Add(downloadItemViewModel);
        }

        public void Update(uint id, long receivedBytes, long totalBytes, bool isComplete, bool isCanceled,string reason)
        {
            var existing = _downloadsItems.FirstOrDefault(d => d.Id == id);
            if (existing != null)
                existing?.Update(receivedBytes, totalBytes, isComplete, isCanceled,reason);

        }
    }


}
