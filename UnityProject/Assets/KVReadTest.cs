using UnityEngine;
using KVReader;
using System.Collections.Generic;
using System;

public class KVReadTest : MonoBehaviour
{

    public TextAsset File;

    void Start()
    {
        Reader reader = new Reader();

        reader.ParseFromString(File.text);

        Debug.Log(reader["KEY"][0]);

        string test = @"
// Comment
KEY1 VALUE1
KEY2 VALUE2
";

        // Parse
        var data = Reader.ParseDocumentFromString(test, out List<string> headComments);

        // Read
        Debug.Log(data[1].Key + " " + data[1].Value);

        // Change Value
        data[1].Key = "KEY_CHANGE";
        data[1].Value = "VALUE_CHANGE";

        // Format
        var result = Reader.FormatDocumentToString(data, headComments);

        // Print Result
        Debug.Log(result);
    }
}
