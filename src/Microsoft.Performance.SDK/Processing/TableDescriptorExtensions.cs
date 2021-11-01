// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Additional functionality for <see cref="TableDescriptor"/>s.
    /// </summary>
    public static class TableDescriptorExtensions
    {
        /// <summary>
        ///     Determines whether a table requires data extensions (e.g. data cookers).
        /// </summary>
        /// <param name="self">
        ///     Identifies a table with a <see cref="TableDescriptor"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if data extensions are required; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static bool RequiresDataExtensions(this TableDescriptor self)
        {
            Guard.NotNull(self, nameof(self));

            return self.RequiredDataCookers.Any() || self.RequiredDataProcessors.Any();
        }
    }
}
