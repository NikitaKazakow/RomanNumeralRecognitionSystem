using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Collections.ObjectModel;
using RomanNumeralRecognitionSystem.Model;
using RomanNumeralRecognitionSystem.Util;

namespace RomanNumeralRecognitionSystem.ViewModel
{
    public abstract class NerualNetworkViewModelBase : ViewModelBase
    {
        private static readonly object Locker = new object();
        public static int InputNodesCount { get; } = 90000;

        private int _outputNeuronCount;
        
        private bool _isValid;
        private bool _isValidLayersCount;
        private bool _isValidNeuronCount;
        private bool _isValidOutputNeuronCount;

        private string _waitLabelText;
        private string _saveFolder;
        private string _saveName;

        private Visibility _folderErrorVisibility;
        private Visibility _nameErrorVisibility;
        private Visibility _fileAlreadyExistVisibility;
        private Visibility _waitAnimationVisibility;

        private IFileService<NerualNetwork> FileService { get; }


        private ObservableCollection<HiddenLayerViewModel> _hiddenLayersCollection;

        public static double MaxHiddenLayersCount { get; } = 3;
        public static double MaxOutputNeuronCount { get; } = 100;

        public ObservableCollection<HiddenLayerViewModel> HiddenLayersCollection
        {
            get
            {
                if (_hiddenLayersCollection != null) return _hiddenLayersCollection;
                _hiddenLayersCollection = new ObservableCollection<HiddenLayerViewModel>
                {
                    new HiddenLayerViewModel
                    {
                        Number = 1,
                        NeuronCount = 1
                    }
                };
                return _hiddenLayersCollection;
            }
            set
            {
                _hiddenLayersCollection = value;
                OnPropertyChanged(nameof(HiddenLayersCollection));
            }
        }

