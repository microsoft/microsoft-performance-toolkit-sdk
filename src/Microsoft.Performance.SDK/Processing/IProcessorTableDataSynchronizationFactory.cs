// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Used to create an <see cref="ITableDataSynchronization"/> specific to a given processor.
    /// </summary>
    public interface IProcessorTableDataSynchronizationFactory
    {
        /// <summary>
        ///     Create an <see cref="ITableDataSynchronization"/> specific to <paramref name="processor"/>.
        /// </summary>
        /// <param name="processor">
        ///     The processor with which the <see cref="ITableDataSynchronization"/> will be associated.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="ITableDataSynchronization"/>.
        /// </returns>
        ITableDataSynchronization Create(ICustomDataProcessor processor);
    }
}
