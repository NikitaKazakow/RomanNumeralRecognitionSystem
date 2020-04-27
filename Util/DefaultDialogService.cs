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
            if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePath = openFolderDialog.FileName;
                return true;
            }
            return false;
        }

        public bool SaveFileDialog()
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() != true) return false;
            FilePath = saveFileDialog.FileName;
            return true;
        }
    }
}