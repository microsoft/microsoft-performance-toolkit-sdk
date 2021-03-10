// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency
{
    /// <summary>
    ///     Represents the complete graph of all Data Extensions
    ///     and their dependencies as loaded in the system.
    ///     <para/>
    ///     Each node has a link to its dependencies and dependents.
    ///     In the context of a given node, a _dependency_ is a node
    ///     that is required for the given node, and a _dependent_ is
    ///     a node that needs the current node. A node is a dependency
    ///     of a dependent.
    ///     <para/>
    ///     Because this DAG represents dependency chains, we are going
    ///     to define the "roots" to be all nodes that have no dependents. 
    /// </summary>
    public sealed class DependencyDag
    {
        private DependencyDag(IEnumerable<Node> allNodes)
        {
            Debug.Assert(allNodes != null);

            this.All = new ReadOnlyHashSet<Node>(allNodes.ToSet());
            this.Roots = new ReadOnlyHashSet<Node>(allNodes.Where(x => x.Dependents.Count == 0).ToSet());
        }

        /// <summary>
        ///     Gets all <see cref="Node"/>s in the graph.
        /// </summary>
        public IReadOnlyCollection<Node> All { get; }

        /// <summary>
        ///     Gets all <see cref="Node"/>s that do not have
        ///     any dependents.
        /// </summary>
        public IReadOnlyCollection<Node> Roots { get; }

        /// <summary>
        ///     Creates a new instance of the <see cref="DependencyDag"/>
        ///     class from all of the loaded plugins / extensions.
        /// </summary>
        /// <param name="catalog">
        ///     The catalog containing all of the loaded plugins.
        /// </param>
        /// <param name="repository">
        ///     The repository containing all of the loaded extensions.
        /// </param>
        /// <returns>
        ///     A new <see cref="DependencyDag"/> that is a graph of all
        ///     of the dependencies between components.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     The <paramref name="catalog"/> is not loaded.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="catalog"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="repository"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     A dependency cycle exists between two or more
        ///     componenets in the graph.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        ///     <paramref name="catalog"/> is disposed.
        ///     - or -
        ///     <paramref name="repository"/> is disposed.
        /// </exception>
        public static DependencyDag Create(
            IPlugInCatalog catalog,
            IDataExtensionRepository repository)
        {
            Guard.NotNull(catalog, nameof(catalog));
            Guard.NotNull(repository, nameof(repository));


            if (!catalog.IsLoaded)
            {
                throw new ArgumentException("The catalog must be loaded.", nameof(catalog));
            }

            if (!catalog.PlugIns.Any() &&
                !repository.GetAllReferences().Any())
            {
                return new DependencyDag(Enumerable.Empty<Node>());
            }

            //
            // We construct the graph by creating a node for each
            // reference. Then, we simply walk each reference's dependency
            // chain and link the nodes to eachother as appropriate.
            //

            var allNodes = catalog.PlugIns
                .Select(x => new Node(x))
                .Concat(repository.GetAllReferences().Select(x => new Node(x)))
                .ToSet();

            var depToNode = allNodes.ToDictionary(
                key => key.Target,
                value => value);

            foreach (var node in allNodes)
            {
                node.Target.Match(
                    (CustomDataSourceReference x) =>
                    {
                        foreach (var p in x.TrackedProcessors.OfType<ICustomDataProcessorWithSourceParser>())
                        {
                            var deps = allNodes.Where(
                                n => n.Target.Match(
                                    r => false,
                                    r => r is ISourceDataCookerReference sdcr &&
                                            sdcr.Path.SourceParserId == p.SourceParserId))
                                .Select(n => n.Target);
                            foreach (var dep in deps)
                            {
                                //
                                // The source cooker depends on the custom data processor,
                                // as the custom data processor is providing the source
                                // parser.
                                //

                                var depNode = depToNode[dep];
                                depNode.AddDependency(node);
                            }
                        }
                    },
                    (IDataExtensionReference x) =>
                    {
                        foreach (var dep in GetAllDependencyReferences(repository, x))
                        {
                            Debug.Assert(depToNode.ContainsKey(dep));
                            var depNode = depToNode[dep];
                            node.AddDependency(depNode);
                        }
                    });
            }

            //
            // Walk the entire DAG, and remove any dependencies that are also
            // transitive dependencies. The walk starts from the roots.
            // For example:
            //
            //     n                              n
            //    / \                             |
            //   d   |  will be simplified to     d
            //    \  |                            |
            //      d2                            d2
            //
            // This is because n needs d and d2, but d also needs d2, so
            // fundamentally n only needs d.
            //

            var roots = allNodes.Where(x => x.Dependents.Count == 0).ToSet();
            foreach (var node in roots)
            {
                SimplifyTransitiveDependencies(node);
            }

            return new DependencyDag(allNodes);
        }

        private static HashSet<Reference> GetAllDependencyReferences(
            IDataExtensionRepository reps,
            IDataExtensionReference der)
        {
            //
            // return ALL dependencies of the given IDataExtensionReference,
            // regardless of type.
            //

            var cycleErrors = der.DependencyState.Errors.Where(x => x.Code == ErrorCodes.EXTENSION_DependencyCycle);
            if (cycleErrors.Any())
            {
                var cycleMessage = cycleErrors.First().ToString();
                throw new InvalidOperationException(cycleMessage);
            }

            var deps = der.DependencyReferences;

            //
            // we are concatenating all of the dependencies found
            // by the dependency state, as well as those explicitly
            // declared on the references themselves.
            //

            return der.DependencyReferences.RequiredSourceDataCookerPaths
                .Select(reps.GetSourceDataCookerReference)
                .Cast<IDataExtensionReference>()
                .Concat(
                    deps.RequiredCompositeDataCookerPaths.Select(reps.GetCompositeDataCookerReference)
                        .Cast<IDataExtensionReference>())
                .Concat(
                    deps.RequiredDataProcessorIds
                        .Select(reps.GetDataProcessorReference)
                        .Cast<IDataExtensionReference>())
                .Concat(
                    der.RequiredDataCookers
                        .Select(
                            x =>
                            {
                                var r = reps.TryGetDataCookerReference(x, out var c);
                                Debug.Assert(r);
                                return c;
                            })
                        .Cast<IDataExtensionReference>())
                .Concat(
                    der.RequiredDataProcessors
                        .Select(reps.GetDataProcessorReference)
                        .Cast<IDataExtensionReference>())
                .Select(Reference.Create)
                .ToSet();
        }

        private static void SimplifyTransitiveDependencies(Node node)
        {
            Debug.Assert(node != null);

            //
            // Starting with the given node, find any dependencies
            // that are a dependency of a dependency, and remove them
            // from this node.
            //
            // This method recursively performs this operation against
            // every node in the graph rooted at the passed in node.
            //

            var stack = new Stack<Node>();
            stack.Push(node);
            while (stack.Count > 0)
            {
                var n = stack.Pop();

                var depsToEnumerate = n.Dependencies.ToList();
                foreach (var dep in depsToEnumerate)
                {
                    Debug.Assert(dep != null);
                    if (n.Dependencies.Without(dep).Any(x => ContainsDependency(x, dep)))
                    {
                        //
                        // dep was found in the dependency chain of one of this
                        // nodes other dependencies, so it does not need to be
                        // listed as an immediate dependency, as it is transitive.
                        // Remove the node as a direct dependency.
                        //

                        n.RemoveDependency(dep);
                    }

                    //
                    // Perform the same pruning operation on the dependency
                    // we just examined.
                    //

                    stack.Push(dep);
                }
            }
        }

        /// <summary>
        ///     Determines whether the given dependency node
        ///     <paramref name="dep"/> is found in any of the dependecy
        ///     chains of the given node <see cref="node"/>.
        ///     <paramref name="dep"/> is considered to be a dependency
        ///     of <paramref name="node"/> if and only if <paramref name="dep"/>
        ///     if found in <paramref name="node"/>'s <see cref="Node.Dependencies"/>
        ///     or is a dependency of one of <paramref name="node"/>'s
        ///     dependencies.
        /// </summary>
        /// <param name="node">
        ///     The <see cref="Node"/> whose dependencies are to be checked.
        /// </param>
        /// <param name="dep">
        ///     The dependency to search for in <paramref name="node"/>'s
        ///     dependencies.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="dep"/> appears anywhere in
        ///     <paramref name="node"/>'s dependency chains; <c>false</c>
        ///     otherwise.
        /// </returns>
        private static bool ContainsDependency(Node node, Node dep)
        {
            Debug.Assert(node != null);
            Debug.Assert(dep != null);

            //
            // recursively check if dep is a dependency of
            // node, or a dependency of one of the dependencies.
            // This method stops when either the dependency is
            // found, or all nodes in the dependency chains have
            // been searched.
            //

            var stack = new Stack<Node>();
            stack.Push(node);
            while (stack.Count > 0)
            {
                var n = stack.Pop();

                if (n.Dependencies.Contains(dep))
                {
                    return true;
                }

                foreach (var d in n.Dependencies)
                {
                    stack.Push(d);
                }
            }

            return false;
        }

        /// <summary>
        ///     Represents a node in the <see cref="DependencyDag"/>.
        /// </summary>
        public sealed class Node
        {
            private readonly HashSet<Node> dependencies;
            private readonly HashSet<Node> dependents;

            /// <summary>
            ///     Initializes a new instance of the <see cref="Node"/>
            ///     class.
            /// </summary>
            /// <param name="cdsr">
            ///     The <see cref="CustomDataSourceReference"/> that is
            ///     the target of this instance.
            /// </param>
            /// <exception cref="System.ArgumentNullException">
            ///     <paramref name="cdsr"/> is <c>null</c>.
            /// </exception>
            public Node(CustomDataSourceReference cdsr)
                : this(Reference.Create(cdsr))
            {
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="Node"/>
            ///     class.
            /// </summary>
            /// <param name="der">
            ///     The <see cref="IDataExtensionReference"/> that is
            ///     the target of this instance.
            /// </param>
            /// <exception cref="System.ArgumentNullException">
            ///     <paramref name="der"/> is <c>null</c>.
            /// </exception>
            public Node(IDataExtensionReference der)
                : this(Reference.Create(der))
            {
            }

            private Node(Reference target)
            {
                Debug.Assert(target != null);

                this.dependencies = new HashSet<Node>(NodesEqualByTargetComparer.Instance);
                this.dependents = new HashSet<Node>(NodesEqualByTargetComparer.Instance);

                this.Target = target;
                this.Dependencies = new ReadOnlyHashSet<Node>(this.dependencies);
                this.Dependents = new ReadOnlyHashSet<Node>(this.dependents);
                this.ExtensionData = new Dictionary<string, object>();
            }

            /// <summary>
            ///     Gets the reference that is held by this <see cref="Node"/>.
            /// </summary>
            public Reference Target { get; }

            /// <summary>
            ///     Gets the <see cref="Node"/>s on which this
            ///     <see cref="Node"/> depends.
            /// </summary>
            public ReadOnlyHashSet<Node> Dependencies { get; }

            /// <summary>
            ///     Gets the <see cref="Node"/>s that depend on this
            ///     <see cref="Node"/>.
            /// </summary>
            public ReadOnlyHashSet<Node> Dependents { get; }

            /// <summary>
            ///     Gets user-specified data for this instance.
            /// </summary>
            public IDictionary<string, object> ExtensionData { get; }

            /// <summary>
            ///     Adds the given <see cref="Node"/> as a dependency
            ///     of this instance. This has the side effect of
            ///     making the current node a dependent of the
            ///     dependency.
            /// </summary>
            /// <param name="dependency">
            ///     The dependency to add.
            /// </param>
            /// <exception cref="System.ArgumentNullException">
            ///     <paramref name="dependency"/> is <c>null</c>.
            /// </exception>
            public void AddDependency(Node dependency)
            {
                Guard.NotNull(dependency, nameof(dependency));

                if (this.dependencies.Add(dependency))
                {
                    dependency.AddDependent(this);
                }
            }

            /// <summary>
            ///     Adds the given <see cref="Node"/> as a dependent
            ///     of this instance. This has the side effect of
            ///     making the current node a dependency of the
            ///     dependent.
            /// </summary>
            /// <param name="dependent">
            ///     The dependent to add.
            /// </param>
            /// <exception cref="System.ArgumentNullException">
            ///     <paramref name="dependent"/> is <c>null</c>.
            /// </exception>
            public void AddDependent(Node dependent)
            {
                Guard.NotNull(dependent, nameof(dependent));

                if (this.dependents.Add(dependent))
                {
                    dependent.AddDependency(this);
                }
            }

            /// <summary>
            ///     Removes the given <see cref="Node"/> as a dependency
            ///     of this instance. This has the side effect of removing
            ///     this node as a dependent of the dependency.
            /// </summary>
            /// <param name="dependency">
            ///     The dependency to remove.
            /// </param>
            /// <exception cref="System.ArgumentNullException">
            ///     <paramref name="dependency"/> is <c>null</c>.
            /// </exception>
            public void RemoveDependency(Node dependency)
            {
                Guard.NotNull(dependency, nameof(dependency));

                if (this.dependencies.Remove(dependency))
                {
                    dependency.RemoveDependent(this);
                }
            }

            /// <summary>
            ///     Removes the given <see cref="Node"/> as a dependent
            ///     of this instance. This has the side effect of removing
            ///     this node as a depency of the dependent.
            /// </summary>
            /// <param name="dependent">
            ///     The dependent to remove.
            /// </param>
            /// <exception cref="System.ArgumentNullException">
            ///     <paramref name="dependent"/> is <c>null</c>.
            /// </exception>
            public void RemoveDependent(Node dependent)
            {
                Guard.NotNull(dependent, nameof(dependent));

                if (this.dependents.Remove(dependent))
                {
                    dependent.RemoveDependency(this);
                }
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return this.Target.ToString();
            }

            /// <summary>
            ///     Two <see cref="Node"/>s are considered to be equal
            ///     if an only if their <see cref="Node.Target"/>s are
            ///     considered to be equal.
            /// </summary>
            private sealed class NodesEqualByTargetComparer
                : IEqualityComparer<Node>
            {
                public static readonly IEqualityComparer<Node> Instance = new NodesEqualByTargetComparer();

                public bool Equals(Node x, Node y)
                {
                    Debug.Assert(x != null);
                    Debug.Assert(y != null);
                    Debug.Assert(x.Target != null);
                    Debug.Assert(y.Target != null);

                    return x.Target.Equals(y.Target);
                }

                public int GetHashCode(Node obj)
                {
                    Debug.Assert(obj != null);
                    Debug.Assert(obj.Target != null);

                    return obj.Target.GetHashCode();
                }
            }
        }

        /// <summary>
        ///     A discriminated union of the reference types
        ///     in the system.
        /// </summary>
        public abstract class Reference
        {
            private Reference()
            {
            }

            /// <summary>
            ///     Creates a new instance of the <see cref="Reference"/>
            ///     class using a <see cref="CustomDataSourceReference"/>.
            /// </summary>
            /// <param name="value">
            ///     The value.
            /// </param>
            /// <returns>
            ///     A new discriminated union whose value represents
            ///     a <see cref="CustomDataSourceReference"/>.
            /// </returns>            
            /// <exception cref="System.ArgumentNullException">
            ///     <paramref name="value"/> is <c>null</c>.
            /// </exception>
            public static Reference Create(CustomDataSourceReference value)
            {
                Guard.NotNull(value, nameof(value));

                return new Cdsr(value);
            }

            /// <summary>
            ///     Creates a new instance of the <see cref="Reference"/>
            ///     class using an <see cref="IDataExtensionReference"/>.
            /// </summary>
            /// <param name="value">
            ///     The value.
            /// </param>
            /// <returns>
            ///     A new discriminated union whose value represents
            ///     an <see cref="IDataExtensionReference"/>.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">
            ///     <paramref name="value"/> is <c>null</c>.
            /// </exception>
            public static Reference Create(IDataExtensionReference value)
            {
                Guard.NotNull(value, nameof(value));

                return new Der(value);
            }

            /// <summary>
            ///     Determines whether the given <see cref="Reference"/>s
            ///     are considered to be equal.
            /// </summary>
            /// <param name="x">
            ///     The first <see cref="Reference"/> to compare.
            /// </param>
            /// <param name="y">
            ///     The second <see cref="Reference"/> to compare.
            /// </param>
            /// <returns>
            ///     <c>true</c> if the given instances are considered to
            ///     be equal; <c>false</c> otherwise.
            /// </returns>
            public static bool operator ==(Reference x, Reference y)
            {
                if (x is null)
                {
                    return y is null;
                }

                return x.Equals(y);
            }

            /// <summary>
            ///     Determines whether the given <see cref="Reference"/>s
            ///     are not considered to be equal.
            /// </summary>
            /// <param name="x">
            ///     The first <see cref="Reference"/> to compare.
            /// </param>
            /// <param name="y">
            ///     The second <see cref="Reference"/> to compare.
            /// </param>
            /// <returns>
            ///     <c>true</c> if the given instances are not considered to
            ///     be equal; <c>false</c> otherwise.
            /// </returns>
            public static bool operator !=(Reference x, Reference y)
            {
                return !(x == y);
            }

            //
            // force each case to implement Equals and GetHashCode.
            //

            /// <inheritdoc />
            public abstract override bool Equals(object obj);

            /// <inheritdoc />
            public abstract override int GetHashCode();

            /// <summary>
            ///     Performs the correct action depending on the target
            ///     of this instance.
            /// </summary>
            /// <typeparam name="T">
            ///     The <see cref="Type"/> of return value.
            /// </typeparam>
            /// <param name="a">
            ///     The function to invoke if this instance targets a
            ///     <see cref="CustomDataSourceReference"/>.
            /// </param>
            /// <param name="b">
            ///     The function to invoke if this instance targets an
            ///     <see cref="IDataExtensionReference"/>.
            /// </param>
            /// <returns>
            ///     The result of the function invocation.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">
            ///     <paramref name="a"/> is <c>null</c>.
            ///     - or -
            ///     <paramref name="b"/> is <c>null</c>.
            /// </exception>
            public abstract void Match(
                Action<CustomDataSourceReference> a,
                Action<IDataExtensionReference> b);

            /// <summary>
            ///     Performs the correct action depending on the target
            ///     of this instance.
            /// </summary>
            /// <typeparam name="T">
            ///     The <see cref="Type"/> of return value.
            /// </typeparam>
            /// <param name="a">
            ///     The function to invoke if this instance targets a
            ///     <see cref="CustomDataSourceReference"/>.
            /// </param>
            /// <param name="b">
            ///     The function to invoke if this instance targets an
            ///     <see cref="IDataExtensionReference"/>.
            /// </param>
            /// <returns>
            ///     The result of the function invocation.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">
            ///     <paramref name="a"/> is <c>null</c>.
            ///     - or -
            ///     <paramref name="b"/> is <c>null</c>.
            /// </exception>
            public abstract T Match<T>(
                Func<CustomDataSourceReference, T> a, 
                Func<IDataExtensionReference, T> b);

            /// <summary>
            ///     Represents the case when the <see cref="Reference"/> is
            ///     to be interpreted as a <see cref="CustomDataSourceReference"/>.
            /// </summary>
            private sealed class Cdsr
                : Reference
            {
                private readonly CustomDataSourceReference value;

                /// <summary>
                ///     Initializes a new instance of the <see cref="Cdsr"/>
                ///     class.
                /// </summary>
                /// <param name="value">
                ///     The <see cref="IDataExtensionReference"/>.
                /// </param>
                internal Cdsr(CustomDataSourceReference value)
                {
                    Debug.Assert(value != null);
                    this.value = value;
                }

                /// <inheritdoc />
                public override void Match(
                    Action<CustomDataSourceReference> a,
                    Action<IDataExtensionReference> b)
                {
                    Debug.Assert(a != null);
                    a(this.value);
                }

                /// <inheritdoc />
                public override T Match<T>(
                    Func<CustomDataSourceReference, T> a, 
                    Func<IDataExtensionReference, T> b)
                {
                    Debug.Assert(a != null);
                    return a(this.value);
                }

                /// <inheritdoc />
                public override bool Equals(object obj)
                {
                    return ReferenceEquals(this, obj) ||
                        (obj is Cdsr t && this.value.Equals(t.value));
                }

                /// <inheritdoc />
                public override int GetHashCode()
                {
                    return this.value.GetHashCode();
                }

                /// <inheritdoc />
                public override string ToString()
                {
                    return this.value.ToString();
                }
            }

            /// <summary>
            ///     Represents the case when the <see cref="Reference"/> is
            ///     to be interpreted as an <see cref="IDataExtensionReference"/>.
            /// </summary>
            private sealed class Der
                : Reference
            {
                private readonly IDataExtensionReference value;

                /// <summary>
                ///     Initializes a new instance of the <see cref="Der"/>
                ///     class.
                /// </summary>
                /// <param name="value">
                ///     The <see cref="IDataExtensionReference"/>.
                /// </param>
                internal Der(IDataExtensionReference value)
                {
                    Debug.Assert(value != null);
                    this.value = value;
                }

                /// <inheritdoc />
                public override void Match(Action<CustomDataSourceReference> a, Action<IDataExtensionReference> b)
                {
                    Debug.Assert(b != null);
                    b(this.value);
                }

                /// <inheritdoc />
                public override T Match<T>(
                    Func<CustomDataSourceReference, T> a,
                    Func<IDataExtensionReference, T> b)
                {
                    Debug.Assert(b != null);
                    return b(this.value);
                }

                /// <inheritdoc />
                public override bool Equals(object obj)
                {
                    return ReferenceEquals(this, obj) ||
                        (obj is Der t && this.value.Equals(t.value));
                }

                /// <inheritdoc />
                public override int GetHashCode()
                {
                    return this.value.GetHashCode();
                }

                /// <inheritdoc />
                public override string ToString()
                {
                    return this.value.ToString();
                }
            }
        }
    }
}
