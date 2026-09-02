// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.ColumnCommands;

namespace Microsoft.Performance.SDK.Processing
{
    public interface IDataColumnCommands<T>
    {
        DataColumnCommands<T> Commands { get; }
    }
}
