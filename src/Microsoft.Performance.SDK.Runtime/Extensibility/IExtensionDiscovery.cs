// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    /// <summary>
    ///     This interface provides a way to load data extensions into an <see cref="IDataExtensionRepositoryBuilder"/>.
    /// </summary>
    public interface IExtensionDiscovery
    {
        /// <summary>
        ///     Load data extensions into an <see cref="IDataExtensionRepositoryBuilder"/>.
        /// </summary>
        /// <param name="repoBuilder">
        ///     Data extension repository builder.
        /// </param>
        void LoadExtensions(IDataExtensionRepositoryBuilder repoBuilder);
    }
}
