// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="DependencyDag"/> instances.
    /// </summary>
    public static class DependencyDagExtensions
    {
        /// <summary>
        ///     Walks the given DAG, starting with the roots and performing
        ///     the given action on each node as appropriate.
        ///     <para/>
        ///     A node is considered eligible to visit if the following are
        ///     true:
        ///     <list type="bullet">
        ///         <item>All of the node's dependents have been visited.</item>
        ///     </list>
        ///     Once a node is visited, all of the dependencies of said node are
        ///     scheduled for visitation.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="DependencyDag"/> to walk.
        /// </param>
        /// <param name="visitor">
        ///     The action to perform when visiting each node.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="self"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="visitor"/> is <c>null</c>.
        /// </exception>
        public static void DependentWalk(this DependencyDag self, Action<DependencyDag.Node> visitor)
        {
            Guard.NotNull(self, nameof(self));
            Guard.NotNull(visitor, nameof(visitor));

            //
            // The following token is used to mark a node as having been
            // visited. This key will be removed when the method has
            // completed.
            //

            var visitMarker = "VISIT_" + Guid.NewGuid().ToString();
            var visitToken = new object();

            try
            {
                self.All.ForEach(x => x.ExtensionData[visitMarker] = null);

                var visitQueue = new Queue<DependencyDag.Node>();
                foreach (var root in self.Roots)
                {
                    //
                    // Always start at the root, as they have no dependents,
                    // and thus are eligible to be visited immediately.
                    //

                    visitQueue.Enqueue(root);
                }

                while (visitQueue.Count > 0)
                {
                    var n = visitQueue.Dequeue();
                    if (n.Dependents.All(x => x.ExtensionData[visitMarker] == visitToken))
                    {
                        //
                        // All of the dependents of the current node have been
                        // visited, so we can now visit this node. After this node
                        // is visited, we can mark it as having been visited, and 
                        // then schedule all of the dependencies for visitation.
                        //

                        visitor(n);
                        n.ExtensionData[visitMarker] = visitToken;

                        foreach (var d in n.Dependencies)
                        {
                            visitQueue.Enqueue(d);
                        }
                    }
                    else
                    {
                        //
                        // One or more dependents still has not been visited,
                        // so simply reschedule this node to be visited again
                        // later.
                        //

                        visitQueue.Enqueue(n);
                    }
                }
            }
            finally
            {
                //
                // So as not to pollute the extension data on the node
                // unnecessarily, we will always remove the visitation
                // tokens from each node.
                //

                self.All.ForEach(
                    x =>
                    {
                        if (x.ExtensionData.ContainsKey(visitMarker))
                        {
                            x.ExtensionData.Remove(visitMarker);
                        }
                    });
            }
        }
    }
}
