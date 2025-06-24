using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace AvaloniaDemo.ViewModels
{


    public class DownloadItemViewModel : ReactiveObject
    {

        public DownloadItemViewModel()
        {
            this.WhenAnyValue(x => x.TotalBytes)
                 .Subscribe(_ => this.RaisePropertyChanged(nameof(IsIndeterminate)));
        }

        private long _receivedBytes;
        private long _totalBytes;
        private string _status;

        public uint Id { get; set; }

        public string Url { get; set; }
        public string FileName { get; set; }

        public bool IsIndeterminate => TotalBytes == 0;

        public long TotalBytes
        {
            get => _totalBytes;
            set
            {
                this.RaiseAndSetIfChanged(ref _totalBytes, value);
                this.RaisePropertyChanged(nameof(Progress));  
            }
        }

        public long ReceivedBytes
        {
            get => _receivedBytes;
            set
            {
                this.RaiseAndSetIfChanged(ref _receivedBytes, value);
                this.RaisePropertyChanged(nameof(Progress)); 
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                this.RaiseAndSetIfChanged(ref _status, value);
                this.RaisePropertyChanged(nameof(Progress));
            }
        }

        int prog;
        public int Progress
        {
            get
            {
                prog = TotalBytes == 0 ? 0 : (int)(ReceivedBytes * 100 / TotalBytes);
                return prog;
            }
        }
        bool _isComplete = false;
        public void Update(long receivedBytes, long totalBytes, bool isComplete, bool isCanceled,string reason)
        {

              if(!_isComplete)  {
                ReceivedBytes = receivedBytes;
                TotalBytes = totalBytes;
                Status = "Downloading...";
              }
              _isComplete = isComplete;

             if(_isComplete)
                Status = "Complete";
             
             if(!string.IsNullOrEmpty(reason))
                Status = reason;
        }

    }
}


