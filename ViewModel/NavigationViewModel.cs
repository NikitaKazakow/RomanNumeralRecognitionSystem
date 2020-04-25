using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Threading;
using RomanNumeralRecognitionSystem.Util;
using RomanNumeralRecognitionSystem.View;

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

        private ObservableCollection<Page> PageCollection =>
            _pageCollection ?? (_pageCollection = new ObservableCollection<Page>
            {
                new StartPage(),
                new CreateNerualNetwork()
            });

        public RelayCommand ShowPageRelayCommand
        {
            get
            {
                return _showPageRelayCommand ??
                       (_showPageRelayCommand = new RelayCommand(obj => 
                           {
                               switch (obj as string)
                               {
                                   case "createNerualNetworkPage":
                                       CurrentPage = PageCollection[1];
                                       break;
                               }
                           },
                           (obj) => CurrentPage != obj as Page));
            }
        }
    }
}