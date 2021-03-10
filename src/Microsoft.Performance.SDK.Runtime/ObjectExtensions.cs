// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for
    ///     interacting with <see cref="object"/> instances.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        ///     Attempts to dispose the given <see cref="object"/>.
        ///     If <paramref name="self"/> is <see cref="IDisposable"/>,
        ///     then the <see cref="SafeDispose(IDisposable)"/> method 
        ///     will be called for <paramref name="self"/>. Otherwise, this
        ///     method does nothing.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="object"/> to dispose.
        /// </param>
        public static void TryDispose(this object self)
        {
            (self as IDisposable)?.SafeDispose();
        }

        /// <summary>
        ///     Disposes the given instance of <see cref="IDisposable"/>
        ///     and enforces the contract that <see cref="IDisposable.Dispose"/>
        ///     should not throw by catch any <see cref="Exception"/>s that
        ///     occur.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="IDisposable"/> to dispose.
        /// </param>
        public static void SafeDispose(this IDisposable self)
        {
            self.SafeDispose(out _);
        }

        /// <summary>
        ///     Disposes the given instance of <see cref="IDisposable"/>
        ///     and enforces the contract that <see cref="IDisposable.Dispose"/>
        ///     should not throw by catch any <see cref="Exception"/>s that
        ///     occur.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="IDisposable"/> to dispose.
        /// </param>
        /// <param name="e">
        ///     If an <see cref="Exception"/> is thrown by the call
        ///     to <see cref="IDisposable.Dispose"/>, then this parameter
        ///     will receive the thrown <see cref="Exception"/>.
        /// </param>
        public static void SafeDispose(this IDisposable self, out Exception e)
        {
            try
            {
                self?.Dispose();
                e = null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("{0}.Dispose threw an exception: {1}", self.GetType(), ex);
                e = ex;
            }
        }
    }
}
