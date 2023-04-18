## Plugin Metadata Json Schema

```
{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "title": "PluginMetadata",
  "type": "object",
  "additionalProperties": false,
  "properties": {
    "Identity": {
      "oneOf": [
        {
          "type": "null"
        },
        {
          "$ref": "#/definitions/PluginIdentity"
        }
      ]
    },
    "Id": {
      "type": [
        "null",
        "string"
      ]
    },
    "Version": {
      "type": [
        "null",
        "string"
      ]
    },
    "DisplayName": {
      "type": [
        "null",
        "string"
      ]
    },
    "Description": {
      "type": [
        "null",
        "string"
      ]
    },
    "Owners": {
      "type": [
        "array",
        "null"
      ],
      "items": {
        "$ref": "#/definitions/PluginOwner"
      }
    },
    "SdkVersion": {
      "type": [
        "null",
        "string"
      ]
    },
    "ProcessingSources": {
      "type": [
        "array",
        "null"
      ],
      "items": {
        "$ref": "#/definitions/ProcessingSourceMetadata"
      }
    },
    "DataCookers": {
      "type": [
        "array",
        "null"
      ],
      "items": {
        "$ref": "#/definitions/DataCookerMetadata"
      }
    },
    "ExtensibleTables": {
      "type": [
        "array",
        "null"
      ],
      "items": {
        "$ref": "#/definitions/TableMetadata"
      }
    }
  },
  "definitions": {
    "PluginIdentity": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Id": {
          "type": [
            "null",
            "string"
          ]
        },
        "Version": {
          "type": [
            "null",
            "string"
          ]
        }
      }
    },
    "PluginOwner": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Name": {
          "type": [
            "null",
            "string"
          ]
        },
        "Address": {
          "type": [
            "null",
            "string"
          ]
        },
        "EmailAddresses": {
          "type": [
            "array",
            "null"
          ],
          "items": {
            "type": "string"
          }
        },
        "PhoneNumbers": {
          "type": [
            "array",
            "null"
          ],
          "items": {
            "type": "string"
          }
        }
      }
    },
    "ProcessingSourceMetadata": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Guid": {
          "type": "string",
          "format": "guid"
        },
        "Name": {
          "type": [
            "null",
            "string"
          ]
        },
        "Version": {
          "type": [
            "null",
            "string"
          ]
        },
        "Description": {
          "type": [
            "null",
            "string"
          ]
        },
        "AboutInfo": {
          "oneOf": [
            {
              "type": "null"
            },
            {
              "$ref": "#/definitions/ProcessingSourceInfo"
            }
          ]
        },
        "AvailableTables": {
          "type": [
            "array",
            "null"
          ],
          "items": {
            "$ref": "#/definitions/TableMetadata"
          }
        },
        "SupportedDataSources": {
          "type": [
            "array",
            "null"
          ],
          "items": {
            "$ref": "#/definitions/DataSourceMetadata"
          }
        }
      }
    },
    "ProcessingSourceInfo": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Owners": {
          "type": [
            "array",
            "null"
          ],
          "items": {
            "$ref": "#/definitions/ContactInfo"
          }
        },
        "ProjectInfo": {
          "oneOf": [
            {
              "type": "null"
            },
            {
              "$ref": "#/definitions/ProjectInfo"
            }
          ]
        },
        "LicenseInfo": {
          "oneOf": [
            {
              "type": "null"
            },
            {
              "$ref": "#/definitions/LicenseInfo"
            }
          ]
        },
        "CopyrightNotice": {
          "type": [
            "null",
            "string"
          ]
        },
        "AdditionalInformation": {
          "type": [
            "array",
            "null"
          ],
          "items": {
            "type": "string"
          }
        }
      }
    },
    "ContactInfo": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Name": {
          "type": [
            "null",
            "string"
          ]
        },
        "Address": {
          "type": [
            "null",
            "string"
          ]
        },
        "EmailAddresses": {
          "type": [
            "array",
            "null"
          ],
          "items": {
            "type": "string"
          }
        },
        "PhoneNumbers": {
          "type": [
            "array",
            "null"
          ],
          "items": {
            "type": "string"
          }
        }
      }
    },
    "ProjectInfo": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Uri": {
          "type": [
            "null",
            "string"
          ]
        }
      }
    },
    "LicenseInfo": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Name": {
          "type": [
            "null",
            "string"
          ]
        },
        "Uri": {
          "type": [
            "null",
            "string"
          ]
        },
        "Text": {
          "type": [
            "null",
            "string"
          ]
        }
      }
    },
    "TableMetadata": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Guid": {
          "type": "string",
          "format": "guid"
        },
        "Name": {
          "type": [
            "null",
            "string"
          ]
        },
        "Description": {
          "type": [
            "null",
            "string"
          ]
        },
        "Category": {
          "type": [
            "null",
            "string"
          ]
        },
        "IsMetadataTable": {
          "type": "boolean"
        }
      }
    },
    "DataSourceMetadata": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Name": {
          "type": [
            "null",
            "string"
          ]
        },
        "Description": {
          "type": [
            "null",
            "string"
          ]
        }
      }
    },
    "DataCookerMetadata": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Name": {
          "type": [
            "null",
            "string"
          ]
        },
        "Description": {
          "type": [
            "null",
            "string"
          ]
        },
        "ProcessingSourceGuid": {
          "type": "string",
          "format": "guid"
        }
      }
    }
  }
}
```