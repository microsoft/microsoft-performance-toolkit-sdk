// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors
{
    // TODO: __SDK_DP__
    // Redesign Data Processor API
    /////// <summary>
    ///////     A data processor is different than data cookers in that
    ///////     their output is not static but may change based on input.
    ///////     References to DataProcessor types don't create a singleton
    ///////     instance of their target type. Instead, <see cref="GetOrCreateInstance"/>
    ///////     should be called to create an instance based on input data.
    /////// </summary>
    ////internal sealed class DataProcessorReference
    ////    : DataExtensionReference<DataProcessorReference>,
    ////      IDataProcessorReference
    ////{
    ////    private object instanceLock = new object();

    ////    private string id;
    ////    private string description;
    ////    private IDataProcessor instance;
    ////    private bool isInitialized;

    ////    private bool isDisposing = false;
    ////    private bool isDisposed = false;

    ////    /// <summary>
    ////    ///     Initializes a new instance of the <see cref="DataProcessorReference"/>
    ////    ///     class as a copy of the given instance.
    ////    /// </summary>
    ////    /// <param name="other">
    ////    ///     The instance from which to initialize this instance.
    ////    /// </param>
    ////    /// <exception cref="System.ObjectDisposedException">
    ////    ///     <paramref name="other"/> is disposed.
    ////    /// </exception>
    ////    public DataProcessorReference(DataProcessorReference other)
    ////        : base(other)
    ////    {
    ////        this.InitializeDescriptorData(other);

    ////        lock (other.instanceLock)
    ////        {
    ////            this.Instance = other.Instance;
    ////            this.InstanceInitialized = other.InstanceInitialized;
    ////        }
    ////    }

    ////    private DataProcessorReference(Type type)
    ////        : base(type)
    ////    {
    ////        Debug.Assert(type != null, nameof(type));

    ////        // Create an instance just to pull out the descriptor without saving any references to it.
    ////        this.CreateInstance();
    ////        if (this.Instance == null)
    ////        {
    ////            this.AddError($"Unable to create an instance of {this.Type}");
    ////            this.InitialAvailability = DataExtensionAvailability.Error;
    ////        }
    ////        else if (string.IsNullOrWhiteSpace(this.Instance.Id))
    ////        {
    ////            this.AddError("A data processor Id may not be empty.");
    ////            this.InitialAvailability = DataExtensionAvailability.Error;
    ////        }

    ////        if (this.InitialAvailability != DataExtensionAvailability.Error)
    ////        {
    ////            this.InitializeDescriptorData(this.Instance);
    ////        }
    ////    }

    ////    /// <summary>
    ////    ///     Gets or sets the id of this instance.
    ////    /// </summary>
    ////    /// <exception cref="System.ObjectDisposedException">
    ////    ///     This instance is disposed.
    ////    /// </exception>
    ////    public string Id
    ////    {
    ////        get
    ////        {
    ////            this.ThrowIfDisposed();
    ////            return this.id;
    ////        }
    ////        private set
    ////        {
    ////            this.ThrowIfDisposed();
    ////            this.id = value;
    ////        }
    ////    }

    ////    /// <summary>
    ////    ///     Gets or sets a description of this instance.
    ////    /// </summary>
    ////    /// <exception cref="System.ObjectDisposedException">
    ////    ///     This instance is disposed.
    ////    /// </exception>
    ////    public string Description
    ////    {
    ////        get
    ////        {
    ////            this.ThrowIfDisposed();
    ////            return this.description;
    ////        }
    ////        private set
    ////        {
    ////            this.ThrowIfDisposed();
    ////            this.description = value;
    ////        }
    ////    }

    ////    private IDataProcessor Instance
    ////    {
    ////        get
    ////        {
    ////            this.ThrowIfDisposed();
    ////            return this.instance;
    ////        }
    ////        set
    ////        {
    ////            this.ThrowIfDisposed();
    ////            this.instance = value;
    ////        }
    ////    }

    ////    private bool InstanceInitialized
    ////    {
    ////        get
    ////        {
    ////            this.ThrowIfDisposed();
    ////            return this.isInitialized;
    ////        }
    ////        set
    ////        {
    ////            this.ThrowIfDisposed();
    ////            this.isInitialized = value;
    ////        }
    ////    }

    ////    /// <summary>
    ////    ///     Tries to create an instance of <see cref="IDataProcessorReference"/> based on the
    ////    ///     <paramref name="candidateType"/>.
    ////    ///     <para/>
    ////    ///     A <see cref="Type"/> must satisfy the following criteria in order to 
    ////    ///     be eligible as a reference:
    ////    ///     <list type="bullet">
    ////    ///         <item>must be concrete.</item>
    ////    ///         <item>must implement IDataProcessor somewhere in the inheritance hierarchy (either directly or indirectly.)</item>
    ////    ///         <item>must have a public parameterless constructor.</item>
    ////    ///     </list>
    ////    /// </summary>
    ////    /// <param name="candidateType">
    ////    ///     Candidate <see cref="Type"/> for the <see cref="IDataProcessorReference"/>
    ////    /// </param>
    ////    /// <param name="reference">
    ////    ///     Out <see cref="IDataProcessorReference"/>
    ////    /// </param>
    ////    /// <returns>
    ////    ///     <c>true</c> if the <paramref name="candidateType"/> is valid and can create a instance of <see cref="ISourceDataCookerReference"/>;
    ////    ///     <c>false</c> otherwise.
    ////    /// </returns>
    ////    internal static bool TryCreateReference(
    ////        Type candidateType,
    ////        out IDataProcessorReference reference)
    ////    {
    ////        Guard.NotNull(candidateType, nameof(candidateType));

    ////        reference = null;

    ////        if (candidateType.IsInstantiatableOfType(typeof(IDataProcessor)))
    ////        {
    ////            // There must be an empty, public constructor
    ////            if (candidateType.TryGetEmptyPublicConstructor(out var constructor))
    ////            {
    ////                try
    ////                {
    ////                    reference = new DataProcessorReference(candidateType);
    ////                }
    ////                catch (InvalidOperationException e)
    ////                {
    ////                    Console.Error.WriteLine($"Data processor disabled: {candidateType}.");
    ////                    Console.Error.WriteLine($"{e.Message}");
    ////                    return false;
    ////                }
    ////            }
    ////        }

    ////        return reference != null;
    ////    }

    ////    /// <inheritdoc />
    ////    public override DataProcessorReference CloneT()
    ////    {
    ////        this.ThrowIfDisposed();
    ////        return new DataProcessorReference(this);
    ////    }

    ////    /// <inheritdoc />
    ////    public override bool Equals(DataProcessorReference other)
    ////    {
    ////        return base.Equals(other) &&
    ////               StringComparer.Ordinal.Equals(this.Id, other.Id) &&
    ////               StringComparer.Ordinal.Equals(this.Description, other.Description);
    ////    }

    ////    /// <inheritdoc />
    ////    /// <exception cref="System.ObjectDisposedException">
    ////    ///     This instance is disposed.
    ////    /// </exception>
    ////    public IDataProcessor GetOrCreateInstance(IDataExtensionRetrieval requiredData)
    ////    {
    ////        Guard.NotNull(requiredData, nameof(requiredData));
    ////        this.ThrowIfDisposed();

    ////        if (this.Instance == null)
    ////        {
    ////            return null;
    ////        }

    ////        if (!this.InstanceInitialized)
    ////        {
    ////            lock (this.instanceLock)
    ////            {
    ////                if (!this.InstanceInitialized)
    ////                {
    ////                    this.Instance.OnDataAvailable(requiredData);
    ////                    this.InstanceInitialized = true;
    ////                }
    ////            }
    ////        }

    ////        return this.Instance;
    ////    }

    ////    /// <inheritdoc />
    ////    protected override void Dispose(bool disposing)
    ////    {
    ////        if (this.isDisposed)
    ////        {
    ////            return;
    ////        }

    ////        this.isDisposing = true;

    ////        if (disposing)
    ////        {
    ////            this.instance.TryDispose();
    ////            this.instance = null;
    ////            this.description = null;
    ////            this.id = null;
    ////        }

    ////        this.isDisposed = true;
    ////        this.isDisposing = false;
    ////        base.Dispose(disposing);
    ////    }

    ////    private void InitializeDescriptorData(IDataProcessorDescriptor descriptor)
    ////    {
    ////        Guard.NotNull(descriptor, nameof(descriptor));

    ////        this.Id = descriptor.Id;
    ////        this.Description = descriptor.Description;

    ////        if (descriptor is IDataCookerDependent cookerRequirements)
    ////        {
    ////            foreach (var dataCookerPath in cookerRequirements.RequiredDataCookers)
    ////            {
    ////                this.AddRequiredDataCooker(dataCookerPath);
    ////            }
    ////        }

    ////        if (descriptor is IDataProcessorDependent processorRequirements)
    ////        {
    ////            foreach (var dataProcessorId in processorRequirements.RequiredDataProcessors)
    ////            {
    ////                this.AddRequiredDataProcessor(dataProcessorId);
    ////            }
    ////        }
    ////    }

    ////    private void CreateInstance()
    ////    {
    ////        this.Instance = Activator.CreateInstance(this.Type) as IDataProcessor;
    ////        Debug.Assert(this.Instance != null);
    ////    }
    ////}
}