        public bool IsValid
        {
            get
            {
                _isValid = IsValidNeuronCount && IsValidLayersCount && IsValidOutputNeuronCount;
                return _isValid;
            }
            set
            {
                _isValid = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }
        public bool IsValidLayersCount
        {
            private get => _isValidLayersCount;
            set
            {
                _isValidLayersCount = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }
        public bool IsValidNeuronCount
        {
            get => _isValidNeuronCount;
            set
            {
                _isValidNeuronCount = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }
        public bool IsValidOutputNeuronCount
        {
            private get => _isValidOutputNeuronCount;
            set
            {
                _isValidOutputNeuronCount = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public int HiddenLayersCount
        {
            get => HiddenLayersCollection.Count;
            set
            {
                if (value <= 0 || value > MaxHiddenLayersCount)
                    return;
                IsValidLayersCount = true;
                var count = HiddenLayersCollection.Count;
                if (value > count)
                {
                    for (var i = 0; i < value - count; i++)
                        HiddenLayersCollection
                            .Add(new HiddenLayerViewModel
                            {
                                Number = count + (i * 1) + 1,
                                NeuronCount = 1
                            });
                }
                else if (value < count)
                {
                    for (var i = count - 1; i >= value; i--)
                        HiddenLayersCollection
                            .RemoveAt(i);
                }
                OnPropertyChanged(nameof(HiddenLayersCount));
            }
        }
        public int OutputNeuronCont
        {
            get
            {
                if (_outputNeuronCount == 0)
                    _outputNeuronCount = 1;
                return _outputNeuronCount;
            }
            set
            {
                _outputNeuronCount = value;
                IsValidOutputNeuronCount = true;
                OnPropertyChanged(nameof(OutputNeuronCont));
            }
        }

        public string WaitLabelText
        {
            get => _waitLabelText;
            set
            {
                _waitLabelText = value;
                OnPropertyChanged(nameof(WaitLabelText));
            }
        }

        public Visibility WaitAnimationVisibility
        {
            get => _waitAnimationVisibility;
            set
            {
                _waitAnimationVisibility = value;
                OnPropertyChanged(nameof(WaitAnimationVisibility));
            }
        }

        public Visibility FolderErrorVisibility
        {
            get => _folderErrorVisibility;
            set
            {
                _folderErrorVisibility = value;
                OnPropertyChanged(nameof(FolderErrorVisibility));
            }
        }
        public Visibility NameErrorVisibility
        {
            get => _nameErrorVisibility;
            set
            {
                _nameErrorVisibility = value;
                OnPropertyChanged(nameof(NameErrorVisibility));
            }
        }
        public Visibility FileAlreadyExistVisibility
        {
            get => _fileAlreadyExistVisibility;
            set
            {
                _fileAlreadyExistVisibility = value;
                OnPropertyChanged(nameof(FileAlreadyExistVisibility));
            }
        }

        public string SaveFolder
        {
            get
            {
                if (_saveFolder != null) return _saveFolder;
                FolderErrorVisibility = Visibility.Collapsed;
                IsValidLayersCount = true;
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            set
            {
                if (_saveFolder == value)
                    return;
                try
                {
                    if (!Directory.Exists(Path.GetFullPath(value)))
                    {
                        IsValidLayersCount = false;
                        FolderErrorVisibility = Visibility.Visible;
                    }
                    else
                    {
                        IsValidLayersCount = true;
                        FolderErrorVisibility = Visibility.Collapsed;
                    }
                }
                catch (Exception)
                {
                    IsValidLayersCount = false;
                    FolderErrorVisibility = Visibility.Visible;
                }
                _saveFolder = value;
                OnPropertyChanged(nameof(SaveFolder));
            }
        }
        public string SaveName
        {
            get
            {
                if (_saveName != null) return _saveName;
                var i = 1;
                var name = "MyNerualNetwork" + i + ".nrnw";
                while (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    name)))
                {
                    i++;
                    name = "MyNerualNetwork" + i + ".nrnw";
                }
                _saveName = "MyNerualNetwork" + i;
                IsValidLayersCount = true;
                NameErrorVisibility =
                    FileAlreadyExistVisibility = Visibility.Collapsed;
                return _saveName;
            }
            set
            {
                if (_saveName == value)
                    return;
                if (value == "" ||
                    value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    IsValidLayersCount = false;
                    NameErrorVisibility = Visibility.Visible;
                }
                else if (File.Exists(Path.Combine(SaveFolder, value + ".nrnw")))
                {
                    IsValidLayersCount = false;
                    FileAlreadyExistVisibility = Visibility.Visible;
                }
                else
                {
                    IsValidLayersCount = true;
                    NameErrorVisibility =
                        FileAlreadyExistVisibility = Visibility.Collapsed;
                }
                _saveName = value;
                OnPropertyChanged(nameof(SaveName));
            }
        }
        public static string SavePath { get; set; }

        protected NerualNetworkViewModelBase()
        {
            IsValidLayersCount = true;
            IsValidNeuronCount = true;
            IsValidOutputNeuronCount = true;

            FileService = new BinaryFileService();
        }

        protected void CreateNerualNetwork()
        {
            var thread = new Thread(() =>
                {
                    WaitLabelText = "Создание нейронной сети...";
                    WaitAnimationVisibility = Visibility.Visible;
                    lock (Locker)
                    {
                        NerualNetworkProcessViewModel.Instance.NerualNetwork = new NerualNetwork(InputNodesCount,
                            HiddenLayersCollection.Select(hiddenLayerViewModel => hiddenLayerViewModel.NeuronCount)
                                .ToList(),
                            OutputNeuronCont);
                        SavePath = Path.Combine(SaveFolder, SaveName + ".nrnw");
                    }
                    WaitAnimationVisibility = Visibility.Collapsed;
                })
                { IsBackground = true };
            thread.Start();
        }

        protected void SaveNerualNetwork(object obj)
        {
            var thread = new Thread(() =>
                {
                    WaitLabelText = "Сохранение на диск...";
                    WaitAnimationVisibility = Visibility.Visible;
                    lock (Locker)
                    {
                        FileService.Save(SavePath, NerualNetworkProcessViewModel.Instance.NerualNetwork);
                    }
                    WaitAnimationVisibility = Visibility.Collapsed;
                    if (obj != null)
                        NavigationViewModel.Instance.ShowPageRelayCommand.Execute(obj);
                })
                { IsBackground = true };
            thread.Start();
        }
    }
}