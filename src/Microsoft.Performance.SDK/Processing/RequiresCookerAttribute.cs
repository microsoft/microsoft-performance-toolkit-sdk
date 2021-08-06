// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// This attribute is used on a class with static properties that return <see cref="TableDescriptor"/>
    /// or on individual <see cref="TableDescriptor"/> static properties to indicate that a 
    /// table described by the <see cref="TableDescriptor"/> requires a data cooker.
    /// </summary>
    [Obsolete("This will be removed by SDK v1.0.0 release candidate 1. Please use RequiresSourceCooker or RequiresCompositeCooker attributes instead.")]
    public class RequiresCookerAttribute
        : Attribute
    {
        // Placeholder so deriving classes can call a constructor. When this class is made abstract this can be removed.
        protected RequiresCookerAttribute()
        {
        }

        /// <summary>
        /// Constructor that takes a data cooker path.
        /// </summary>
        /// <param name="dataCookerPath">Identifies the required data cooker</param>
        [Obsolete("This will be removed by SDK v1.0.0 release candidate 1. Please use RequiresSourceCooker or RequiresCompositeCooker attributes instead.")]
        public RequiresCookerAttribute(string dataCookerPath)
        {
            Guard.NotNullOrWhiteSpace(dataCookerPath, nameof(dataCookerPath));

            if (!DataCookerPath.IsWellFormed(dataCookerPath))
            {
                throw new ArgumentException($"{nameof(dataCookerPath)} is not a valid identifier.");
            }

            this.RequiredDataCookerPath = new DataCookerPath(
                DataCookerPath.GetSourceParserId(dataCookerPath),
                DataCookerPath.GetDataCookerId(dataCookerPath));
        }

        /// <summary>
        /// Constructor that takes a data cooker path.
        /// </summary>
        /// <param name="dataCookerPath">Identifies the required data cooker</param>
        [Obsolete("This will be removed by SDK v1.0.0 release candidate 1. Please use RequiresSourceCooker or RequiresCompositeCooker attributes instead.")]
        public RequiresCookerAttribute(DataCookerPath dataCookerPath)
        {
            this.RequiredDataCookerPath = dataCookerPath;
        }

        /// <summary>
        /// Constructor that takes a type which implements <see cref="IDataCookerDescriptor"/>.
        /// </summary>
        /// <param name="dataCookerType">Identifies the required data cooker</param>
        [Obsolete("This will be removed by SDK v1.0.0 release candidate 1. Please use RequiresSourceCooker or RequiresCompositeCooker attributes instead.")]
        public RequiresCookerAttribute(Type dataCookerType)
        {
            Guard.NotNull(dataCookerType, nameof(dataCookerType));

            if (!dataCookerType.IsPublicAndInstantiatableOfType(typeof(IDataCookerDescriptor)))
            {
                throw new ArgumentException($"Cannot instantiate type {dataCookerType} as an {nameof(IDataCookerDescriptor)}.");
            }

            // There must be an empty, public constructor
            var constructor = dataCookerType.GetConstructor(Type.EmptyTypes);
            if (constructor == null || !constructor.IsPublic)
            {
                throw new ArgumentException($"Type {dataCookerType} lacks an empty public constructor.");
            }

            IDataCookerDescriptor cookerDescriptor = null;
            try
            {
                cookerDescriptor = Activator.CreateInstance(dataCookerType) as IDataCookerDescriptor;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Unable to instantiate type {dataCookerType} as an {nameof(IDataCookerDescriptor)}.", e);
            }

            if (cookerDescriptor == null)
            {
                throw new ArgumentException($"Unable to instantiate type {dataCookerType} as an {nameof(IDataCookerDescriptor)}.");
            }

            this.RequiredDataCookerPath = new DataCookerPath(cookerDescriptor.Path);
        }

        /// <summary>
        /// Path to a required data cooker for the given table.
        /// </summary>
        public DataCookerPath RequiredDataCookerPath { get; protected set; }
    }
}
