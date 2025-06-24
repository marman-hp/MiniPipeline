using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using AvaloniaDemo.Behaviors;
using ReactiveUI;
using System;
using System.Reactive;
using System.Windows.Input;
using Xilium.CefGlue.Avalonia;

namespace AvaloniaDemo.ViewModels;
public class BrowserViewModel : ReactiveObject
{
    private string? _title;
    private string? _url;

    private bool _isBusy;
    private double _progress;
    public ReactiveCommand<Unit, Unit> Navigate { get; }
    public string Title
    {
        get => _title!;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private Bitmap? _favicon;
    public Bitmap? Favicon
    {
       get => _favicon;
       set => this.RaiseAndSetIfChanged(ref _favicon, value);

    }

    public bool IsFaviconVisible => Favicon != null;

    public string Url
    {
        get => _url!;
        set => this.RaiseAndSetIfChanged(ref _url, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value); 
    }

    public double Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    readonly bool _autolunch ;
    public bool Autolunch => _autolunch;

    public BrowserViewModel(string? url = null,bool autolunch=true)
    {
        _autolunch = autolunch;
        Url = url ?? "about:blank";
        Title = url ?? "NewTab"; // Default title for new tabs
         this.WhenAnyValue(x => x.Favicon)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(IsFaviconVisible)));
    }

    AvaloniaCefBrowser? _browser;

    public void AttachBrowser(AvaloniaCefBrowser browser)
    {
        if (!Design.IsDesignMode)
        {
            _browser = browser;

        }
    }

    private ICommand? _textBoxKeyDownCommand;
    public ICommand TextBoxKeyDownCommand =>
        _textBoxKeyDownCommand ??= new RelayCommand<KeyEventArgs>(OnKeyDown);

    private void OnKeyDown(KeyEventArgs args)
    {

        if (args.Key == Key.Enter)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(Url, UriKind.Absolute, out uriResult);
            if (result)
            {
                _browser.Address = Url;
            }
        }
    }

    private bool _canGoForward;
    public bool CanGoForward
    {
        get => _canGoForward;
        set => this.RaiseAndSetIfChanged(ref _canGoForward, value);
    }



    private bool _canGoBack;
    public bool CanGoBack
    {
        get => _canGoBack;
        set => this.RaiseAndSetIfChanged(ref _canGoBack, value);
    }

    public void GoBack() => _browser?.GoBack();
    public void GoForward() => _browser?.GoForward();

    public void ReloadOrStop()
    {
        if(IsBusy)
            _browser?.ExecuteJavaScript("window.stop();");
        else
            _browser?.Reload();
    }

    public void RemoveBrowser()
    {
        
        _browser?.CloseDeveloperTools();
        _browser?.Dispose();
        _browser = null;
    }

    public void ShowDevTools()
    {
         _browser.ShowDeveloperTools();
    }


}
