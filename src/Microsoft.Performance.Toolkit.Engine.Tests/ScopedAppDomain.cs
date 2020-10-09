// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    public sealed class ScopedAppDomain
        : IDisposable
    {
        private AppDomain instance;

        private bool isDisposed;

        private ScopedAppDomain()
        {
            this.instance = null;
            this.isDisposed = false;
        }

        public AppDomain Instance
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException(nameof(ScopedAppDomain));
                }

                return this.instance;
            }
        }

        public static implicit operator AppDomain(ScopedAppDomain scoped)
        {
            return scoped.instance;
        }

        public static ScopedAppDomain Create(string name)
        {
            return Create(name, AppDomain.CurrentDomain);
        }

        public static ScopedAppDomain Create(string name, AppDomain template)
        {
            var d = new ScopedAppDomain();
            d.instance = AppDomain.CreateDomain(
                name, 
                template.Evidence,
                template.SetupInformation);

            return d;
        }

        ~ScopedAppDomain()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                AppDomain.Unload(this.instance);
                this.instance = null;
            }

            this.isDisposed = true;
        }
    }
}
