# Microsoft Performance Toolkit SDK - Copilot Instructions

## Repository Overview
The Microsoft Performance Toolkit SDK is a cross-platform C# library that enables developers to create "SDK plugins" for processing and analyzing performance data from arbitrary data sources (ETL files, CTF files, SQL logs, CSV files, etc.). These plugins can be used by performance analysis applications like Windows Performance Analyzer (WPA).

**Repository Stats:**
- **Languages:** C# (.NET)
- **Project Type:** Class library SDK with NuGet packages
- **Target Frameworks:** .NET Standard 2.0, .NET Standard 2.1, .NET 6.0, .NET 8.0
- **Solution:** Single main solution at `src/SDK.sln` with 17+ projects
- **Size:** Medium-sized repository with SDK core, runtime, engine, plugins system, tools, and comprehensive test coverage

## Build and Test Instructions

### Prerequisites
- **.NET SDK 8.x** (or compatible version that supports .NET Standard 2.0)
- **Nerdbank.GitVersioning** tool version 3.6.143 (automatically restored via NuGet)
- **Windows OS** (primary target platform, uses Windows-specific APIs in some areas)

### Build Commands (VALIDATED - ALWAYS USE THESE)

**CRITICAL: Always run commands in the following order and configuration:**

1. **Restore Dependencies** (Optional but recommended for clean builds):
   ```powershell
   dotnet restore "src/SDK.sln"
   ```
   - Can be skipped; `dotnet build` will restore automatically
   - Always restore before building if you want `--no-restore` flag benefits

2. **Build the Solution** (REQUIRED):
   ```powershell
   dotnet build "src/SDK.sln" --no-restore -c Release
   ```
   - **Configuration:** Always use `-c Release` for production builds
   - **Expected Warnings:** 
     - ~22 warnings about obsolete interfaces (IApplicationEnvironmentV2, IApplicationEnvironmentV3) - THESE ARE EXPECTED
   - Build **will succeed** even with these warnings
   - Without `--no-restore`: Auto-restores first

3. **Run Tests** (REQUIRED for validation):
   ```powershell
   dotnet test "src/SDK.sln" -c Release --filter "TestCategory~Unit|TestCategory~Integration|TestCategory~Functional" --no-build --verbosity normal
   ```
   - **Expected Results:** 712+ tests pass, 0-1 skipped, 0 failures
   - **Filter:** Only runs Unit, Integration, and Functional tests (not all tests are categorized)
   - **Important:** Use `--no-build` to run against already-built assemblies
   - Some test projects show warnings about no matching tests - THIS IS EXPECTED for PluginsSystem test projects

4. **Build Sample Projects** (Optional validation):
   ```powershell
   Get-ChildItem "samples" -Filter *.sln -Recurse | ForEach-Object { dotnet build $_.FullName -c Release }
   ```
   - Builds `SimpleDataSource/SampleAddIn.sln` and `SqlPlugin/SqlPlugin.sln`
   - Validates plugin development patterns

### Clean Build
To start fresh:
```powershell
dotnet clean "src/SDK.sln"
dotnet build "src/SDK.sln" -c Release
```
Note: `dotnet build` without restore will auto-restore.

## Project Structure and Key Files

### Root Directory Structure
```
.github/              # CI/CD workflows and GitHub configuration
  workflows/
    ci.yml            # PR validation: build + test + samples
    main_build_status.yml   # Main branch build status
    main_tests_status.yml   # Main branch test status
src/                  # Main solution and all projects
  SDK.sln             # Primary solution file
  Directory.Build.props     # Shared MSBuild properties (Nerdbank.GitVersioning)
  Directory.Build.targets   # Shared MSBuild targets (test package versions)
  version.json        # Version configuration (Nerdbank.GitVersioning)
  nuget.config        # NuGet package source configuration
samples/              # Example SDK plugins
  SimpleDataSource/   # Basic text file processing plugin
  SqlPlugin/          # SQL trace file processing plugin
documentation/        # Comprehensive SDK usage documentation
  Using-the-SDK/      # Plugin development guides
  Architecture/       # SDK architecture deep-dives
devel-template/       # .NET template for creating SDK plugins (WIP)
```

