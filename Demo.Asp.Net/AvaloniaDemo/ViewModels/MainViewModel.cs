using AvaloniaDemo.Behaviors;
using MiniPipeline.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaDemo.ViewModels
{
    public class MainViewModel : ReactiveObject
    {

        public ObservableCollection<BrowserViewModel> Tabs { get; set;} = new ObservableCollection<BrowserViewModel>();

        public ICommand OpenDevToolsCommand { get; }
        public ICommand CloseTabCommand { get; }
        public MainViewModel()
        {
            CloseTabCommand = new RelayCommand<BrowserViewModel>(OnCloseTab);
            OpenDevToolsCommand = new RelayCommand(OnOpenDevTools);
        }

        void OnOpenDevTools()
        {
            SelectedTab.ShowDevTools();
        }
        private void OnCloseTab(BrowserViewModel tab)
        {
           if (Tabs.Contains(tab))
           {
              Tabs.Remove(tab);
              tab.RemoveBrowser();
           }
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
        }

        private BrowserViewModel _selectedTab;
         public BrowserViewModel SelectedTab
        {
            get => _selectedTab;
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }


        public void AddNewTab(string url, bool autolunch = true)
        {
            var tab = new BrowserViewModel(url,autolunch);
            Tabs.Add(tab);
            SelectedTabIndex = Tabs.Count - 1; 
            SelectedTab = Tabs[SelectedTabIndex];
        }
    }
}
