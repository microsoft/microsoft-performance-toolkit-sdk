## Plugintool CLI
The `plugintool` cli is a WIP that is currently available in the assembly `Microsoft.Performance.Toolkit.Plugins.Publisher.Cli` in branch `user/helzhang/pluginToolCLI`. This tool can be used to generate plugin metadata and package plugin binaries into a zip format that is installable by the new **Plugins System**. The extension of the packaged plugin is `.ptpck`and the metadata file is called `pluginspec.json`.

It also provides a `push` option with a template but the actual pusher will need to be implemented. 

### Usage
#### Metadata File Generation

`plugintool generateMetadata «source» [--target=«target»] [--id=«id»] [--version=«version»] [--displayName=«displayName»] [--description=«description»]` 

- `source` is the directory containing the plugin binaries.
- `target` is the directory where the metadata file will be created.

#### Package a Plugin
`plugintool pack «source» [--target=«target»] [--metadata=«metadata»] [--id=«id»] [--version=«version»] [--displayName=«displayName»] [--description=«description»]`

- `source` is the directory containing the plugin binaries.
- `target` is the directory where the .ptpck file will be created.
- `metadata` is the file to an already generated `pluginspec.json`. Will be auto generated if not provided.

#### Push a Plugin Package to a Plugin Source
This is not usable until we have an `IPluginPackagePusher` implementation.

` plugintool push «feed» «package»`

- `feed` is the uri to push the plugin package to.
- `package` is the path to the `.ptpck` package to push. 
