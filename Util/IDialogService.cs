namespace RomanNumeralRecognitionSystem.Util
{
    public interface IDialogService
    {
        string FilePath { get; set; }
        void ShowMessage(string message);
        bool OpenFolderDialog();
        bool SaveFileDialog();
    }
}