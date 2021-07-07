// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     This attribute is used to mark a concrete class as an <see cref="IProcessingSource"/>
    /// </summary>
    /// <remarks>
    ///     This class will be sealed prior to SDK v1.0.0 release candidate 1. It is
    ///     currently not sealed to maintain backwards compatability with <see cref="CustomDataSourceAttribute"/>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ProcessingSourceAttribute
        : Attribute,
          IEquatable<ProcessingSourceAttribute>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceAttribute"/>
        ///     class.
        /// </summary>
        /// <param name="guid">
        ///     The unique identifier for this <see cref="IProcessingSource"/>. This MAY NOT be
        ///     the default (empty) <see cref="Guid"/>.
        /// </param>
        /// <param name="name">
        ///     The name of this <see cref="IProcessingSource"/>.
        /// </param>
        /// <param name="description">
        ///     A user friendly description of this <see cref="IProcessingSource"/>.
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
        public ProcessingSourceAttribute(
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
        ///     Gets the unique identifier for this <see cref="IProcessingSource"/>.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        ///     Gets the name of this <see cref="IProcessingSource"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the description of this <see cref="IProcessingSource"/>.
        /// </summary>
        public string Description { get; }

        /// <inheritdoc />
        public bool Equals(ProcessingSourceAttribute other)
        {
            return !(ReferenceEquals(other, null)) &&
                this.Guid.Equals(other.Guid) &&
                this.Name.Equals(other.Name) &&
                this.Description.Equals(other.Description);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ProcessingSourceAttribute);
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
