using System;
using System.IO;
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
        private IFileService<NerualNetwork> FileService { get; }
        private IDialogService DialogService { get; }

        public string SaveFolder
        {
            get
            {
                if (_saveFolder == null)
                {
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                return Directory.Exists(_saveFolder) ? _saveFolder : "";
            }
            set
            {
                if (_saveFolder == value)
                    return;
                _saveFolder = value;
                OnPropertyChanged(nameof(SaveFolder));
            }
        }

        public string SaveName
        {
            get => _saveName ?? "MyNerualNetwork";
            set => _saveName = value;
        }

        public NerualNetwork NerualNetwork { get; set; }
        public CreateNerualNetworkViewModel()
        {
            FileService = new JsonFileService();
            DialogService = new DefaultDialogService();
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