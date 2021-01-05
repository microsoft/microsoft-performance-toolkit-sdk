# Introduction 
The Performance Toolkit SDK allows for you to create plugins to process your own data to be used by viewers like Windows Performance Analyzer (WPA). This project is a sample plugin that you can use as a reference for creating your own plugins. 
The sample plugin contains a Custom Data Source (CDS) that understands files with the `.txt` extension and a Custom Data Processor that processes simple text files with the following format:

```
2/4/2019 9:40:00 AM, Word1 Word2 Word3 ...  
2/4/2019 9:41:00 AM, Word4 Word5 Word6 ...  
2/4/2019 9:42:00 AM, Word7 Word8 Word9 ...  
2/4/2019 9:43:00 AM, Word10 Word11 Word12 ...
```

See `SampleTextFile.txt`

# Requirements
1. [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
2. [.NET Standard 2.0](https://dotnet.microsoft.com/download/visual-studio-sdks)

# Nuget Package

https://www.nuget.org/packages/Microsoft.Performance.SDK/

# Instructions

You can follow the instruction our Wiki page on how to create your own AddIn. You can find other topics on how to make use of the SDK including graphing, table configuration, command line options and so on.

**_NOTE:_** Below links are place holders

1. [Overview](https://github.com/microsoft/microsoft-performance-toolkit-sdk/wiki)
1. [Creating your project](https://github.com/microsoft/microsoft-performance-toolkit-sdk/wiki)
2. [Creating your AddIn](https://github.com/microsoft/microsoft-performance-toolkit-sdk/wiki)
3. [Graphing](https://github.com/microsoft/microsoft-performance-toolkit-sdk/wiki)
4. [Advanced Topics](https://github.com/microsoft/microsoft-performance-toolkit-sdk/wiki)
