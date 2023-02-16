// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Creates a <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/> 
    ///     where T is <see cref="IPluginManagerResource"/> and Derived is <see cref="PluginManagerResourceReference"/>.
    /// </summary>
    public sealed class PluginManagerResourceReference
         : AssemblyTypeReferenceWithInstance<IPluginManagerResource, PluginManagerResourceReference>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginManagerResourceReference"/> class.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> of instance being referenced by this instance.
        /// </param>
        /// <param name="metadata">
        ///     The <see cref="IPluginManagerResource"/> metadata.
        /// </param>
        public PluginManagerResourceReference(
            Type type,
            PluginManagerResourceAttribute metadata)
            : base(type)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNull(metadata, nameof(metadata));

            this.Guid = metadata.Guid;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginManagerResourceReference"/>
        ///     class with the given instance.
        /// </summary>
        /// <param name="instance">
        ///     An existing instance of <see cref="IPluginManagerResource"/>
        /// </param>
        public PluginManagerResourceReference(IPluginManagerResource instance)
            : base(instance.GetType(), () => instance)
        {
            Guard.NotNull(instance, nameof(instance));

            this.Guid = instance.TryGetGuid();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginManagerResourceReference"/>
        ///     class as a copy of the given instance.
        /// </summary>
        /// <param name="other">
        ///      The instance from which to make a copy.
        /// </param>
        public PluginManagerResourceReference(PluginManagerResourceReference other)
            : base(other.Type)
        {
            Guard.NotNull(other, nameof(other));

            other.ThrowIfDisposed();

            this.Guid = other.Guid;
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="PluginManagerResourceReference"/> 
        ///     based on the <paramref name="candidateType"/>.
        /// </summary>
        /// <param name="candidateType">
        ///      Candidate <see cref="Type"/> for the <see cref="PluginManagerResourceReference"/>
        /// </param>
        /// <param name="reference">
        ///      Out <see cref="PluginManagerResourceReference"/>
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="candidateType"/> is valid and can create a instance of 
        ///     <see cref="PluginManagerResourceReference"/>; <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreateReference(
            Type candidateType,
            out PluginManagerResourceReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            reference = null;
            if (IsValidType(candidateType))
            {
                if (candidateType.TryGetEmptyPublicConstructor(out ConstructorInfo constructor))
                {
                    PluginManagerResourceAttribute metadataAttribute = candidateType.GetCustomAttribute<PluginManagerResourceAttribute>();
                    if (metadataAttribute != null)
                    {
                        reference = new PluginManagerResourceReference(
                            candidateType,
                            metadataAttribute);
                    }
                }
            }

            return reference != null;
        }

        /// <summary>
        ///     Gets the Guid of this <see cref="IPluginManagerResource"/>.
        /// </summary>
        public Guid Guid { get; }

        /// <inheritdoc />
        public override PluginManagerResourceReference CloneT()
        {
            return new PluginManagerResourceReference(this);
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

            return obj is PluginManagerResourceReference other &&
                this.Guid.Equals(other.Guid);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Guid.GetHashCode();
        }
    }
}
