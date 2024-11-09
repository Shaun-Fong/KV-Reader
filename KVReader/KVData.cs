using System;

namespace KVReader
{

    [System.Serializable]
    public class KVData
    {
        public string Key;
        public string Value;

        public KVData(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
