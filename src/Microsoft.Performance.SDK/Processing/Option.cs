// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Defines an option that is accepted on the command line for a
    ///     custom data source.
    /// </summary>
    public class Option
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Option"/>
        ///     class.
        /// </summary>
        /// <param name="id">
        ///     An identifier for this option.
        /// </param>
        /// <param name="name">
        ///     The long name of the option.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="name"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="id"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="name"/> is <c>null</c>.
        /// </exception>
        public Option(
            object id,
            string name)
            : this(id, name, 0, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Option"/>
        ///     class.
        /// </summary>
        /// <param name="id">
        ///     An identifier for this option.
        /// </param>
        /// <param name="name">
        ///     The long name of the option.
        /// </param>
        /// <param name="minParam">
        ///     The minimum number of parameters accepted by the option.
        /// </param>
        /// <param name="maxParam">
        ///     The maximum number of parameters accepted by the option.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="name"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="id"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="name"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <paramref name="minParam"/> is less than zero (0.)
        ///     - or -
        ///     <paramref name="maxParam"/> is less than zero (0.)
        ///     - or -
        ///     <paramref name="maxParam"/> is less than <paramref name="minParam"/>.
        /// </exception>
        public Option(
            object id,
            string name,
            int minParam,
            int maxParam)
        {
            Guard.NotNull(id, nameof(id));
            Guard.NotNullOrWhiteSpace(name, nameof(name));
            Guard.GreaterThanOrEqualTo(minParam, 0, nameof(minParam));
            Guard.GreaterThanOrEqualTo(maxParam, 0, nameof(maxParam));
            if (maxParam < minParam)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxParam),
                    $"{nameof(maxParam)} must be greater than or equal to {nameof(minParam)}");
            }

            this.Id = id;
            this.Name = name;
            this.MinimumParameterCount = minParam;
            this.MaximumParameterCount = maxParam;
            this.Id = id;
            this.Description = null;
            this.ArgumentNames = null;
        }

        /// <summary>
        ///     Gets the id for this option.
        /// </summary>
        /// <remarks>
        ///     Any object reference or boxed value type may be supplied. Enumeration values are 
        ///     usually a good choice.
        /// </remarks>
        public object Id{ get; }

        /// <summary>
        ///     Gets the name for this option.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets or sets a description of this option.
        ///     This property may be <c>null</c>.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets a collection of examples on how to use this option.
        ///     This property may be <c>null</c>.
        /// </summary>
        public IEnumerable<OptionExample> Examples { get; set; }

        /// <summary>
        ///     Gets or sets names to use for arguments to this option when
        ///     displayed in a help capacity. This property may be <c>null</c>.
        /// </summary>
        public IEnumerable<string> ArgumentNames { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this option is deprecated.
        /// </summary>
        public bool IsDeprecated { get; set; }

        /// <summary>
        ///     Gets the minimum number of parameters accepted by this option.
        /// </summary>
        public int MinimumParameterCount { get; }

        /// <summary>
        ///     Gets the maximum number of parameters accepted by this option.
        /// </summary>
        public int MaximumParameterCount { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"-{this.Name}";
        }
    }
}
