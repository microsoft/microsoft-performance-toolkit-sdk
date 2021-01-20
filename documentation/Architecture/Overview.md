# Architecture Overview

This document outlines the architecture of the Microsoft Performance Toolkit SDK.

For more detailed information on how to create your own project using the SDK, please view [Creating your own project](./Using-the-SDK/Creating-your-project.md). 


For an example of a custom data source, please view [Custom Data Source Example](../samples/SimpleDataSource/SimpleCustomDataSource.cs).
For an example of a custom data processor, please view [Custom Data Processor Example](../samples/SimpleDataSource/SimpleCustomDataProcessor.cs). 


# Microsoft Performance Toolkit SDK

The Microsoft Performance Toolkit SDK was built to empower users to analyze any arbitrary data source (e.g. .etl, .txt, lttng, etc). Using the SDK, any data source (including large files) can be quickly processed to generate custom graphs and tables.
Below is a brief overview of the structure of the SDK.




![](./Architecture/.attachments/FileOpenOverview.png)

Discuss how WPA, SDK, CDS

# Driver

The Driver's main objective is to render data for a user to consume and interact with. The user can select any data source which will be sent down to the SDK, which will in turn return tables for visualization.
We recommend using Windows Performance Analyzer (WPA) to interact with the data as it grants a plethora of tools for analysis.


# SDK

The SDK is primarily responsible for building relevant tables and graphs. Using logic from the [Data Processor](./Architecture/The-Data-Processing-Pipeline.md), the SDK creates custom tables and/or graphs.


# Custom Data Source

[Architecture/The Custom Data Source](./Architecture/The-Custom-Data-Source-Model.md) is a native binary which can parse the Data Source. A Custom Data Source (CDS) advertises the data source which it can parse.
If the relevant binaries exist, the CDS has the logic for creating tables from the data source in a . 
One or more Custom Data Sources is a plugin. Plugins are compiled into binaries which are loaded into the SDK to handle table building.


Step-by-step
1) Select Data Source in Driver
2) Driver passes Data Source to SDK
3) SDK passes Data Source to the Custom Data Source
4) Custom Data Source validates Data Source and sends plugin (compiled binaries) to SDK
5) SDK creates default/custom tables based on the plugin logic  
6) Driver displays visualized data for interaction and manipulation
