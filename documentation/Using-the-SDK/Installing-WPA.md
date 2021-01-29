# Windows Performance Analyzer

Windows Performance Analyzer (WPA) is used to open data sources (usually files) for visual analysis. It loads SDK Plugins to process data sources. When developing an SDK Plugin, WPA is a useful debugging tool.

# Installation

WPA is bundled as part of the [Windows Performance Toolkit](https://docs.microsoft.com/en-us/windows-hardware/test/wpt/) which may be installed as part of the [Windows Assessment and Deployment Kit](https://docs.microsoft.com/en-us/windows-hardware/get-started/adk-install).

- Download and run the setup program for the Windows Assessment and Deployment Kit.
- Chose an installation path and follow installation insructions until you reach the "Select the features you want to install" screen.
  * Take note of the installation path as it will be used when debugging SDK Plugin projects.
- Make sure to select "Windows Performance Toolkit" from this screen.  ![ADK_Installation_Markup.png](/.attachments/ADK_Installation_Markup.png)
- Click Install and Close when installation is complete.