### Key Projects in src/SDK.sln

**Core SDK Libraries (Plugins depend on these):**
- `Microsoft.Performance.SDK` - Core SDK library (.NET Standard 2.0) - **This is what plugins reference**
- `Microsoft.Performance.SDK.Runtime` - Runtime for loading/processing plugins (.NET Standard 2.0) - **Plugins should NOT depend on this**
- `Microsoft.Performance.SDK.Runtime.NetCoreApp` - .NET Core specific runtime (multi-targeted: net6.0, net8.0)
- `Microsoft.Performance.Toolkit.Engine` - Programmatic interface for manipulating plugin data (.NET Standard 2.1)

**Plugins System:**
- `Microsoft.Performance.Toolkit.Plugins.Core` - Core plugin system abstractions (.NET Standard 2.0)
- `Microsoft.Performance.Toolkit.Plugins.Runtime` - Plugin system runtime (.NET Standard 2.0)
- `Microsoft.Performance.Toolkit.Plugins.Cli` - CLI tool for plugin management (net8.0)

**Test Projects:** All use .NET 8.0, named with `.Tests` suffix

**Tools:**
- `PluginConfigurationEditor` - GUI tool for editing plugin configuration (net8.0)

### Configuration Files
- `src/version.json` - Nerdbank.GitVersioning config, current version: "1.4-preview2"
- `src/Directory.Build.props` - Sets Nerdbank.GitVersioning 3.6.143 for all projects
- `src/Directory.Build.targets` - Centralizes test framework versions (MSTest 2.2.7, Microsoft.NET.Test.Sdk 16.11.0)
- `src/nuget.config` - Uses nuget.org as package source
- No `.editorconfig` or style enforcement files present

## CI/CD Validation Pipeline

### PR Validation (.github/workflows/ci.yml)
**Triggers:** On pull requests to `main`
**Runs on:** windows-latest
**Steps:**
1. Checkout with full history (`fetch-depth: 0`)
2. Setup .NET 8.x
3. Setup Nerdbank.GitVersioning (nbgv) 3.6.143
4. `dotnet restore "src/SDK.sln"`
5. `dotnet build "src/SDK.sln" --no-restore -c Release`
6. `dotnet test "src/SDK.sln" -c Release --filter "TestCategory~Unit|TestCategory~Integration|TestCategory~Functional" --no-build --verbosity normal`
7. Build all sample solutions: `ls "samples/**/*.sln" | % { dotnet build $_.FullName -c Release }`

**To replicate PR checks locally, run these commands in order:**
```powershell
dotnet restore "src/SDK.sln"
dotnet build "src/SDK.sln" --no-restore -c Release
dotnet test "src/SDK.sln" -c Release --filter "TestCategory~Unit|TestCategory~Integration|TestCategory~Functional" --no-build --verbosity normal
Get-ChildItem "samples" -Filter *.sln -Recurse | ForEach-Object { dotnet build $_.FullName -c Release }
```

### Main Branch Validation
- **main_build_status.yml:** Build-only validation
- **main_tests_status.yml:** Build + test validation (same as PR but on push to main)

## Architecture and Development Patterns

### Plugin Development Model
SDK plugins consist of:
1. **ProcessingSource** - Entry point, advertises supported data sources
2. **CustomDataProcessor** - Implements data processing logic
3. **Table(s)** - Defines tabular data output
4. **DataCooker(s)** (optional) - Enables data sharing between plugins via data-processing pipeline

**Key Pattern:** Plugins target .NET Standard 2.0/2.1 and reference `Microsoft.Performance.SDK` NuGet package only (NOT the Runtime).

### Target Framework Guidelines
- **SDK plugins:** .NET Standard 2.0 or 2.1
- **Core libraries:** .NET Standard 2.0 (Microsoft.Performance.SDK) or 2.1 (Engine)
- **Runtime:** Multi-targeted: net6.0, net8.0
- **Tests/Tools:** .NET 8.0
- **WPA Compatibility:** Check `documentation/Known-SDK-Driver-Compatibility/WPA.md`

