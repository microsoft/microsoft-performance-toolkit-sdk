// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Runtime.Discovery;
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

        public static DependencyDag Create(
            IPlugInCatalog catalog,
            IDataExtensionRepository repository)
        {
            Guard.NotNull(catalog, nameof(catalog));
            Guard.NotNull(repository, nameof(repository));

            if (!catalog.IsLoaded)
            {
                throw new InvalidOperationException();
            }

            if (!catalog.PlugIns.Any() &&
                !repository.GetAllReferences().Any())
            {
                return new DependencyDag(Enumerable.Empty<Node>());
            }

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
                        // nothing to do; these must always
                        // be roots as source parsers, etc. are
                        // created by Custom Data Sources.
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

            if (!allNodes.Any(x => x.Dependents.Count == 0))
            {
                throw new InvalidOperationException("The graph contains a cycle.");
            }

            foreach (var node in allNodes.Where(x => x.Dependents.Count == 0))
            {
                RemoveTransitiveDependencies(node);
            }

            return new DependencyDag(allNodes);
        }

        private static HashSet<Reference> GetAllDependencyReferences(
            IDataExtensionRepository reps,
            IDataExtensionReference der)
        {
            var state = new DataExtensionDependencyState(der);
            state.ProcessDependencies(reps);
            var deps = state.DependencyReferences;

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

        private static void RemoveTransitiveDependencies(Node node)
        {
            Debug.Assert(node != null);

            var stack = new Stack<Node>();
            stack.Push(node);
            while (stack.Count > 0)
            {
                var n = stack.Pop();

                var depsToEnumerate = n.Dependencies.ToList();
                foreach (var dep in depsToEnumerate)
                {
                    Debug.Assert(dep != null);
                    if (n.Dependencies.Except(dep).Any(x => ContainsDependency(x, dep)))
                    {
                        n.RemoveDependency(dep);
                    }

                    stack.Push(dep);
                }
            }
        }

        private static bool ContainsDependency(Node node, Node dep)
        {
            Debug.Assert(node != null);
            Debug.Assert(dep != null);

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
            ///     Gets or sets user-specified data for this instance.
            /// </summary>
            public IDictionary<string, object> ExtensionData { get; }

            /// <summary>
            ///     Adds the given <see cref="Node"/> as a dependency
            ///     of this instance.
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
            ///     of this instance.
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
            ///     Adds the given <see cref="Node"/> as a dependency
            ///     of this instance.
            /// </summary>
            /// <param name="dependency">
            ///     The dependency to add.
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
            ///     Adds the given <see cref="Node"/> as a dependent
            ///     of this instance.
            /// </summary>
            /// <param name="dependent">
            ///     The dependent to add.
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
            ///     A new discriminated union.
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
            ///     A new discriminated union.
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
            public static bool operator !=(Reference x, Reference y)
            {
                return !(x == y);
            }

            /// <summary>
            ///     Force each case to implement <see cref="object.Equals(object)"/>
            /// </summary>
            public abstract override bool Equals(object obj);

            /// <summary>
            ///     Force each case to implement <see cref="object.GetHashCode()"/>
            /// </summary>
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
            public abstract void Match(Action<CustomDataSourceReference> a, Action<IDataExtensionReference> b);

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
            public abstract T Match<T>(Func<CustomDataSourceReference, T> a, Func<IDataExtensionReference, T> b);

            private sealed class Cdsr
                : Reference
            {
                private readonly CustomDataSourceReference value;

                internal Cdsr(CustomDataSourceReference value)
                {
                    Debug.Assert(value != null);
                    this.value = value;
                }

                public override void Match(Action<CustomDataSourceReference> a, Action<IDataExtensionReference> b)
                {
                    Debug.Assert(a != null);
                    a(this.value);
                }

                public override T Match<T>(Func<CustomDataSourceReference, T> a, Func<IDataExtensionReference, T> b)
                {
                    Debug.Assert(a != null);
                    return a(this.value);
                }

                public override bool Equals(object obj)
                {
                    return ReferenceEquals(this, obj) ||
                        (obj is Cdsr t && this.value.Equals(t.value));
                }

                public override int GetHashCode()
                {
                    return this.value.GetHashCode();
                }
            }

            private sealed class Der
                : Reference
            {
                private readonly IDataExtensionReference value;

                internal Der(IDataExtensionReference value)
                {
                    Debug.Assert(value != null);
                    this.value = value;
                }

                public override void Match(Action<CustomDataSourceReference> a, Action<IDataExtensionReference> b)
                {
                    Debug.Assert(b != null);
                    b(this.value);
                }

                public override T Match<T>(Func<CustomDataSourceReference, T> a, Func<IDataExtensionReference, T> b)
                {
                    Debug.Assert(b != null);
                    return b(this.value);
                }

                public override bool Equals(object obj)
                {
                    return ReferenceEquals(this, obj) ||
                        (obj is Der t && this.value.Equals(t.value));
                }

                public override int GetHashCode()
                {
                    return this.value.GetHashCode();
                }
            }
        }
    }
}
