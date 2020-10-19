// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.Formatting
{
    /// <summary>
    ///     Declares the default format string of an <see cref="IFormatProvider"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DefaultFormatAttribute
        : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultFormatAttribute"/>
        ///     class with the given default format.
        /// </summary>
        /// <param name="defaultFormat">
        ///     A composite format string representing the default format.
        /// </param>
        public DefaultFormatAttribute(string defaultFormat)
        {
            this.DefaultFormat = defaultFormat;
        }

        /// <see cref="IFormatProviderExtensions.DefaultFormat(IFormatProvider)"/> is the public interface.
        internal string DefaultFormat { get; }
    }
}