// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="IDataColumn"/> instances.
    /// </summary>
    public static class DataColumnExtensions
    {
        /// <summary>
        ///     Sets the format provider for the given column.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="IDataColumn"/> to be modified.
        /// </param>
        /// <param name="formatProvider">
        ///     The new <see cref="IFormatProvider"/> for the configuration.
        ///     This parameter may be <c>null</c>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static void SetFormatProvider(
            this IDataColumn self,
            IFormatProvider formatProvider)
        {
            self.Configuration.SetFormatProvider(formatProvider);
        }
    }
}
