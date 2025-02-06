// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.Toolkit.Engine.Options;

internal record PluginOptionValue<T, TValue>(Guid Guid, TValue Value) : PluginOptionValue(Guid)
    where T : PluginOption<TValue>;
