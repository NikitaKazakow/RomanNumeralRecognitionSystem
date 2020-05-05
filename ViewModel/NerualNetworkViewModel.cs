using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using RomanNumeralRecognitionSystem.Model;
using RomanNumeralRecognitionSystem.Util;
using RomanNumeralRecognitionSystem.View.Pages.SubMenu;

namespace RomanNumeralRecognitionSystem.ViewModel
{
    public class NerualNetworkViewModel : ViewModelBase
    {
        private static readonly object SLock = new object();
        private static NerualNetworkViewModel _instance;

        private NerualNetworkViewModel() { }

        public static NerualNetworkViewModel Instance
        {
            get
            {
                if (_instance != null) return _instance;
                Monitor.Enter(SLock);
                var temp = new NerualNetworkViewModel();
                Interlocked.Exchange(ref _instance, temp);
                Monitor.Exit(SLock);

                return _instance;
            }
        }

        private RelayCommand _showLearningPageCommand;
        private RelayCommand _showRecognitionRelayCommand;
        private RelayCommand _showSettingsRelayCommand;

        private Page _currentPage;

        private NerualNetwork _nerualNetwork;

        private ObservableCollection<Page> _subMenuPages;

        private ObservableCollection<Page> SubMenuPagesCollection =>
            _subMenuPages ?? (_subMenuPages = new ObservableCollection<Page>
            {
                new LearningPage(),
                new RecognitionPage(),
                new SettingsPage()
            });

        public RelayCommand ShowLearningPageCommand
        {
            get
            {
                return _showLearningPageCommand ?? (_showLearningPageCommand = new RelayCommand(obj =>
                {
                    CurrentPage = SubMenuPagesCollection[0];
                }));
            }
        }
        public RelayCommand ShowRecognitionRelayCommand
        {
            get
            {
                return _showRecognitionRelayCommand ?? (_showRecognitionRelayCommand = new RelayCommand(obj =>
                {
                    CurrentPage = SubMenuPagesCollection[1];
                }));
            }
        }
        public RelayCommand ShowSettingsRelayCommand
        {
            get
            {
                return _showSettingsRelayCommand ?? (_showSettingsRelayCommand = new RelayCommand(obj =>
                {
                    CurrentPage = SubMenuPagesCollection[2];
                }));
            }
        }

        public Page CurrentPage
        {
            get => _currentPage ?? (_currentPage = SubMenuPagesCollection[0]);
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public NerualNetwork NerualNetwork
        {
            get => _nerualNetwork;
            set
            {
                _nerualNetwork = value;
                OnPropertyChanged(nameof(NerualNetwork));
            }
        }
    }
}