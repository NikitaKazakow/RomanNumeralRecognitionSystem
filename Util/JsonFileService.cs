using System;
using System.IO;
using RomanNumeralRecognitionSystem.Model;
using System.Runtime.Serialization.Json;

namespace RomanNumeralRecognitionSystem.Util
{
    public class JsonFileService : IFileService<NerualNetwork>
    {
        public NerualNetwork Open(string fileName)
        {
            NerualNetwork nerualNetwork;
            var jsonSerializer = new DataContractJsonSerializer(typeof(NerualNetwork));

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                nerualNetwork = jsonSerializer.ReadObject(fs) as NerualNetwork;
            }

            return nerualNetwork;
        }

        public void Save(string fileName, NerualNetwork data)
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(NerualNetwork));
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                jsonSerializer.WriteObject(fs, data);
            }
        }
    }
}