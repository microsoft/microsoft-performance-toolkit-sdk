// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///    Supports cloning, which creates a new instance of a class with the same value
    ///    as an existing instance.
    /// </summary>
    /// <typeparam name="T">
    ///     The <see cref="Type"/> of object being cloned.
    /// </typeparam>
    public interface ICloneable<T>
        : ICloneable
    {
        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        T CloneT();
    }
}
