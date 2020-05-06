using System;
using System.Windows;
using System.Threading;
using RomanNumeralRecognitionSystem.Model;
using RomanNumeralRecognitionSystem.Util;

namespace RomanNumeralRecognitionSystem.ViewModel
{
    public class CreateNerualNetworkViewModel : NerualNetworkViewModelBase
    {
        private RelayCommand _openFolderDialogRelayCommand;
        private RelayCommand _openFileDialogRelayCommand;
        private RelayCommand _createNerualNetworkRelayCommand;

        private IFileService<NerualNetwork> FileService { get; }
        private IDialogService DialogService { get; }
        
        public CreateNerualNetworkViewModel()
        {
            FileService = new BinaryFileService();
            DialogService = new DefaultDialogService();

            WaitAnimationVisibility = Visibility.Collapsed;
        }
        
        public RelayCommand OpenFolderDialogRelayCommand
        {
            get
            {
                return _openFolderDialogRelayCommand ?? (_openFolderDialogRelayCommand = new RelayCommand(obj =>
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
        public RelayCommand OpenFileDialogRelayCommand
        {
            get
            {
                return _openFileDialogRelayCommand ?? (_openFileDialogRelayCommand = new RelayCommand(obj =>
                {
                    try
                    {
                        if (!DialogService.OpenFileDialog()) return;
                        SaveName = DialogService.FilePath;
                        WaitAnimationVisibility = Visibility.Visible;
                        WaitLabelText = "Открытие файла...";
                        var thread = new Thread(() =>
                            {
                                NerualNetworkProcessViewModel.Instance.NerualNetwork = FileService.Open(DialogService.FilePath);
                                WaitAnimationVisibility = Visibility.Collapsed;
                                SavePath = DialogService.FilePath;
                                NavigationViewModel.Instance.ShowPageRelayCommand.Execute(obj);
                            })
                            { IsBackground = true };
                        thread.Start();
                    }
                    catch (Exception e)
                    {
                        DialogService.ShowMessage(e.Message);
                    }
                }));
            }
        }
        public RelayCommand CreateNerualNetworkRelayCommand
        {
            get
            {
                return _createNerualNetworkRelayCommand ?? (_createNerualNetworkRelayCommand = new RelayCommand(obj =>
                {
                    CreateNerualNetwork();
                    SaveNerualNetwork(obj);
                }));
            }
        }
    }
}