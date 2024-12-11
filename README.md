<p align="center">
<h1 align="center"><b>KV-Reader</b></h1>
<br>
</p>

<p align="center">
<b>Key-Value file reader.</b>
<br>
This is a simple key-value structure file parser, which can be used for various purposes, such as configuration files, do whatever you want.
</p>

<p align="center">
<img src="https://github.com/user-attachments/assets/4194df4d-805c-434e-9a7b-b1b1d86714ad">
</p>

# Install
For Unity, download package from [here](https://github.com/Shaun-Fong/KV-Reader/releases/latest/download/com.shaunfong.kvreader.unitypackage).

For Dll, download from [here](https://github.com/Shaun-Fong/KV-Reader/releases/latest/download/ShaunFong.KVReader.dll).

# K-V File

The typical K-V file is as follows:

```
// Comment line1
// Comment line2
// Comment line3

KEY          VALUE1
KEY1         VALUE2
```

This tiny library can parse almost any file :

![image](https://github.com/user-attachments/assets/e21f8b58-6624-4b88-9f17-2cfd9183a4ef)

![image](https://github.com/user-attachments/assets/0c24fe55-6bb6-4418-a07e-0f1f55d6c07d)


# Usage

Open `KV Reader` Tool by clicking `Tools/KV Reader`

![image](https://github.com/user-attachments/assets/11567ebe-f3f5-4e98-a90b-d5ef9d8b3c96)


``` C#
// Reader Parse
Reader reader = new Reader();

// Parse
reader.ParseFromString(File.text);

// Print
Console.WriteLine(reader["KEY"][0]);
```

``` C#
string test = @"
// Comment
KEY1 VALUE1
KEY2 VALUE2
";

// Parse
var data = Reader.ParseDocumentFromString(test, out List<string> headComments);

// Read
Console.WriteLine(data[1].Key + " " + data[1].Value);

// Change Value
data[1].Key = "KEY_CHANGE";
data[1].Value = "VALUE_CHANGE";

// Format
var result = Reader.FormatDocumentToString(data, headComments);

// Print Result
Console.WriteLine(result);
```
