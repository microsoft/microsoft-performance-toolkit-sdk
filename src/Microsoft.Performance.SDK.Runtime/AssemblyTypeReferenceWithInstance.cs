// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Abstract class meant to be extended to support
    ///     functionality like data extensibility and custom data sources, 
    ///     adding an Instance property of type <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     Instance type.
    /// </typeparam>
    /// <typeparam name="Derived">
    ///     The class that derives from this class.
    /// </typeparam>
    public abstract class AssemblyTypeReferenceWithInstance<T, Derived>
        : AssemblyTypeReference<Derived>
          where Derived : AssemblyTypeReferenceWithInstance<T, Derived>
    {
        private T instance;
        private bool isDisposed;

        /// <summary>
        ///     Initializes an instance of <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/> with a new instance of <see cref="T"/>.
        /// </summary>
        /// <param name="type">
        ///     <see cref="Type"/> of <see cref="T"/>
        /// </param>
        protected AssemblyTypeReferenceWithInstance(Type type)
            : base(type)
        {
            this.Instance = (T)Activator.CreateInstance(type);
            Debug.Assert(this.Instance != null);
        }

        /// <summary>
        ///     Initializes an instance of <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/> taking a reference to <see cref="AssemblyTypeReferenceWithInstance{T, Derived}.Instance"/> in <paramref name="other"/>.
        /// </summary>
        /// <param name="other"><inheritdoc/></param>         
        protected AssemblyTypeReferenceWithInstance(
            AssemblyTypeReferenceWithInstance<T, Derived> other)
            : base(other)
        {
            this.Instance = other.Instance;
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/>
        ///     class.
        /// </summary>
        ~AssemblyTypeReferenceWithInstance()
        {
            this.Dispose(false);
        }

        /// <summary>
        ///     Gets an instance of <see cref="T"/>
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public T Instance
        {
            get
            {
                this.ThrowIfDisposed();
                return this.instance;
            }
            private set
            {
                this.ThrowIfDisposed();
                this.instance = value;
            }
        }

        /// <summary>
        ///     <inheritdoc cref="AssemblyTypeReference{Derived}.IsValidType(Type, Type)"/>
        /// </summary>
        /// <param name="candidateType"><see cref="Type"/> to be checked.</param>
        /// <returns><inheritdoc cref="AssemblyTypeReference{Derived}.IsValidType(Type, Type)"/></returns>
        protected static bool IsValidType(Type candidateType)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            return IsValidType(candidateType, typeof(T));
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.instance?.TryDispose();
                this.instance = default(T);
            }

            this.isDisposed = true;
        }
    }
}
