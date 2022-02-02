# Troubleshooting

This document outlines steps for troubleshooting common issues that arise when developing SDK plugins.

* [WPA-Related Issues](#wpa-related-issues)
  * [WPA does not load my plugin](#wpa-does-not-load-my-plugin)

## WPA-Related Issues

### WPA does not load my plugin

There are many reasons this could happen. Here are some things to check:

* Check WPA's diagnostic console to see if there were any errors when loading your plugin. From within WPA, select `Window` -> `Diagnostic Console`
* Ensure Visual Studio is correctly setup to load your plugin when debugging. Follow [these steps](./Using-the-SDK/Creating-your-plugin.md#setup-for-debugging-using-wpa)
* Ensure your plugin builds successfully. In Visual Studio, select `Build` -> `Rebuild Solution`. Or, from a command prompt in your plugin's folder, run `dotnet build`
* Ensure the `-addsearchdir` path in your debug profile created during [these steps](./Using-the-SDK/Creating-your-plugin.md#setup-for-debugging-using-wpa) is correct. Manually navigate to the folder used for `-addsearchdir` and verify your DLLs are there
* If your `-addsearchdir` path contains spaces, ensure the path is surrounded by quotes (`"`)
* Ensure the SDK version your plugin uses is [compatible with your version of WPA](./Known-SDK-Driver-Compatibility/WPA.md). To find your WPA version, select `Help` -> `About Windows Performance Analyzer`