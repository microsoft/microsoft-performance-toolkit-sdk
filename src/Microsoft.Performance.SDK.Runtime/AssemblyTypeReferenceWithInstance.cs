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
        private Func<T> instanceFactory;
        private T instance;

        private bool isDisposing;
        private bool isDisposed;

        /// <summary>
        ///     Initializes an instance of <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/> with a new instance of <see cref="T"/>.
        /// </summary>
        /// <param name="type">
        ///     <see cref="Type"/> of <see cref="T"/>
        /// </param>
        protected AssemblyTypeReferenceWithInstance(Type type)
            : this(type, () => (T)Activator.CreateInstance(type))
        {
        }

        /// <summary>
        ///     Initializes an instance of <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/> with a new instance of <see cref="T"/>.
        /// </summary>
        /// <param name="type">
        ///     <see cref="Type"/> of <see cref="T"/>
        /// </param>
        /// <param name="instanceFactory">
        ///     The function that creates the instance.
        /// </param>
        protected AssemblyTypeReferenceWithInstance(Type type, Func<T> instanceFactory)
            : base(type)
        {
            this.instanceFactory = instanceFactory;
            this.InitializeThis();
        }

        /// <summary>
        ///     Initializes an instance of <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/> taking a reference to <see cref="AssemblyTypeReferenceWithInstance{T, Derived}.Instance"/> in <paramref name="other"/>.
        /// </summary>
        /// <param name="other"><inheritdoc/></param>         
        protected AssemblyTypeReferenceWithInstance(
            AssemblyTypeReferenceWithInstance<T, Derived> other)
            : base(other)
        {
            this.instanceFactory = other.instanceFactory;
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
        ///     Disposes the <see cref="Instance"/> referenced by this instance 
        ///     without disposing this instance. The <see cref="Instance"/> is
        ///     then reinitialized.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public void Release()
        {
            this.ThrowIfDisposed();
            
            this.OnInstanceDisposing();
            this.instance.TryDispose();
            this.instance = default(T);

            if (this.isDisposing || this.isDisposed)
            {
                return;
            }

            this.InitializeThis();
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

        /// <summary>
        ///     When overridden in a derived class, allows for performing
        ///     any operations against <see cref="Instance"/> before the
        ///     <see cref="Instance"/> is disposed.
        /// </summary>
        protected abstract void OnInstanceDisposing();

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposing = true;
            if (disposing)
            {
                this.Release();
                this.instanceFactory = null;
            }

            this.isDisposed = true;
            this.isDisposing = false;            
            base.Dispose(disposing);
        }

        private void InitializeThis()
        {
            this.instance = this.instanceFactory();
            Debug.Assert(this.instance != null);
        }
    }
}
