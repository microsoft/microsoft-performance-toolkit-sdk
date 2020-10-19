// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     This attribute is used to mark a concrete class as a custom data
    ///     source.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomDataSourceAttribute
        : Attribute,
          IEquatable<CustomDataSourceAttribute>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomDataSourceAttribute"/>
        ///     class.
        /// </summary>
        /// <param name="guid">
        ///     The unique identifier for this custom data source. This MAY NOT be
        ///     the default (empty) <see cref="Guid"/>.
        /// </param>
        /// <param name="name">
        ///     The name of this custom data source.
        /// </param>
        /// <param name="description">
        ///     A user friendly description of this custom data source.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="guid"/> is whitespace.
        ///     - or -
        ///     <paramref name="guid"/> parsed to a value
        ///     equal to <c>default(Guid)</c>.
        ///     - or -
        ///     <paramref name="name"/> is whitespace.
        ///     - or -
        ///     <paramref name="description"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="guid"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="name"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="description"/> is <c>null</c>.
        /// </exception>
        public CustomDataSourceAttribute(
            string guid,
            string name,
            string description)
        {
            Guard.NotNullOrWhiteSpace(guid, nameof(guid));
            Guard.NotNullOrWhiteSpace(name, nameof(name));
            Guard.NotNullOrWhiteSpace(description, nameof(description));

            this.Guid = Guid.Parse(guid);
            this.Name = name;
            this.Description = description;

            if (this.Guid == default(Guid))
            {
                throw new ArgumentException($"The default GUID `{default(Guid)}` is not allowed.", nameof(guid));
            }
        }

        /// <summary>
        ///     Gets the unique identifier for this custom data source.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        ///     Gets the name of this custom data source.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the description of this custom data source.
        /// </summary>
        public string Description { get; }

        /// <inheritdoc />
        public bool Equals(CustomDataSourceAttribute other)
        {
            return !(ReferenceEquals(other, null)) &&
                this.Guid.Equals(other.Guid) &&
                this.Name.Equals(other.Name) &&
                this.Description.Equals(other.Description);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return this.Equals(obj as CustomDataSourceAttribute);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Guid.GetHashCode(),
                this.Name.GetHashCode(),
                this.Description.GetHashCode());
        }
    }
}
