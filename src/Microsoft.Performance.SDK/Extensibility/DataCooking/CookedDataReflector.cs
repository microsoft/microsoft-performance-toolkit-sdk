// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Microsoft.Performance.SDK.Extensibility.DataCooking
{
    /// <summary>
    ///     This class implements <see cref="ICookedDataSet"/> through reflection to identify output
    ///     from the derived class. Data to be exposed through <see cref="ICookedDataSet"/> must be
    ///     exposed as public properties in the class, attributed with <see cref="DataOutputAttribute"/>.
    /// </summary>
    public abstract class CookedDataReflector
        : ICookedDataSet
    {
        private readonly Dictionary<DataOutputPath, PropertyInfo> publicDataProperties 
            = new Dictionary<DataOutputPath, PropertyInfo>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CookedDataReflector"/>
        ///     class with the given path.
        /// </summary>
        /// <param name="dataCookerPath">
        ///     The path to this cooker.
        /// </param>
        protected CookedDataReflector(DataCookerPath dataCookerPath)
        {
            var dataExtensionType = this.GetType();
            var properties = dataExtensionType.GetProperties();
            foreach (var property in properties)
            {
                var dataAttribute = property.GetCustomAttribute<DataOutputAttribute>();
                if (dataAttribute == null)
                {
                    continue;
                }

                string dataIdentifier = dataAttribute.DataIdentifier;

                if (string.IsNullOrWhiteSpace(dataIdentifier))
                {
                    dataIdentifier = property.Name;
                }

                this.publicDataProperties.Add(new DataOutputPath(dataCookerPath, dataIdentifier), property);
            }

            this.OutputIdentifiers = new ReadOnlyCollection<DataOutputPath>(this.publicDataProperties.Keys.ToList());
        }

        /// <inheritdoc />
        public IReadOnlyCollection<DataOutputPath> OutputIdentifiers { get; }

        /// <inheritdoc />
        public T QueryOutput<T>(DataOutputPath identifier)
        {
            var value = this.QueryOutput(identifier);

            return (T)value;
        }

        /// <inheritdoc />
        public object QueryOutput(DataOutputPath identifier)
        {
            if (!this.publicDataProperties.TryGetValue(identifier, out PropertyInfo property))
            {
                throw new InvalidOperationException($"Output identifier is unknown ({identifier}).");
            }

            return property.GetValue(this);
        }
    }
}
