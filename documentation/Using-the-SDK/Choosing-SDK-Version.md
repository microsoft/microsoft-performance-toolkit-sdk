# Choosing an SDK Version

The SDK is versioned using semantic versioning and will only load plugins that were compiled against a build of the SDK that is semantically compatible with the running version. Concretely, this means

- If your plugin is compiled against SDK version `a.b.c`
- Your plugin is being loaded by SDK version `x.y.z`

then

- If `a != x` (i.e. the major versions do not match), the plugin will not load
- If `b > y` (i.e. the minor of the plugin is higher) the plugin will not load

Another way to say this is "a plugin can only be loaded if it uses the same major version of the SDK and it was compiled against an earlier version of that major version." Note that `c` and `z` are **not** considered, since patch version numbers are only concerned with non-API breaking bug fixes.

---

When choosing which version of the SDK to compile your plugin against, there are therefore two competing tradeoffs to consider:

- Maximizing compatibility with SDK drivers (e.g. Windows Performance Analyzer)
- Using features introduced in newer SDK versions

This tradeoff exists because if you use a later version of the SDK (e.g. `1.1.x` instead of `1.0.x`), you gain access to the new features introduced in those versions, but prevent yourself from being loaded by drivers targeting earlier versions of the SDK (e.g. you wouldn't load in any version of WPA built against SDK `1.0.x`).

For example, if you compile against SDK version `1.0.27`, your plugin can be loaded by WPA versions `11.0.7` or higher. However, if you compile against SDK versions `1.1.24`, you can only be loaded by WPA versions `11.5.29` or higher.

> The reason a plugin compiled against a later version of the SDK in a program using an earlier version is that your plugin may attempt to access or use APIs that are not available in the earlier version that is currently loaded. *Even if a plugin doesn't use the APIs introduced in later versions, the SDK will refuse to load it.*

The recommended way to choose the SDK version to compile your plugin against is therefore
1. Pick the **latest major version number** (e.g. `1.x.x`), unless you specifically want to target older versions of SDK drivers
2. By referring to the [release changelog](https://github.com/microsoft/microsoft-performance-toolkit-sdk/releases), pick the **earliest minor version number that has all the features you need** (e.g. `1.0.x`)
3. Pick the **latest patch version number** (e.g. `1.0.27`)

> NOTE: In the above steps, "major version number" refers to the first number in a version, "minor version number" the second, and "patch version number" the third.

# Testing with an SDK Driver

Once you pick an SDK version, you can test your plugin with any version of an SDK driver compatible with the version number you picked. For a list of known SDK driver compatibility, refer to [Known SDK Driver Compatibility](../Known-SDK-Driver-Compatibility/README.md).