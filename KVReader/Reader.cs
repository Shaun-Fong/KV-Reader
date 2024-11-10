using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

namespace KVReader
{
    public class Reader
    {
        // Stores the parsed key-value data
        private List<KVData> m_Data;

        public List<KVData> Data => m_Data;

        private List<string> _HeaderComments;

        // Dictionary to cache values grouped by key
        private Dictionary<string, List<string>> _KeyValueIndex;

        // Default constructor
        public Reader()
        {
            m_Data = new List<KVData>();
            _HeaderComments = new List<string>();
            _KeyValueIndex = new Dictionary<string, List<string>>();
        }

        // Indexer for querying by key
        public string[] this[string key]
        {
            get
            {
                if (_KeyValueIndex.ContainsKey(key))
                {
                    return _KeyValueIndex[key].ToArray();
                }
                return new string[0];  // Return an empty array if the key is not found
            }
        }

        /// <summary>
        /// Parse data from string content and populate the m_Data list
        /// </summary>
        /// <param name="content">Content</param>
        public void ParseFromString(string content)
        {
            var data = ParseDocumentFromString(content, out _HeaderComments);
            m_Data.Clear();
            foreach (var kv in data)
            {
                m_Data.Add(kv);
                // Create an index for each key
                if (!_KeyValueIndex.ContainsKey(kv.Key))
                {
                    _KeyValueIndex[kv.Key] = new List<string>();
                }
                _KeyValueIndex[kv.Key].Add(kv.Value);
            }
        }

        /// <summary>
        /// Parse data from a file path and populate the m_Data list
        /// </summary>
        /// <param name="path">File Path</param>
        public void ParseFromPath(string path)
        {
            string content = File.ReadAllText(path);
            ParseFromString(content);
        }

        /// <summary>
        /// Convert the m_Data list back into a formatted string representation
        /// </summary>
        /// <returns>Content</returns>
        public string ParseToString()
        {
            return FormatDocumentToString(m_Data, _HeaderComments);
        }

        /// <summary>
        /// Parse the document from a string and extract comments
        /// </summary>
        /// <param name="documentContent">content</param>
        /// <param name="headerComments">comments</param>
        /// <returns>result data</returns>
        public static List<KVData> ParseDocumentFromString(
            string documentContent,
            out List<string> headerComments)
        {
            var keyValuePairs = new List<KVData>();
            headerComments = new List<string>();
            bool isParsingComments = true;

            using (var reader = new StringReader(documentContent))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Identify and collect header comment lines
                    if (isParsingComments && line.TrimStart().StartsWith("//"))
                    {
                        headerComments.Add(line);
                        continue;
                    }

                    // Stop collecting comment lines
                    isParsingComments = false;

                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Split the line into parts
                    var parts = line.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length < 2)
                        continue;

                    string key = parts[0];
                    string value = parts[1];

                    keyValuePairs.Add(new KVData(key, value));
                }
            }

            return keyValuePairs;
        }

        /// <summary>
        /// Format the key-value pairs and comments back into a string
        /// </summary>
        /// <param name="keyValuePairs">data</param>
        /// <param name="headerComments">comments</param>
        /// <returns>content</returns>
        public static string FormatDocumentToString(
            List<KVData> keyValuePairs,
            List<string> headerComments)
        {
            var sb = new StringBuilder();

            // Add header comment lines
            foreach (var comment in headerComments)
            {
                sb.AppendLine(comment);
            }

            // Add an empty line to separate comments from content
            if (headerComments.Count > 0)
                sb.AppendLine();

            // Calculate the longest key length
            int maxKeyLength = 0;
            foreach (var kv in keyValuePairs)
            {
                if (kv.Key.Length > maxKeyLength)
                {
                    maxKeyLength = kv.Key.Length;
                }
            }

            // Set the space between Key and Value
            int spacing = maxKeyLength + 4;  // Generous spacing with 4 spaces

            // Add the key-value pair lines with proper alignment
            foreach (var kv in keyValuePairs)
            {
                // Calculate the required number of spaces to ensure alignment
                int padding = spacing - kv.Key.Length;
                sb.AppendLine($"{kv.Key}{new string(' ', padding)}{kv.Value}");
            }

            return sb.ToString();
        }
    }
}
