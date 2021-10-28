// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    ///     Represents a reference to a composite data cooker.
    /// </summary>
    internal sealed class CompositeDataCookerReference
        : DataCookerReference<CompositeDataCookerReference>,
          ICompositeDataCookerReference
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositeDataCookerReference"/>
        ///     class as a copy of the given instance.
        /// </summary>
        /// <param name="other">
        ///     The instance from which to create this instance.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     <paramref name="other"/> is disposed.
        /// </exception>
        public CompositeDataCookerReference(CompositeDataCookerReference other)
            : base(other)
        {
            this.InitializeDescriptorData(other);
        }

        private CompositeDataCookerReference(Type type)
            : base(type)
        {
            ICompositeDataCookerDescriptor instance = this.CreateInstance();
            this.ValidateInstance(instance);

            if (instance != null)
            {
                this.InitializeDescriptorData(instance);
            }
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="ICompositeDataCookerReference"/> based on the
        ///     <paramref name="candidateType"/>.
        ///     <para/>
        ///     A <see cref="Type"/> must satisfy the following criteria in order to 
        ///     be eligible as a reference:
        ///     <list type="bullet">
        ///         <item>must be concrete.</item>
        ///         <item>must implement ICompositeDataCookerDescriptor somewhere in the inheritance hierarchy (either directly or indirectly.)</item>
        ///         <item>must have a public parameterless constructor.</item>
        ///     </list>
        /// </summary>
        /// <param name="candidateType">
        ///     Candidate <see cref="Type"/> for the <see cref="ICompositeDataCookerReference"/>
        /// </param>
        /// <param name="reference">
        ///     Out <see cref="ICompositeDataCookerReference"/>
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="candidateType"/> is valid and can create a instance of <see cref="ISourceDataCookerReference"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        internal static bool TryCreateReference(
            Type candidateType,
            out ICompositeDataCookerReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            reference = null;

            if (candidateType.IsInstantiatableOfType(typeof(ICompositeDataCookerDescriptor)))
            {
                // There must be an empty, public constructor
                if (candidateType.TryGetEmptyPublicConstructor(out var constructor))
                {
                    try
                    {
                        reference = new CompositeDataCookerReference(candidateType);
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.Error.WriteLine($"Cooker Disabled: {candidateType}.");
                        Console.Error.WriteLine($"{e.Message}");
                        return false;
                    }
                }
            }

            return reference != null;
        }

        /// <inheritdoc />
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataCooker CreateInstance(IDataExtensionRetrieval requiredData)
        {
            Guard.NotNull(requiredData, nameof(requiredData));
            this.ThrowIfDisposed();

            ICompositeDataCookerDescriptor compositeCooker = CreateInstance();
            compositeCooker.OnDataAvailable(requiredData);

            return compositeCooker;
        }

        /// <inheritdoc />
        public override CompositeDataCookerReference CloneT()
        {
            this.ThrowIfDisposed();
            return new CompositeDataCookerReference(this);
        }

        /// <inheritdoc />
        protected override void ValidateInstance(IDataCookerDescriptor instance)
        {
            this.ThrowIfDisposed();
            base.ValidateInstance(instance);

            if (instance.Path.DataCookerType != DataCookerType.CompositeDataCooker)
            {
                this.AddError($"Unable to create an instance of {this.Type}");
                this.InitialAvailability = DataExtensionAvailability.Error;
            }
        }

        private ICompositeDataCookerDescriptor CreateInstance()
        {
            return Activator.CreateInstance(this.Type) as ICompositeDataCookerDescriptor;
        }
    }
}
