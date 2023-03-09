// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Extensibility
{
    /// <summary>
    ///     Creates a <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/> 
    ///     where T is <see cref="IPluginsManagerResource"/> and Derived is <see cref="PluginsManagerResourceReference"/>.
    /// </summary>
    public sealed class PluginsManagerResourceReference
         : AssemblyTypeReferenceWithInstance<IPluginsManagerResource, PluginsManagerResourceReference>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginsManagerResourceReference"/> class.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> of instance being referenced by this instance.
        /// </param>
        /// <param name="metadata">
        ///     The <see cref="IPluginsManagerResource"/> metadata.
        /// </param>
        public PluginsManagerResourceReference(
            Type type,
            PluginsManagerResourceAttribute metadata)
            : base(type)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNull(metadata, nameof(metadata));

            this.Guid = metadata.Guid;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginsManagerResourceReference"/>
        ///     class with the given instance.
        /// </summary>
        /// <param name="instance">
        ///     An existing instance of <see cref="IPluginsManagerResource"/>
        /// </param>
        public PluginsManagerResourceReference(IPluginsManagerResource instance)
            : base(instance.GetType(), () => instance)
        {
            Guard.NotNull(instance, nameof(instance));

            this.Guid = instance.TryGetGuid();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginsManagerResourceReference"/>
        ///     class as a copy of the given instance.
        /// </summary>
        /// <param name="other">
        ///      The instance from which to make a copy.
        /// </param>
        public PluginsManagerResourceReference(PluginsManagerResourceReference other)
            : base(other.Type)
        {
            Guard.NotNull(other, nameof(other));

            other.ThrowIfDisposed();

            this.Guid = other.Guid;
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="PluginsManagerResourceReference"/> 
        ///     based on the <paramref name="candidateType"/>.
        /// </summary>
        /// <param name="candidateType">
        ///      Candidate <see cref="Type"/> for the <see cref="PluginsManagerResourceReference"/>
        /// </param>
        /// <param name="reference">
        ///      Out <see cref="PluginsManagerResourceReference"/>
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="candidateType"/> is valid and can create a instance of 
        ///     <see cref="PluginsManagerResourceReference"/>; <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreateReference(
            Type candidateType,
            out PluginsManagerResourceReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            reference = null;
            if (IsValidType(candidateType))
            {
                if (TryGetEmptyPublicConstructor(candidateType, out ConstructorInfo constructor))
                {
                    PluginsManagerResourceAttribute metadataAttribute = candidateType.GetCustomAttribute<PluginsManagerResourceAttribute>();
                    if (metadataAttribute != null)
                    {
                        reference = new PluginsManagerResourceReference(
                            candidateType,
                            metadataAttribute);
                    }
                }
            }

            return reference != null;
        }

        /// <summary>
        ///     Gets the Guid of this <see cref="IPluginsManagerResource"/>.
        /// </summary>
        public Guid Guid { get; }

        /// <inheritdoc />
        public override PluginsManagerResourceReference CloneT()
        {
            return new PluginsManagerResourceReference(this);
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

            return obj is PluginsManagerResourceReference other &&
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
