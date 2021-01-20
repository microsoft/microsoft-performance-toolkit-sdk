# Creating an SDK Plugin C# Project

This document outlines how to use the Performance ToolKit SDK (SDK) to create
an SDK plugin. A plugin can be used for processing trace files to be used by
automation, trace extractors, or viewers such as Windows Performance Analyzer
(WPA). This document will cover:
1) Requirements
2) Creating your project
3) Configuring your project to launch WPA with your plugin under a debugger

For simplicity, this section will assume you are using Visual Studio 2019. The instructions may be adapted for other editors / IDEs.

# Requirements

1. [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
2. [.NET Standard 2.0](https://dotnet.microsoft.com/download/visual-studio-sdks)

## Visual Studio 2019

You can install Visual Studio 2019 from here: https://visualstudio.microsoft.com/
Install the version you desire. We recommend _Enterprise 2019_

1) Download the installer
2) Execute the installer
3) Make sure that the following features are selected:
  3a) .NET Desktop Development (Make sure .NET Core 3.1 development tools is selected)
4) Click Install
5) You may have to restart your computer when installation finishes.

## .NET Standard 2.0

If you installed Visual Studio 2019 based on the above instructions, then you are good to go. Otherwise,

1) Execute your VS2019 installer, and choose to "Modify" the installation.
2) Make sure that the following features are selected:
* .NET Desktop Development (Make sure .NET Core 3.1 development tools is selected)
3) Click "Modify" to modify your installation.
4) You may need to restart your computer when modification finishes.

## NuGet Feeds

TODO: add VS instructions here

# Creating the Project

1) Launch Visual Studio 2019
2) Click "Create new project"
 ![VS2017_Create_New_Project.PNG](/.attachments/VS2017_Create_New_Project-f151d280-ecd7-45be-bde0-6ac5f23340e8.PNG)
3) Select .NET Standard on the left, and choose "Class Library (.NET Standard)." Make sure that you are using .NET Standard 2.1
 ![VS2017_New_DotNetStandard_20_Project.PNG](/.attachments/VS2017_New_DotNetStandard_20_Project-33e8f885-59cc-436b-bb41-ec56b872d42d.PNG)
4) Give your project whatever name you want.
5) Click "OK"

# Configuring Your Project

You should now have a solution with one project file.

## Latest SDK Versions (>= 0.100.0)

For versions of the SDK greater than 0.100.0, we are working on an automated template, for the time being:

- Install the SDK NuGet package into your project.
- Navigate to [this page](/Using-the-SDK/SDK-%2D-WPA) and find the version of WPA for the version of the SDK you installed.
- Extract this version of WPA to a well known location.
- Right click your project
- Select the 'Debug' tab on the left.
- For 'Launch', select 'Executable'
- For the 'Executable', place the path to the wpa.exe that you previously downloaded
- For 'Command line Arguments', add '-addsearchdir [bin location for your plug-in]' (e.g. -addsearchdir C:\MyAddIn\bin\Debug\netstandard2.1)
