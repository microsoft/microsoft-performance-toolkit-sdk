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