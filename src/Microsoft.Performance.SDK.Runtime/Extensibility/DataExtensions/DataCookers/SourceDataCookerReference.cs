// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    ///     This class provides a way to create an instance of a particular data extension.
    /// </summary>
    internal sealed class SourceDataCookerReference
        : DataCookerReference<SourceDataCookerReference>,
          ISourceDataCookerReference
    {
        private List<IDataCookerDescriptor> instances = new List<IDataCookerDescriptor>();

        private DataProductionStrategy productionStrategy;

        private bool isDisposed = false;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SourceDataCookerReference"/>
        ///     class as a copy of the given instance.
        /// </summary>
        /// <param name="other">
        ///     The instance from which to initialize this instance.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     <paramref name="other"/> is disposed.
        /// </exception>
        public SourceDataCookerReference(SourceDataCookerReference other)
            : base(other)
        {
            this.InitializeDescriptorData(other);
        }

        private SourceDataCookerReference(Type type, ILogger logger)
            : base(type, logger)
        {
            Guard.NotNull(type, nameof(type));

            // Create an instance just to pull out the descriptor without saving any references to it.

            var instance = this.CreateInstance();

            if (!(instance is ISourceDataCookerDescriptor sourceCookerDescriptor))
            {
                throw new ArgumentException("The type is not a recognized source data cooker.", nameof(type));
            }

            this.productionStrategy = sourceCookerDescriptor.DataProductionStrategy;

            if (sourceCookerDescriptor.DataProductionStrategy == DataProductionStrategy.AsRequired)
            {
                // _CDS_
                // I don't think this should be an issue, because PerfCore event sinks will use
                // EventSinkType.Context rather than going through this, but checking just in case.
                //

                if (StringComparer.Ordinal.Equals(this.Path.SourceParserId, "Microsoft.XPerf"))
                {
                    if (RequiredDataCookers.Any())
                    {
                        // _CDS_ todo: consider loosening this restriction to allow AsRequired source cookers to require other AsRequired source cookers
                        // if we do this, we'll need to decide how to handle this in PerfCore, which currently doesn't allow Context event sinks to have
                        // any dependencies. See "IsCompatibleDependency" in Session.cpp.
                        //

                        // An AsRequired SourceDataCooker must be able to run in every stage, and before source cookers
                        // that require it. XPerf doesn't allow a context data cooker to have any dependencies
                        throw new InvalidOperationException(
                            $"A SourceCooker whose {nameof(DataProductionStrategy)} is " +
                            $"{nameof(DataProductionStrategy.AsRequired)} can only consume SourceCookers whose " +
                            $"{nameof(DataProductionStrategy)} is also {nameof(DataProductionStrategy.AsRequired)}.");
                    }
                }
            }

            this.ValidateInstance(instance);

            this.IsSourceDataCooker = true;

            if (instance != null)
            {
                this.InitializeDescriptorData(instance);
            }
        }

        /// <summary>
        ///     Gets the strategy used by the referenced cooker for
        ///     producing data.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public DataProductionStrategy ProductionStrategy
        {
            get
            {
                this.ThrowIfDisposed();
                return this.productionStrategy;
            }
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="ISourceDataCookerReference"/> based on the
        ///     <paramref name="candidateType"/>.
        ///     <para/>
        ///     A <see cref="Type"/> must satisfy the following criteria in order to 
        ///     be eligible as a reference:
        ///     <list type="bullet">
        ///         <item>must be concrete.</item>
        ///         <item>must implement <see cref="IDataCooker"/> somewhere in the inheritance hierarchy (either directly or indirectly.)</item>
        ///         <item>must implement <see cref="ISourceDataCooker{T, TContext, TKey}"/> somewhere in the inheritance hierarchy (either directly or indirectly.)</item>
        ///         <item>must have a public parameterless constructor.</item>
        ///     </list>
        /// </summary>
        /// <param name="candidateType">
        ///     Candidate <see cref="Type"/> for the <see cref="ISourceDataCookerReference"/>
        /// </param>
        /// <param name="reference">
        ///     Out <see cref="ISourceDataCookerReference"/>
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="candidateType"/> is valid and can create a instance of <see cref="ISourceDataCookerReference"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        internal static bool TryCreateReference(
            Type candidateType,
            out ISourceDataCookerReference reference)
        {
            return TryCreateReference(candidateType, null, out reference);
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="ISourceDataCookerReference"/> based on the
        ///     <paramref name="candidateType"/>.
        ///     <para/>
        ///     A <see cref="Type"/> must satisfy the following criteria in order to 
        ///     be eligible as a reference:
        ///     <list type="bullet">
        ///         <item>must be concrete.</item>
        ///         <item>must implement <see cref="IDataCooker"/> somewhere in the inheritance hierarchy (either directly or indirectly.)</item>
        ///         <item>must implement <see cref="ISourceDataCooker{T, TContext, TKey}"/> somewhere in the inheritance hierarchy (either directly or indirectly.)</item>
        ///         <item>must have a public parameterless constructor.</item>
        ///     </list>
        /// </summary>
        /// <param name="candidateType">
        ///     Candidate <see cref="Type"/> for the <see cref="ISourceDataCookerReference"/>
        /// </param>
        /// <param name="reference">
        ///     Out <see cref="ISourceDataCookerReference"/>
        /// </param>
        /// <param name="logger">
        ///     Logs messages during reference creation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="candidateType"/> is valid and can create a instance of <see cref="ISourceDataCookerReference"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        internal static bool TryCreateReference(
            Type candidateType,
            ILogger logger,
            out ISourceDataCookerReference reference)
        {
            Debug.Assert(candidateType != null, $"{nameof(candidateType)} cannot be null.");

            reference = null;

            if (logger == null)
            {
                logger = Runtime.Logger.Create<SourceDataCookerReference>();
            }

            // perform this basic check first, as it's cheaper than a more specific test below
            if (!candidateType.Implements(typeof(IDataCooker)))
            {
                return false;
            }

            if (!candidateType.IsInstantiatable())
            {
                // this is ok, could just be an abstract base class for a data cooker
                return false;
            }

            if (!candidateType.GetInterfaces().Any(
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISourceDataCooker<,,>)))
            {
                // this is ok, it might be some other type of data cooker
                return false;
            }

            // There must be an empty, public constructor
            if (!candidateType.TryGetEmptyPublicConstructor(out var constructor))
            {
                logger.Error(
                    $"Warning: type {candidateType} seems to be a data cooker, but is missing an empty public " +
                    "constructor.");
                return false;
            }

            try
            {
                reference = new SourceDataCookerReference(candidateType, logger);
            }
            catch (Exception e)
            {
                logger.Warn($"Cooker Disabled: {candidateType}.");
                logger.Warn($"{e.Message}");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override SourceDataCookerReference CloneT()
        {
            this.ThrowIfDisposed();
            return new SourceDataCookerReference(this);
        }

        /// <inheritdoc />
        public IDataCookerDescriptor CreateInstance()
        {
            this.ThrowIfDisposed();

            IDataCookerDescriptor value = null;
            try
            {
                value = Activator.CreateInstance(this.Type) as IDataCookerDescriptor;
                Debug.Assert(value != null);

                lock (this.instances)
                {
                    this.instances.Add(value);
                }

                return value;
            }
            catch
            {
                value.TryDispose();
                throw;
            }
        }

        /// <inheritdoc />
        public override void PerformAdditionalDataExtensionValidation(
            IDataExtensionDependencyStateSupport dependencyStateSupport,
            IDataExtensionReference requiredDataExtension)
        {
            Guard.NotNull(dependencyStateSupport, nameof(dependencyStateSupport));
            Guard.NotNull(requiredDataExtension, nameof(requiredDataExtension));
            Debug.Assert(!string.IsNullOrWhiteSpace(this.Path.SourceParserId));
            this.ThrowIfDisposed();

            // a source data cooker may not rely on any other source
            // a source data cooker may not rely on a non-source data cooker
            // a source data cooker may not rely on data processors

            if (requiredDataExtension is IDataCookerReference dataCookerReference)
            {
                if (!StringComparer.Ordinal.Equals(this.Path.SourceParserId, dataCookerReference.Path.SourceParserId))
                {
                    dependencyStateSupport.AddError(
                        new ErrorInfo(
                            ErrorCodes.EXTENSION_CrossSourceDependency,
                            ErrorCodes.EXTENSION_CrossSourceDependency.Description)
                        {
                            Target = this.Type.FullName,
                        });
                    dependencyStateSupport.UpdateAvailability(DataExtensionAvailability.Error);
                }
                else if (!(requiredDataExtension is SourceDataCookerReference))
                {
                    dependencyStateSupport.AddError(
                        new CrossSourceError(dataCookerReference.Path.ToString(), this.Path.ToString())
                        {
                            Target = dataCookerReference.Path.ToString(),
                        });
                }

                if (ProductionStrategy == DataProductionStrategy.AsRequired)
                {
                    if (((SourceDataCookerReference)requiredDataExtension).ProductionStrategy != DataProductionStrategy.AsRequired)
                    {
                        throw new InvalidOperationException(
                            $"A SourceCooker whose {nameof(DataProductionStrategy)} is " +
                            $"{nameof(DataProductionStrategy.AsRequired)} can only consume SourceCookers whose " +
                            $"{nameof(DataProductionStrategy)} is also {nameof(DataProductionStrategy.AsRequired)}.");
                    }
                }
            }
            // TODO: __SDK_DP__
            // Redesign Data Processor API
            ////else if (requiredDataExtension is IDataProcessorReference dataProcessorReference)
            ////{
            ////    dependencyStateSupport.AddError(
            ////        new ErrorInfo(
            ////            ErrorCodes.EXTENSION_DisallowedDataProcessorDependency,
            ////            ErrorCodes.EXTENSION_DisallowedDataProcessorDependency.Description)
            ////        {
            ////            Target = dataProcessorReference.Id,
            ////        });
            ////    dependencyStateSupport.UpdateAvailability(DataExtensionAvailability.Error);
            ////}
            else
            {
                dependencyStateSupport.AddError(
                    new ErrorInfo(
                        ErrorCodes.EXTENSION_UnknownDependencyType,
                        ErrorCodes.EXTENSION_UnknownDependencyType)
                    {
                        Target = requiredDataExtension.Name,
                    });
                dependencyStateSupport.UpdateAvailability(DataExtensionAvailability.Error);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var v in this.instances)
                {
                    v.TryDispose();
                }

                this.instances = null;
            }

            this.isDisposed = true;
            base.Dispose(disposing);
        }

        [Serializable]
        private sealed class CrossSourceError
            : ErrorInfo
        {
            internal CrossSourceError(string dataReferencePath, string thisPath)
                : base(ErrorCodes.EXTENSION_UnrecognizedDataCookerPath, ErrorCodes.EXTENSION_UnrecognizedDataCookerPath)
            {
                this.DataReferencePath = dataReferencePath;
                this.ThisPath = thisPath;
            }

            private CrossSourceError(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                this.DataReferencePath = info.GetString(nameof(this.DataReferencePath));
                this.ThisPath = info.GetString(nameof(this.ThisPath));
            }

            public string DataReferencePath { get; }

            public string ThisPath { get; }

            [SecurityPermission(
                SecurityAction.LinkDemand,
                Flags = SecurityPermissionFlag.SerializationFormatter)]
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                Guard.NotNull(info, nameof(info));

                info.AddValue(nameof(this.DataReferencePath), this.DataReferencePath);
                info.AddValue(nameof(this.ThisPath), this.ThisPath);
            }
        }
    }
}
