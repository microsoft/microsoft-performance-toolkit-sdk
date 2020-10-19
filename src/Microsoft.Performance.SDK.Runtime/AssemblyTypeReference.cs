// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Abstract class meant to be extended to support
    ///     functionality like data extensibility and custom data sources.
    /// </summary>
    /// <typeparam name="TDerived">
    ///     A type that extends this class.
    /// </typeparam>
    public abstract class AssemblyTypeReference<TDerived>
        : IEquatable<TDerived>,
          ICloneable<TDerived>
        where TDerived : AssemblyTypeReference<TDerived>
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="AssemblyTypeReference{TDerived}"/>.
        /// </summary>
        /// <param name="type">
        ///     Reference <see cref="Type"/>
        /// </param>
        protected AssemblyTypeReference(Type type)
        {
            Debug.Assert(type != null);

            this.Type = type;

            if (!this.Type.Assembly.IsDynamic)
            {
                this.AssemblyPath = this.Type.Assembly.Location;
                this.Version = FileVersionInfo.GetVersionInfo(this.AssemblyPath).FileVersion;
            }
            else
            {
                this.AssemblyPath = this.Type.FullName;
                this.Version = this.Type.FullName;
            }
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="AssemblyTypeReference{TDerived}"/> using
        ///     <paramref name="other"/>'s <see cref="AssemblyTypeReference{TDerived}.Type"/>, 
        ///     <see cref="AssemblyTypeReference{TDerived}.AssemblyPath"/>, and
        ///     <see cref="AssemblyTypeReference{TDerived}.Version"/>.
        /// </summary>
        /// <param name="other">
        ///     Instance of <see cref="AssemblyTypeReferenceWithInstance{T, TDerived}"/> to take a reference(s).
        /// </param>
        protected AssemblyTypeReference(AssemblyTypeReference<TDerived> other)
        {
            Debug.Assert(other != null);

            this.Type = other.Type;
            this.AssemblyPath = other.AssemblyPath;
            this.Version = other.Version;
        }

        /// <summary>
        ///     Gets the assembly <see cref="Type"/> referenced.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     Gets the file path location of the assembly the <see cref="AssemblyTypeReference{TDerived}.Type"/> is located.
        /// </summary>
        public string AssemblyPath { get; }

        /// <summary>
        ///     Gets the <see cref="FileVersionInfo.FileVersion"/> of the assembly the <see cref="AssemblyTypeReference{TDerived}.Type"/>.
        /// </summary>
        public string Version { get; }

        /// <summary>
        ///     Checks to see if the <paramref name="candidateType"/> is <see cref="Type.IsPublic"/> and
        ///     <see cref="SDK.TypeExtensions.IsInstantiatable(Type)"/> in the assembly reference.
        /// </summary>
        /// <param name="candidateType">
        ///     <see cref="Type"/> to be checked.
        /// </param>
        /// <param name="requiredImplementation">
        ///     <see cref="Type"/> that is implemented by <paramref name="candidateType"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="candidateType"/> is Public, Instantiatable, and
        ///     <paramref name="candidateType"/> implements <paramref name="requiredImplementation"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        protected static bool IsValidType(Type candidateType, Type requiredImplementation)
        {
            Guard.NotNull(candidateType, nameof(candidateType));
            Guard.NotNull(requiredImplementation, nameof(requiredImplementation));

            if (candidateType.IsPublic() &&
                candidateType.IsInstantiatable())
            {
                if (candidateType.Implements(requiredImplementation))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Determines whether this <see cref="TDerived"/> and a specified <see cref="TDerived"/> object have the
        ///     same <see cref="Type"/>, <see cref="AssemblyPath"/>, and <see cref="Version"/>.
        /// </summary>
        /// <param name="other">
        ///     The <see cref="TDerived"/> to compare to this instance.
        /// </param>
        /// <returns>
        ///     <inheritdoc cref="object.Equals(object)"/>
        /// </returns>
        public virtual bool Equals(TDerived other)
        {
            var toCompare = other as AssemblyTypeReference<TDerived>;

            return !ReferenceEquals(toCompare, null) &&
                   this.Type.Equals(toCompare.Type) &&
                   this.AssemblyPath.Equals(toCompare.AssemblyPath, StringComparison.InvariantCulture) &&
                   this.Version.Equals(toCompare.Version, StringComparison.InvariantCulture);
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as TDerived);
        }

        /// <summary>
        ///     Creates a clone of the derived type, <typeparamref name="TDerived"/>.
        /// </summary>
        /// <returns>
        ///     A clone of the derived object.
        /// </returns>
        public abstract TDerived CloneT();

        /// <inheritdoc cref="ICloneable.Clone()"/>
        object ICloneable.Clone()
        {
            return this.CloneT();
        }

        /// <inheritdoc cref="object.GetHashCode()"/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;

                hash = ((hash << 5) + hash) ^ this.Type.GetHashCode();
                hash = ((hash << 5) + hash) ^ this.AssemblyPath.GetHashCode();
                hash = ((hash << 5) + hash) ^ this.Version.GetHashCode();

                return hash;
            }
        }
    }
}
