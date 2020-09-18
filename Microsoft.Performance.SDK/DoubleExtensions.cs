// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="double"/> instances.
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        ///     Determines whether the given instance represents
        ///     a finite quantity.
        /// </summary>
        /// <param name="value">
        ///     The value to check for finiteness.
        /// </param>
        /// <returns>
        ///     <c>true</c> if value represents a finite quantity;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsFinite(this double value)
        {
            return value >= double.MinValue && value <= double.MaxValue;
        }

        /// <summary>
        ///     Helper method to check if a double is valid.
        /// </summary>
        /// <param name="value">
        ///     double to check.</param>
        /// <returns>
        ///     true if the value is invalid, false otherwise.
        /// </returns>
        public static bool IsNonReal(this double value)
        {
            return Double.IsNaN(value) || Double.IsInfinity(value);
        }
    }
}
