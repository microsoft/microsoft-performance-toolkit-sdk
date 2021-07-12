# Abstract

This document outlines major code changes that might be needed when updating from 
Preview Version 0.109.\* to 1.0.0 Relase Candidate 1\*.
# Breaking Changes

There are a number of breaking changes in this version; please see the release notes for a list of these changes.

## Renamed Classes
The following references must be changed:
- `BaseSourceDataCooker` -> `SourceDataCooker`
- `SourceParserBase` -> `SourceParser`
- `BaseDataColumn` -> `DataColumn`
- `CustomDataProcessorBase` -> `CustomDataProcessor`
- `CustomDataProcessorBaseWithSourceParser` -> `CustomDataProcessorWithSourceParser`