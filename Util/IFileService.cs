namespace RomanNumeralRecognitionSystem.Util
{
    public interface IFileService<T>
    {
        T Open(string fileName);
        void Save(string fileName, T data);
    }
}