### Common Patterns in Codebase
- Obsolete warnings for IApplicationEnvironmentV2/V3 are expected (removal planned for v2.0)
- ProcessingSource constructor overloads have obsolete variants (use CreateTableProvider pattern)
- Some code has TODO comments for known future work (see grep results)
- Tests use MSTest framework with TestCategory attributes for filtering

## Known Issues and Workarounds

### Expected Build Warnings (NON-BLOCKING)
1. **CS0618 Obsolete Warnings** (~15 warnings):
   - `IApplicationEnvironmentV2` and `IApplicationEnvironmentV3` interfaces marked obsolete
   - `ProcessingSource` constructor overloads marked obsolete
   - **Action:** Ignore these; they're intentional for deprecation path to v2.0

2. **CS1574 XML Comment Warnings** (~6 warnings):
   - cref attributes that couldn't be resolved (e.g., 'PluginOption')
   - **Action:** Ignore; documentation generation still works

3. **CS1685 Predefined Type Warning** (1 warning in Runtime.Tests):
   - ExtensionAttribute defined in multiple assemblies
   - **Action:** Ignore; compiler picks correct definition

### Test Execution Notes
- Some test projects (PluginsSystem tests) may show "No test matches filter" warnings - this is expected as not all tests are categorized
- Test count may vary slightly (712-713) based on skipped tests
- All functional tests should pass; failures indicate real issues

## Code Changes and Testing Workflow

### Making Code Changes
1. **Before editing:** Understand if you're working on:
   - SDK core library (plugins depend on this - breaking changes affect consumers)
   - Runtime (internal to SDK drivers)
   - Plugin system (affects plugin loading/management)
   - Samples (demonstrates patterns)

2. **After making changes:**
   ```powershell
   # Build and immediately test
   dotnet build "src/SDK.sln" -c Release
   dotnet test "src/SDK.sln" -c Release --filter "TestCategory~Unit|TestCategory~Integration|TestCategory~Functional" --no-build --verbosity normal
   ```

3. **For SDK API changes:** Also validate samples build successfully

4. **For new features:** Consider if samples should be updated to demonstrate the feature

### Debugging Plugins
- Plugins are debugged using Windows Performance Analyzer (WPA)
- WPA launch arguments: `-nodefault -addsearchdir <plugin_bin_directory>`
- See `documentation/Using-the-SDK/Creating-your-plugin.md` for Visual Studio debug setup

## Important Guidelines

### Trust These Instructions
These instructions have been validated by running actual builds and tests. If you encounter issues:
1. First, verify you're using the exact commands as specified
2. Check you're in the correct directory context
3. Confirm .NET SDK version compatibility
4. Only then perform additional investigation using grep/search tools

### When to Search vs. Use Instructions
- **Use instructions for:** Building, testing, understanding project structure, CI validation
- **Search/explore when:** Implementing new features, finding specific APIs, understanding complex logic, locating related code
- **Common pitfall:** Over-searching instead of following validated build steps

### File Path Conventions
- Use relative paths from the workspace root in commands (e.g., `src/SDK.sln`)
- Paths with spaces should be quoted
- Use PowerShell path syntax (backslashes or forward slashes both work)

### Version Management
- Version controlled by Nerdbank.GitVersioning (NBGV)
- Version is auto-generated from git history + `version.json`
- Current preview version: 1.4-preview2
- Don't manually edit version numbers in code

## Additional Resources
- **Architecture Overview:** `documentation/Architecture/README.md`
- **Plugin Creation Guide:** `documentation/Using-the-SDK/Creating-your-plugin.md`
- **Simple Plugin Tutorial:** `documentation/Using-the-SDK/Creating-a-simple-sdk-plugin.md`
- **Troubleshooting:** `documentation/Troubleshooting.md`
- **Sample Code:** `samples/SimpleDataSource/` and `samples/SqlPlugin/`
- **Contributing:** `CONTRIBUTING.md` (requires CLA for external contributors)
