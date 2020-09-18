// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Used with <see cref="IProcessorEnvironment.AddNewTable"/> to specify where
    ///     the new table should be added.
    /// </summary>
    public enum AddNewTableOption
    {
        /// <summary>
        ///     The table should be added to the existing analysis view.
        /// </summary>
        CurrentView,

        /// <summary>
        ///     The table should be added to a new analysis view.
        /// </summary>
        NewView,
    }
}
