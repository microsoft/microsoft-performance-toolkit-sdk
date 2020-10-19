// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.Formatting
{
    /// <summary>
    ///     Targets are collection access provider types. When
    ///     <see cref="ICollectionAccessProvider{TCollection, T}"/> flattens a collection, it delimits elements
    ///     with this character.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DelimiterAttribute
        : Attribute
    {
        private static readonly char defaultDelimiter = '/';
        private readonly char delimiter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelimiterAttribute"/>
        ///     class for the given delimiter.
        /// </summary>
        /// <param name="delimiter">
        ///     The delimiter character.
        /// </param>
        public DelimiterAttribute(char delimiter)
        {
            this.delimiter = delimiter;
        }

        /// <summary>
        ///     Gets the default delimiter character.
        /// </summary>
        public static char DefaultDelimiter
        {
            get
            {
                return DelimiterAttribute.defaultDelimiter;
            }
        }

        /// <summary>
        ///     Gets the delimiter represented by this instance.
        /// </summary>
        public char Delimiter
        {
            get
            {
                return this.delimiter;
            }
        }
    }
}
