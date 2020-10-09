// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides a means of getting the value of an instance
    ///     that is able to be plotted on a graph.
    /// </summary>
    public interface IPlottableGraphType
    {
        /// <summary>
        ///     Gets the value to plot for this instance.
        /// </summary>
        /// <returns>
        ///     A plottable value.
        /// </returns>
        double GetGraphValue();
    }
}
