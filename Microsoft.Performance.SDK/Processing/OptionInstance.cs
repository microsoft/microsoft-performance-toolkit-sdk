// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents an instance of an option on the command line.
    /// </summary>
    public sealed class OptionInstance
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionInstance"/>
        ///     class.
        /// </summary>
        /// <param name="optionId">
        ///     The id of the option.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="optionId"/> is <c>null</c>.
        /// </exception>
        public OptionInstance(object optionId)
            : this(optionId, Enumerable.Empty<string>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionInstance"/>
        ///     class.
        /// </summary>
        /// <param name="optionId">
        ///     The id of the option.
        /// </param>
        /// <param name="argument">
        ///     The argument passed to the option.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="optionId"/> is <c>null</c>.
        /// </exception>
        public OptionInstance(object optionId, string argument)
            : this(optionId, new[] { argument, })
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionInstance"/>
        ///     class.
        /// </summary>
        /// <param name="optionId">
        ///     The id of the option.
        /// </param>
        /// <param name="arguments">
        ///     The arguments passed to the option.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="arguments"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="optionId"/> is <c>null</c>.
        /// </exception>
        public OptionInstance(object optionId, IEnumerable<string> arguments)
        {
            Guard.NotNull(optionId, nameof(optionId));
            Guard.NotNull(arguments, nameof(arguments));

            this.Id = optionId;
            this.Arguments = arguments.ToList().AsReadOnly();
        }

        /// <summary>
        ///     Gets the ID of the option that caused this instance.
        /// </summary>
        public object Id { get; }

        /// <summary>
        ///     Gets the arguments provided to this option.
        /// </summary>
        public IEnumerable<string> Arguments { get; }
    }
}
