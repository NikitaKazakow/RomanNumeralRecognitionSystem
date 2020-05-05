using System;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace RomanNumeralRecognitionSystem.Util
{
    public class DefaultDialogService : IDialogService
    {
        public string FilePath { get; set; }
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        public bool OpenFolderDialog()
        {
            var openFolderDialog = new CommonOpenFileDialog
            {
                InitialDirectory = "/",
                IsFolderPicker = true
            };
            if (openFolderDialog.ShowDialog() != CommonFileDialogResult.Ok) return false;
            FilePath = openFolderDialog.FileName;
            return true;
        }

        public bool OpenFileDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "Nerual network files (*.nrnw)|*.nrnw",
                FilterIndex = 0
            };
            if (openFileDialog.ShowDialog() != true) return false;
            FilePath = openFileDialog.FileName;
            return true;
        }
    }
}