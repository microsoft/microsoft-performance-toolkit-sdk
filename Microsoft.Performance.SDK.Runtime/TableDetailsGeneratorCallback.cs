// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Represents a function to be called when the UI requests a update on Table Details view.
    /// </summary>
    /// <param name="selectedRows">
    ///     The rows that are selected by the user at the time the Table Details View requests an update.
    /// </param>
    public delegate TableDetails TableDetailsGeneratorCallback(IReadOnlyList<int> selectedRows);
}
