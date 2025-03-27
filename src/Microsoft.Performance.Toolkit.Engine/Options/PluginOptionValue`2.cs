// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.Toolkit.Engine.Options;

internal record EnginePluginOptionValue<T, TValue>(Guid Guid, TValue Value) : EnginePluginOptionValue(Guid)
    where T : Microsoft.Performance.SDK.Options.Values.PluginOptionValue<TValue>;
