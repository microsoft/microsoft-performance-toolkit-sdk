// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides a means of passing options to a data processor.
    /// </summary>
    public class ProcessorOptions
    {
        /// <summary>
        ///     Gets the instance that represents default <see cref="ProcessorOptions"/>.
        /// </summary>
        public static readonly ProcessorOptions Default = new ProcessorOptions(
            Enumerable.Empty<OptionInstance>(),
            Enumerable.Empty<string>());

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessorOptions"/>
        ///     class.
        /// </summary>
        public ProcessorOptions(
            IEnumerable<OptionInstance> options)
            : this(options, Enumerable.Empty<string>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessorOptions"/>
        ///     class.
        /// </summary>
        public ProcessorOptions(
            IEnumerable<string> arguments)
            : this(Enumerable.Empty<OptionInstance>(), arguments)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessorOptions"/>
        ///     class.
        /// </summary>
        public ProcessorOptions(
            IEnumerable<OptionInstance> options,
            IEnumerable<string> arguments)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(arguments, nameof(arguments));

            this.Options = options.Where(x => x != null).ToList().AsReadOnly();
            this.Arguments = arguments.ToList().AsReadOnly();
        }

        /// <summary>
        ///     Gets the collection of options that should be passed
        ///     to the processor.
        /// </summary>
        public IEnumerable<OptionInstance> Options { get; }

        /// <summary>
        ///     Gets the collection of free arguments that should be
        ///     passed to the processor.
        /// </summary>
        public IEnumerable<string> Arguments { get; }
    }
}
