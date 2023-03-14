using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public interface IDataCleaner<TTarget>
    {
        IEnumerable<TTarget> CleanData(
            TTarget destination,
            Func<TTarget, bool> predicate = null,
            CancellationToken cancellationToken = default);
    }
}
