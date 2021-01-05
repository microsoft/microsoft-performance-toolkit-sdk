// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Base attribute for denoting a Data Source that feeds into
    ///     a Custom Data Source. This class cannot be instantiated.
    /// </summary>
    public abstract class DataSourceAttribute
        : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSourceAttribute"/>
        ///     class.
        /// </summary>
        protected DataSourceAttribute()
            : this("No description.")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSourceAttribute"/>
        ///     class.
        /// </summary>
        /// <param name="description">
        ///     A description of the Data Source denoted by this attribute.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="description"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="description"/> is null.
        /// </exception>
        protected DataSourceAttribute(string description)
        {
            Guard.NotNullOrWhiteSpace(description, nameof(description));

            this.Description = description;
        }

        /// <summary>
        ///     Gets the description of the Data Source.
        /// </summary>
        public string Description { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as DataSourceAttribute;

            return
                other != null &&
                this.GetType() == obj.GetType() &&
                this.Description == other.Description;

        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.GetType().GetHashCode(),
                this.Description.GetHashCode());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Description;
        }
    }
}
