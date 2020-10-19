// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    // do NOT replace this with a concurrent dictionary
    // do NOT replace with a call to FuncUtils.Memoize.
    // The users of this class depend on the constructor being invoked exactly once.
    // and the above two similar classes do not guarantee this.

    internal sealed class Cache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> cache;
        private readonly Func<TKey, TValue> constructor;

        public Cache(Func<TKey, TValue> constructor)
        {
            this.cache = new Dictionary<TKey, TValue>();
            this.constructor = constructor;
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (cache)
                {
                    TValue value;
                    if (this.cache.TryGetValue(key, out value))
                    {
                        return value;
                    }
                    else
                    {
                        return this.ConstructAndAdd(key);
                    }
                }
            }
        }

        private TValue ConstructAndAdd(TKey key)
        {
            TValue value;
            if (!this.cache.TryGetValue(key, out value))
            {
                value = this.constructor(key);

                this.cache.Add(key, value);
            }

            return value;
        }
    }
}
