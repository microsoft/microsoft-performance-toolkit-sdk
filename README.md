# Microsoft Performance Toolkit SDK

[![main build status](https://github.com/microsoft/microsoft-performance-toolkit-sdk/actions/workflows/main_build_status.yml/badge.svg)](https://github.com/microsoft/microsoft-performance-toolkit-sdk/actions/workflows/main_build_status.yml)

[![main tests status](https://github.com/microsoft/microsoft-performance-toolkit-sdk/actions/workflows/main_tests_status.yml/badge.svg)](https://github.com/microsoft/microsoft-performance-toolkit-sdk/actions/workflows/main_tests_status.yml)

The Microsoft Performance Toolkit is a collection of cross-platform tools developers can use to create 
and extend performance analysis applications. It serves as the runtime of the [Windows Performance Analyzer](https://docs.microsoft.com/en-us/windows-hardware/test/wpt/windows-performance-analyzer), 
a Windows program included in the [Windows Performance Toolkit](https://docs.microsoft.com/en-us/windows-hardware/test/wpt/). 
By using the Microsoft Performance Toolkit SDK, Windows Performance Anlazyer - or any performance analysis application - 
can be configured to process and display performance data from arbitrary sources.

The Microsoft Performance Toolkit SDK provides two key functionalities:
1) Facilitating the development of "SDK plugins," which provide the SDK with logic for creating structured, 
tabular data from arbitrary data sources such as Common Trace Format (`.ctf`) files
2) Allowing developers to extend an existing SDK plugin without access to its source code through an efficient,
feature-rich data-processing pipeline

These two functionalities are not mutually exclusive, and plugins may access data in another plugin's (or, commonly, its own) 
data processing pipeline when creating tables for a given data source.

For help with getting started and developing SDK plugins, refer to the [documentation folder](./documentation).

## In this Repository
* `devel-template`: a work-in-progress .NET template for creating SDK plugins
* `documentation`: instructions for how to utilize the SDK and create SDK plugins
* `samples`: example SDK plugins' source code
* `src`: the source code for the SDK

## Projects in the SDK Solution
* `Microsoft.Performance.SDK`: Software library for building SDK plugins
* `Microsoft.Performance.SDK.Runtime`: Runtime for loading and processing data from plugins
* `Microsoft.Performance.SDK.Runtime.NetCoreApp`: .NET Core version of `Microsoft.Performance.SDK.Runtime`
* `Microsoft.Performance.SDK.Toolkit.Engine`: Interface for programmatically manipulating, cooking, and accessing data from SDK plugins
* `Microsoft.Performance.SDK.Tests`: Tests for `Microsoft.Performance.SDK`
* `Microsoft.Performance.SDK.Runtime.Tests`: Tests for `Microsoft.Performance.SDK.Runtime`
* `Microsoft.Performance.SDK.Runtime.NetCoreApp.Tests`: Tests for `Microsoft.Performance.SDK.Runtime.NetCoreApp`
* `Microsoft.Performance.SDK.Toolkit.Engine.Tests`: Tests for `Microsoft.Performance.SDK.Toolkit.Engine`
* `Microsoft.Performance.SDK.Testing`: *description coming soon*
* `Microsoft.Performance.SDK.Testing.SDK`: *description coming soon*
* `PluginConfigurationEditor`: *description coming soon*

## Coming Soon

Team is actively working to provide better documentation, more samples, and several in-depth tutorials.

## How to Engage, Contribute, and Provide Feedback

The Performance ToolKit team encourages [contributions](CONTRIBUTING.md) through both issues and PRs.

### Community

This project uses the [Microsoft Open Source Code of Conduct.](https://opensource.microsoft.com/codeofconduct).

## Trademarks

This project may contain Microsoft trademarks or logos for Microsoft projects, products, or services. Authorized use of these trademarks or logos is subject to and must follow Microsoft’s Trademark & Brand Guidelines. Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.

## Security

To report a security issue, please follow the guidance here: [Security](SECURITY.md).

## Licenses

All code in this repository is licensed under the [MIT License](LICENSE.txt).

