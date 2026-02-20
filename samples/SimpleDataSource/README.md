# Introduction 
This sample plugin consists of a single Processing Source that understands files with the `.txt` extension and a Custom Data Processor that processes simple text files with the following format:

```
2/4/2019 9:40:00 AM, Word1 Word2 Word3 ...  
2/4/2019 9:41:00 AM, Word4 Word5 Word6 ...  
2/4/2019 9:42:00 AM, Word7 Word8 Word9 ...  
2/4/2019 9:43:00 AM, Word10 Word11 Word12 ...
```

See `SampleTextFile.txt` for an example of a file with this format.

# Requirements
1. [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
2. [.NET Standard 2.0](https://dotnet.microsoft.com/download/visual-studio-sdks)

# Instructions
Please refer to [Using the SDK/Creating a Simple SDK Plugin](../../documentation/Using-the-SDK/Creating-a-simple-sdk-plugin.md) for details 
on how to implement this source code.