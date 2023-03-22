// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Creates a <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/> 
    ///     where T is <see cref="IPluginsSystemResource"/> and Derived is <see cref="PluginsSystemResourceReference"/>.
    /// </summary>
    public sealed class PluginsSystemResourceReference
         : AssemblyTypeReferenceWithInstance<IPluginsSystemResource, PluginsSystemResourceReference>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginsSystemResourceReference"/> class.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> of instance being referenced by this instance.
        /// </param>
        /// <param name="metadata">
        ///     The <see cref="IPluginsSystemResource"/> metadata.
        /// </param>
        public PluginsSystemResourceReference(
            Type type,
            PluginsSystemResourceAttribute metadata)
            : base(type)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNull(metadata, nameof(metadata));

            this.Guid = metadata.Guid;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginsSystemResourceReference"/>
        ///     class with the given instance.
        /// </summary>
        /// <param name="instance">
        ///     An existing instance of <see cref="IPluginsSystemResource"/>
        /// </param>
        public PluginsSystemResourceReference(IPluginsSystemResource instance)
            : base(instance.GetType(), () => instance)
        {
            Guard.NotNull(instance, nameof(instance));

            this.Guid = instance.TryGetGuid();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginsSystemResourceReference"/>
        ///     class as a copy of the given instance.
        /// </summary>
        /// <param name="other">
        ///      The instance from which to make a copy.
        /// </param>
        public PluginsSystemResourceReference(PluginsSystemResourceReference other)
            : base(other.Type)
        {
            Guard.NotNull(other, nameof(other));

            other.ThrowIfDisposed();

            this.Guid = other.Guid;
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="PluginsSystemResourceReference"/> 
        ///     based on the <paramref name="candidateType"/>.
        /// </summary>
        /// <param name="candidateType">
        ///      Candidate <see cref="Type"/> for the <see cref="PluginsSystemResourceReference"/>
        /// </param>
        /// <param name="reference">
        ///      Out <see cref="PluginsSystemResourceReference"/>
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="candidateType"/> is valid and can create a instance of 
        ///     <see cref="PluginsSystemResourceReference"/>; <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreateReference(
            Type candidateType,
            out PluginsSystemResourceReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            reference = null;
            if (IsValidType(candidateType))
            {
                if (TryGetEmptyPublicConstructor(candidateType, out ConstructorInfo constructor))
                {
                    PluginsSystemResourceAttribute metadataAttribute = candidateType.GetCustomAttribute<PluginsSystemResourceAttribute>();
                    if (metadataAttribute != null)
                    {
                        reference = new PluginsSystemResourceReference(
                            candidateType,
                            metadataAttribute);
                    }
                }
            }

            return reference != null;
        }

        /// <summary>
        ///     Gets the Guid of this <see cref="IPluginsSystemResource"/>.
        /// </summary>
        public Guid Guid { get; }

        /// <inheritdoc />
        public override PluginsSystemResourceReference CloneT()
        {
            return new PluginsSystemResourceReference(this);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is PluginsSystemResourceReference other &&
                this.Guid.Equals(other.Guid);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Guid.GetHashCode();
        }

        private static bool TryGetEmptyPublicConstructor(Type type, out ConstructorInfo constructor)
        {
            constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor != null && constructor.IsPublic)
            {
                return true;
            }

            constructor = null;
            return false;
        }
    }
}
