## Plugintool CLI
The `plugintool` cli can be used to generate plugin metadata files (`metadata.json` and `metadatacontents.json`) and pack plugin binaries into `.ptix` packages.

### Usage
#### Metadata Files Generation

`plugintool metadata-gen [-s|--source <SOURCE_DIR>] [-o|--output <OUTPUT_DIR>] [-m|--manifest <MANIFEST_FILE>] [-w|--overwrite]` 

- `-s|--source <SOURCE_DIR>` (Required)  
  Specifies the directory containing the plugin binaries.
- `-o|--output <OUTPUT_DIR>`  
  Specifies the directory where the metadata files will be created. If not provided, the files will be created in the current directory.
- `-m|--manifest <MANIFEST_FILE>`  
  Specifies the path to the manifest file. If not provided, the tool will look for a `pluginManifest.json` file in the source directory.
- `-w|--overwrite`  
  Specifies whether to overwrite existing metadata files if they exist. It's only valid if the `-o|--output` option is specified.

#### Package a Plugin
`plugintool pack [-s|--source <SOURCE_DIR>] [-o|--output <OUTPUT_FILE_PATH>] [-m|--manifest <MANIFEST_FILE>] [-w|--overwrite]` `

- `-s|--source <SOURCE_DIR>` (Required)  
  Specifies the directory containing the plugin binaries.
- `-o|--output <OUTPUT_FILE_PATH>`  
  Specifies the path where the `.ptix` package will be created. If not provided, the package will be created in the current directory.
- `-m|--manifest <MANIFEST_FILE>`  
  Specifies the path to the manifest file. If not provided, the tool will look for a `pluginManifest.json` file in the source directory.
- `-w|--overwrite`  
  Specifies whether to overwrite existing metadata files if they exist. It's only valid if the `-o|--output` option is specified.

### Manifest File
The manifest file is an editable JSON file that allows users to specify the metadata for a plugin. To generate metadata files or package a plugin, a manifest file must be provided. The tool will look for a `pluginManifest.json` file in the source directory by default. The manifest file can also be specified using the `-m|--manifest` option. A template of the manifest file can be found [here](https://raw.githubusercontent.com/microsoft/microsoft-performance-toolkit-sdk/main/devel-template/templates/PluginTemplate/pluginManifest.json).

The manifest file contains the following properties:

#### `identity`

`id` - A unique identifier for the plugin. e.g. `com.mycompany.myplugin`.

`version` - The version of the plugin in the format of sematic versioning. e.g. `1.0.0` or `1.0.0-beta.1`.

#### `displayName`
A human-readable name for the plugin. e.g. `My Plugin`.

#### `description`
A human-readable description for the plugin. e.g. `This is a plugin for my application.`

#### `owners`
A list of owners for the plugin.

`name` - The name of the owner. e.g. `John Doe`.

`address` - The address of the owner. e.g. `My Company, 123 Main St, New York, NY 10001`.

`emailAddresses` - A list of email addresses for the owner. e.g. `[ "myemail1@hotmail.com", "myemail2@outlook.com" ]`

`phoneNumbers` - A list of phone numbers for the owner. e.g. `[ "123-456-7890", "098-765-4321" ]`

#### `projectUrl`
The URL of the project repository. e.g. `https://github.com/mycompany/myplugin`.

#### `manifestVersion`
The version of the manifest file defined in the schema this manifest file conforms to. e.g. `1.0`.
