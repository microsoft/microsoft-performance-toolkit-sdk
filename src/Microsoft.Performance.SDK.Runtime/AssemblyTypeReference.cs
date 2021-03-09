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
          ICloneable<TDerived>,
          IDisposable
        where TDerived : AssemblyTypeReference<TDerived>
    {
        private Type type;
        private string assemblyPath;
        private string version;

        private bool isDisposed;

        /// <summary>
        ///     Initializes a new instance of <see cref="AssemblyTypeReference{TDerived}"/>.
        /// </summary>
        /// <param name="type">
        ///     Reference <see cref="Type"/>
        /// </param>
        protected AssemblyTypeReference(Type type)
        {
            Debug.Assert(type != null);

            this.type = type;

            if (!this.Type.Assembly.IsDynamic)
            {
                this.assemblyPath = this.Type.Assembly.Location;
                this.version = FileVersionInfo.GetVersionInfo(this.AssemblyPath).FileVersion;
            }
            else
            {
                this.assemblyPath = this.Type.FullName;
                this.version = this.Type.FullName;
            }

            this.isDisposed = false;
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

            this.type = other.Type;
            this.assemblyPath = other.AssemblyPath;
            this.version = other.Version;
        }

        /// <summary>
        ///     Gets the assembly <see cref="Type"/> referenced.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public Type Type
        {
            get
            {
                this.ThrowIfDisposed();
                return this.type;
            }
        }

        /// <summary>
        ///     Gets the file path location of the assembly the <see cref="AssemblyTypeReference{TDerived}.Type"/> is located.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public string AssemblyPath
        {
            get
            {
                this.ThrowIfDisposed();
                return this.assemblyPath;
            }
        }

        /// <summary>
        ///     Gets the <see cref="FileVersionInfo.FileVersion"/> of the assembly the <see cref="AssemblyTypeReference{TDerived}.Type"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public string Version
        {
            get
            {
                this.ThrowIfDisposed();
                return this.version;
            }
        }

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
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public virtual bool Equals(TDerived other)
        {
            this.ThrowIfDisposed();

            var toCompare = other as AssemblyTypeReference<TDerived>;

            return !ReferenceEquals(toCompare, null) &&
                   this.Type.Equals(toCompare.Type) &&
                   this.AssemblyPath.Equals(toCompare.AssemblyPath, StringComparison.InvariantCulture) &&
                   this.Version.Equals(toCompare.Version, StringComparison.InvariantCulture);
        }

        /// <summary>
        ///     Releases all resources referenced by this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
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
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public abstract TDerived CloneT();

        /// <inheritdoc cref="ICloneable.Clone()"/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        object ICloneable.Clone()
        {
            return this.CloneT();
        }

        /// <inheritdoc cref="object.GetHashCode()"/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public override int GetHashCode()
        {
            this.ThrowIfDisposed();

            unchecked
            {
                var hash = 17;

                hash = ((hash << 5) + hash) ^ this.Type.GetHashCode();
                hash = ((hash << 5) + hash) ^ this.AssemblyPath.GetHashCode();
                hash = ((hash << 5) + hash) ^ this.Version.GetHashCode();

                return hash;
            }
        }

        /// <summary>
        ///     Disposes all resources held by this class.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to dispose both managed and unmanaged
        ///     resources; <c>false</c> to dispose only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.type = null;
                this.assemblyPath = null;
                this.version = null;
            }

            this.isDisposed = true;
        }

        /// <summary>
        ///     Throws an exception if this instance has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        protected void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
    }
}
