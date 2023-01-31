// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Concurrency
{
    /// <summary>
    ///   Contains extension methods for <see cref="ISynchronizedObject"/>.
    /// </summary>
    public static class SynchronizedObjectExtensions
    {
        /// <summary>
        ///     Returns a disposable object while acquiring the lock from a <see cref="ISynchronizedObject"/> that
        ///     is capable of releasing the lock upon disposal.
        /// </summary>
        /// <param name="instance">
        ///     A instnace <see cref="ISynchronizedObject"/>
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns></returns>
        public static IDisposable UseLock(this ISynchronizedObject instance, CancellationToken cancellationToken)
        {
            return UseLockScope.CreateAsync(instance, cancellationToken);
        }

        private struct UseLockScope
            : IDisposable
        {
            private ISynchronizedObject instance;

            private UseLockScope(ISynchronizedObject instance)
            {
                this.instance = instance;
            }

            internal static async Task<UseLockScope> CreateAsync(ISynchronizedObject instance, CancellationToken cancellationToken)
            {
                Guard.NotNull(instance, nameof(instance));

                var scope = new UseLockScope(instance);

                await instance.AcquireLock(cancellationToken);

                return scope;
            }

            public void Dispose()
            {
                if (this.instance != null)
                {
                    this.instance.ReleaseLock();
                    this.instance = null;
                }
            }
        }
    }
}
