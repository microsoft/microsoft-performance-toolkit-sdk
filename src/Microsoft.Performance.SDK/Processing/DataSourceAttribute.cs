// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Base attribute for denoting a data source that feeds into
    ///     a custom data source. This class cannot be instantiated.
    ///     This class is not meant to be derived by consumers.
    /// </summary>
    public abstract class DataSourceAttribute
        : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSourceAttribute"/>
        ///     class.
        /// </summary>
        protected internal DataSourceAttribute()
        {
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                this.GetType() == obj.GetType();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
