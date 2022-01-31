# Creating an SDK Plugin C# Project

This document outlines how to use the Performance Toolkit SDK (SDK) to create
an SDK plugin. A plugin can be used for processing trace files to be used by
automation, trace extractors, or viewers such as Windows Performance Analyzer. 
This document will cover:
1) [Requirements](#reqs)
2) [Creating your project](#createproj)
3) [Configuring your project to launch WPA with your plugin under a debugger](#configure)



For simplicity, this section will assume you are using Visual Studio. The instructions may be adapted for other editors / IDEs.

<a name="reqs"></a>
# Requirements

1. [Visual Studio](https://visualstudio.microsoft.com/downloads/)
2. [.NET SDK that supports .NET Standard 2.0](https://dotnet.microsoft.com/download/visual-studio-sdks)
   * [See .NET Standard 2.0 support options](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

<a name="createproj"></a>
# Creating Your Project

1) Launch Visual Studio
2) Click "Create new project"  
 ![VS2019_Create_New_Project.PNG](./.attachments/VS2019_CreateProject_Markup.png)
3) Select .NET Standard on the left, and choose "Class Library (.NET Standard)." Make sure that you are using .NET Standard 2.0 
 ![VS2017_New_DotNetStandard_20_Project.PNG](./.attachments/VS2019_CreateProject_ClassLibrary_Markup.png)
4) Name your project based on the name of the SDK plugin you are creating
5) Click "Create"

<a name="configure"></a>
# Configuring Your Project

You should now have a solution with one project file.

## Add the Microsoft.Performance.SDK NuGet Package

[This documentation](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio) describes how to add a NuGet package to a Visual Studio project. Following these instructions, add the `Microsoft.Performance.SDK` package from [nuget.org](nuget.org) to your project.

### Picking your SDK version
The version of the SDK you add to your project will determine which versions of SDK drivers your plugin will work with. For example, a plugin that depends on SDK version 0.109.2 will not work a version of an SDK driver that uses SDK version 1.0.0.

To decide which version of the SDK to use, refer to the [known SDK driver compatability lists](../Known-SDK-Driver-Compatibility/Overview.md).

## Install WPA for Debugging

One way to debug an SDK plugin project is to use WPA. Before we setup our project for this, WPA will need to be installed. 
Please see [Using the SDK/Installing WPA](./Installing-WPA.md) for more information how to install WPA.

## Setup for Debugging Using WPA

1) Right click your project and select "Properties"
2) Select the "Debug" tab on the left
   * In newer versions of Visual Studio, you may need to click "Open debug launch profiles UI"
3) For "Launch", select "Executable"
4) For the "Executable", place the path to the `wpa.exe` that you previously installed as part of the WPT
   * Typically this might be: `C:\Program Files (x86)\Windows Kits\10\Windows Performance Toolkit\wpa.exe`
5) For "Command line Arguments", add `-addsearchdir <bin folder for your plugin>` (e.g. `-addsearchdir C:\MyAddIn\bin\Debug\netstandard2.1`)

# Next Steps

The project is now created and configured and it is time to start writing the SDK plugin. See one of the following documents for further help:

* [Using the SDK/Creating a Simple SDK Plugin](./Creating-a-simple-sdk-plugin.md) to see how to create a basic plugin that can take in a specific data source and output structured tables
* [Using the SDK/Creating a Data Processing Pipeline](./Creating-a-pipeline.md) to see how to create a data processing pipeline that 
exposes data that can be consumed by your tables and other plugins
* [Using the SDK/Creating an Extended Table](./Creating-an-extended-table.md) to see how to use data cookers to obtain the data to display inside of a table