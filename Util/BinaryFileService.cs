using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using RomanNumeralRecognitionSystem.Model;

namespace RomanNumeralRecognitionSystem.Util
{
    public class BinaryFileService : IFileService<NerualNetwork>
    {
        public NerualNetwork Open(string fileName)
        {
            var formatter = new BinaryFormatter();
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                return (NerualNetwork) formatter.Deserialize(fs);
            }
        }

        public void Save(string fileName, NerualNetwork data)
        {
            var formatter = new BinaryFormatter();
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                formatter.Serialize(fs, data);
            }
        }
    }
}