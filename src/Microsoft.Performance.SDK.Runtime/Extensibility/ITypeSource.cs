// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    /// <summary>
    ///     Provides access to a set of Types.
    /// </summary>
    internal interface ITypeSource
    {
        /// <summary>
        ///     Identifies the type source.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The types provided by the source.
        /// </summary>
        IEnumerable<Type> Types { get; }
    }
}
