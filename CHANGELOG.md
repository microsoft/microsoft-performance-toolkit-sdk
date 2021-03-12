# Changelog

This document outlines major changes and features between versions. This
document is in order from most recent to least recent.

# Changes in 0.109.0

## Breaking Changes

- Microsoft.Performance.SDK.Processing.CustomDataSourceBase
  - Removed method IsFileSupported
  - Removed method IsFileSupportedCore
  - Added method IsDataSourceSupportedCore

- Microsoft.Performance.SDK.Processing.ICustomDataSource
  - Removed method IsFileSupported
  - Added method IsDataSourceSupported

- Microsoft.Performance.SDK.ReadOnlyHashSet
  - Now implements IReadOnlyCollection

- Microsoft.Performance.Toolkit.Engine.Engine
  - Renamed FreeFilesToProcess to FreeDataSourcesToProcess
  - Renamed FilesToProcess to DataSourcesToProcess
  - Renamed Microsoft.Performance.Toolkit.EngineUnsupportedDataSourceException to UnsupportedCustomDataSourceException
  - Renamed Microsoft.Performance.Toolkit.EngineUnsupportedFileException to UnsupportedDataSourceException

- Microsoft.Performance.SDK.Processing.DataSourceAttribute
  - a new parameter of type `Type` has been added to the constructors

- Microsoft.Performance.SDK.Processing.IDataSource
  - Uri GetUri() converted to a property

## New Features

- Users may opt-in to making their extensions disposable.
    - [Disposable Extensions](documentation/Using-the-SDK/Advanced/Disposable-Extensions.md)

- Tables may now be queried using the Engine.
    - Documentation coming soon!

## Bug Fixes
