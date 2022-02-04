# Overview
The Microsoft Performance Toolkit SDK allows developers to create "SDK plugins" that process and interpret data. 
These plugins can be used by performance analysis applications that use the SDK runtime - such as [Windows Performance Analyzer](https://docs.microsoft.com/en-us/windows-hardware/test/wpt/windows-performance-analyzer) - to process data sources into structured, tabular data. Plugins may use the SDK's data-processing pipeline to both facilitate the creation of these tables and expose data that can be programmatically accessed by concurrently loaded plugins (even those without access to the originating plugin's source code).

This folder contains instructions for developing SDK plugins and utilizing the SDK's various features.

# Quick Start
Refer to [Creating your first plugin](Using-the-SDK/Creating-your-plugin.md) to jump in quickly.

> ⚠️ We recommend following the [Recommended Reading Order](#recommended-reading-order).

# Glossary

For quick definitions/examples of terms and concepts, refer to the [Glossary](./Glossary.md). 

# Requirements
The following are required in order to develop an SDK plugin:
* [NuGet](https://www.nuget.org/downloads)
* [.NET Standard 2.0](https://dotnet.microsoft.com/download/visual-studio-sdks) or [compatible version](https://docs.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-1-0#tabpanel_1_net-standard-2-0)
* A text editor (for editing your source code)

It is recommended to use [Visual Studio](https://visualstudio.microsoft.com/downloads/) to develop an SDK plugin since it satisfies all three requirements. Future documentation assumes the use of Visual Studio, but the instructions may be adapted for other editors/IDEs.

# Getting the SDK
The SDK is published as a NuGet package under the name [Microsoft.Performance.SDK](https://www.nuget.org/packages/Microsoft.Performance.SDK/). 
Since it is hosted on [NuGet.org](https://www.nuget.org/), it can be added to a `csproj` with no additional configuration by using 
the Visual Studio NuGet Package Manager, `dotnet.exe`, or `nuget.exe`.

# Recommended Reading Order
To best understand how the SDK works and how to develop SDK plugins, it is recommended to read documentation in the following order:
1) [Architecture/Overview](./Architecture/README.md) to understand at a high level the various systems the SDK provides
3) [Architecture/The Data-Processing Pipeline](./Architecture/The-Data-Processing-Pipeline.md) to understand how to systematically process data that 
can be used by tables
3) [Creating your first plugin](Using-the-SDK/Creating-your-plugin.md) to learn how to create an SDK plugin
