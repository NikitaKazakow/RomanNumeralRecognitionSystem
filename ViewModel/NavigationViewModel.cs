using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using RomanNumeralRecognitionSystem.Util;
using RomanNumeralRecognitionSystem.View.Pages;
using RomanNumeralRecognitionSystem.View.Pages.CreationWizard;

namespace RomanNumeralRecognitionSystem.ViewModel
{
    public sealed class NavigationViewModel : ViewModelBase
    {
        private static readonly object SLock = new object();
        private static NavigationViewModel _instance;
        
        private NavigationViewModel() { }

        public static NavigationViewModel Instance
        {
            get
            {
                if (_instance != null) return _instance;
                Monitor.Enter(SLock);
                var temp = new NavigationViewModel();
                Interlocked.Exchange(ref _instance, temp);
                Monitor.Exit(SLock);

                return _instance;
            }
        }

        private Page _currentPage;

        public enum Pages
        {
            StartPage,
            Back,
            CreateNerualNetworkNameDirectory,
            CreateNerualNetworkParams,
            MainMenuPage
        }

        private Stack<Page> _previousPageStack;
        private ObservableCollection<Page> _pageCollection;

        private RelayCommand _showPageRelayCommand;

        public Page CurrentPage
        {
            get => _currentPage ?? PageCollection[0];
            private set
            {
                if (_currentPage == value)
                    return;
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }
        
        private Stack<Page> PreviousPageStack => _previousPageStack ?? (_previousPageStack = new Stack<Page>());

        private ObservableCollection<Page> PageCollection =>
            _pageCollection ?? (_pageCollection = new ObservableCollection<Page>
            {
                new StartPage(),
                new MainMenuPage()
            });

        public RelayCommand ShowPageRelayCommand
        {
            get
            {
                return _showPageRelayCommand ??
                       (_showPageRelayCommand = new RelayCommand(obj => 
                           {
                               switch (obj as Enum)
                               {
                                   case Pages.StartPage:
                                       CurrentPage = PageCollection[0];
                                       PreviousPageStack.Clear();
                                       break;
                                   case Pages.CreateNerualNetworkNameDirectory:
                                       PreviousPageStack.Push(CurrentPage);
                                       CurrentPage = new NameAndPathPage();
                                       break;
                                   case Pages.CreateNerualNetworkParams:
                                       PreviousPageStack.Push(CurrentPage);
                                       CurrentPage = new NerualNetworkParamsPage();
                                       break;
                                   case Pages.MainMenuPage:
                                       PreviousPageStack.Clear();
                                       CurrentPage = PageCollection[1];
                                       break;
                                   case Pages.Back:
                                       if (PreviousPageStack.Count != 0)
                                           CurrentPage = PreviousPageStack.Pop();
                                       break;
                               }
                           }));
            }
        }
    }
}