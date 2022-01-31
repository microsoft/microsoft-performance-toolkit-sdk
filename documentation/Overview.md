# Abstract
The Microsoft Performance Toolkit SDK allows developers to create "SDK plugins" that process and interpret data. 
These plugins can be used by performance analysis applications that use the SDK runtime - such as [Windows Performance Analyzer](https://docs.microsoft.com/en-us/windows-hardware/test/wpt/windows-performance-analyzer) - to process data sources into structured, tabular data. Plugins may utilize the SDK's data-processing pipeline to both facilitate the creation of these tables and expose data that can be programmatically accessed by concurrently loaded plugins (even those without access to the originating plugin's source code).

This folder contains instructions for developing SDK plugins and utilizing the SDK's various features.

# Requirements
The following are required in order to develop an SDK plugin:
* [NuGet](https://www.nuget.org/downloads)
* [.NET Standard 2.0](https://dotnet.microsoft.com/download/visual-studio-sdks)
* A text editor (for editing your source code)

It is recommended to use [Visual Studio](https://visualstudio.microsoft.com/downloads/) to develop an SDK plugin since it satisfies all three requirements. Future documentation assumes the use of Visual Studio, but the instructions may be adapted for other editors/IDEs.

# Getting the SDK
The SDK is currently published as a NuGet package under the name [Microsoft.Performance.SDK](https://www.nuget.org/packages/Microsoft.Performance.SDK/). 
Since it is hosted on [NuGet.org](https://www.nuget.org/), it can be added to a `csproj` with no additional configuration by using 
the Visual Studio NuGet Package Manager, `dotnet.exe`, or `nuget.exe`.

# Creating Your First Project
Please see the [Creating your first project](Using-the-SDK/Creating-your-project.md)

# Recommended Reading Order
To best understand how the SDK works and how to develop SDK plugins, it is recommended to read documentation in the following order:
1) [Architecture/Overview](./Architecture/Overview.md) to understand at a high level the various system the SDK provides
2) [Architecture/The Data Processing Pipeline](./Architecture/The-Data-Processing-Pipeline.md) to understand how to systematically process data that 
can be used by tables
3) [Using the SDK/Creating an SDK Plugin C# Project](Using-the-SDK/Creating-your-project.md) to get your developer environment ready to create an SDK plugin
4) [Using the SDK/Creating a Simple SDK Plugin](Using-the-SDK/Creating-a-simple-sdk-plugin.md) to see how to create a basic plugin that can 
take in a specific data source and output structured tables
5) [Using the SDK/Creating a Data Processing Pipeline](Using-the-SDK/Creating-a-pipeline.md) to see how to create a data processing pipeline that 
exposes data that can be consumed by your tables and other plugins
6) [Using the SDK/Creating an Extended Table](Using-the-SDK/Creating-an-extended-table.md) to see how to use data cookers to obtain the data to display 
inside of a table