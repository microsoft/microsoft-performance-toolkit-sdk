// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides a means of specifying a conversion to
    ///     a new <see cref="Type"/>.
    /// </summary>
    public interface ICustomTypeConverter
    {
        /// <summary>
        ///     Gets the <see cref="Type"/> into which
        ///     this instance converts other instances of
        ///     <see cref="object"/>s.
        /// </summary>
        /// <returns>
        ///     The conversion <see cref="Type"/>.
        /// </returns>
        Type GetOutputType();
    }
}
