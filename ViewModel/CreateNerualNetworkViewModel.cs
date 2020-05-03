using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using RomanNumeralRecognitionSystem.Model;
using RomanNumeralRecognitionSystem.Util;

namespace RomanNumeralRecognitionSystem.ViewModel
{
    public class CreateNerualNetworkViewModel : ViewModelBase
    {
        private RelayCommand _saveRelayCommand;
        private RelayCommand _openRelayCommand;

        private string _saveFolder;
        private string _saveName;

        private bool _isValid;
        private bool _isValidLayersCount;
        private bool _isValidNeuronCount;

        private ObservableCollection<HiddenLayerViewModel> _hiddenLayersCollection;

        private Visibility _folderErrorVisibility;
        private Visibility _nameErrorVisibility;
        private Visibility _fileAlreadyExistVisibility;

        private IFileService<NerualNetwork> FileService { get; }
        private IDialogService DialogService { get; }

        public static double MaxHiddenLayersCount { get; private set; }

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
                var name = "MyNerualNetwork" + i + ".json";
                while (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    name)))
                {
                    i++;
                    name = "MyNerualNetwork" + i + ".json";
                }
                _saveName = "MyNerualNetwork" + i;
                IsValidLayersCount = true;
                NameErrorVisibility = 
                    FileAlreadyExistVisibility =Visibility.Collapsed;
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
                else if (File.Exists(Path.Combine(SaveFolder, value + ".json")))
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
                _isValid = IsValidNeuronCount && IsValidLayersCount;
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

        public NerualNetwork NerualNetwork { get; set; }
        public CreateNerualNetworkViewModel()
        {
            FileService = new JsonFileService();
            DialogService = new DefaultDialogService();

            MaxHiddenLayersCount = 3;
            IsValidLayersCount = true;
            IsValidNeuronCount = true;
        }
        
        public RelayCommand OpenRelayCommand
        {
            get
            {
                return _openRelayCommand ?? (_openRelayCommand = new RelayCommand(obj =>
                {
                    try
                    {
                        if (DialogService.OpenFolderDialog())
                            SaveFolder = DialogService.FilePath;
                    }
                    catch (Exception e)
                    {
                        DialogService.ShowMessage(e.Message);
                    }
                }));
            }
        }

        public RelayCommand SaveRelayCommand
        {
            get
            {
                return _saveRelayCommand ?? (_saveRelayCommand = new RelayCommand(obj =>
                {
                    try
                    {
                        if (!DialogService.SaveFileDialog()) return;
                        FileService.Save(DialogService.FilePath, NerualNetwork);
                        DialogService.ShowMessage("Файл сохранен!");
                    }
                    catch (Exception e)
                    {
                        DialogService.ShowMessage(e.Message);
                    }
                }));
            }
        }
    }
}