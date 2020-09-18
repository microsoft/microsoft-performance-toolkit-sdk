// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Presents an example of how to use the option.
    /// </summary>
    public sealed class OptionExample
        : IEquatable<OptionExample>
    {            
        /// <summary>
        ///     Gets or sets a string that shows how the argument
        ///     should be presented to the user. If argument is
        ///     null, then only the option itself is rendered.
        ///     <example>
        ///         --option=argument    
        ///     </example>
        /// </summary>
        public string Argument { get; set; }

        /// <summary>
        ///     Gets or sets a description of the example, if any.
        /// </summary>
        public string Description { get; set; }

        public bool Equals(OptionExample other)
        {
            return (other != null) &&
                string.Equals(this.Argument, other.Argument) &&
                string.Equals(this.Description, other.Description);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as OptionExample);
        }

        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Argument?.GetHashCode() ?? 0,
                this.Description?.GetHashCode() ?? 0);
        }
    }
}
