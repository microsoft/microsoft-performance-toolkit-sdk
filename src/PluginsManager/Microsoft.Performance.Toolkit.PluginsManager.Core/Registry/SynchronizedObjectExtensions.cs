// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Registry
{
    public static class SynchronizedObjectExtensions
    {
        public static IDisposable UseLock(this ISynchronizedObject instance, CancellationToken cancellationToken)
        {
            return UseLockScope.CreateAsync(instance, cancellationToken);
        }

        public struct UseLockScope
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
