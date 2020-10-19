// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents a piece of detailed information about a table row.
    ///     It contains a top level property with optional children properties.
    /// </summary>
    public struct TableRowDetailEntry
    {
        private readonly IReadOnlyList<TableRowDetailEntry> childrenRO;
        private readonly List<TableRowDetailEntry> children;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableRowDetailEntry">
        /// </summary>
        /// <param name="name">
        ///     The name of the top level property of this entry.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="name"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///       <paramref name="name"/> is WhiteSpace.
        /// </exception>
        public TableRowDetailEntry(string name)
            : this(name, null)
        {
        }

        /// <summary>
        ///      Initializes a new instance of the <see cref="TableRowDetailEntry"/>
        /// </summary>
        /// <param name="name">
        ///      The name of the top level property of this entry.
        /// </param>
        /// <param name="value">
        ///      The value of top level property.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="name"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///       <paramref name="name"/> is WhiteSpace.
        /// </exception>
        public TableRowDetailEntry(string name, string value)
        {
            Guard.NotNullOrWhiteSpace(name, nameof(name));

            this.Name = name;
            this.Value = value;
            this.children = new List<TableRowDetailEntry>();
            this.childrenRO = new ReadOnlyCollection<TableRowDetailEntry>(this.children);
        }

        /// <summary>
        ///     Gets the name for the top level property of this entry.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the value for the top level property of this entry.
        /// </summary>
        public string Value { get; }

        /// <summary>
        ///     Gets if the <see cref="Value" /> is <c>null</c> or empty.
        /// </summary>
        public bool HasValue => !string.IsNullOrEmpty(Value);

        /// <summary>
        ///     Gets the children <see cref="TableRowDetailEntry" /> of this instance.
        /// </summary>
        public IReadOnlyList<TableRowDetailEntry> Childrens => this.childrenRO;

        /// <summary>
        ///     Add a child <see cref="TableRowDetailEntry"/> to this instance.
        /// </summary>
        /// <param name="child">
        ///     A <see cref="TableRowDetailEntry"/> that a piece of detailed information of the top level property. 
        /// </param>
        /// <returns>
        ///     This instance of the <see cref="TableRowDetailEntry"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="child"/> is <c>null</c>.
        /// </exception>
        public TableRowDetailEntry AddChildDetailsInfo(TableRowDetailEntry child)
        {
            Guard.NotNull(child, nameof(child));

            this.children.Add(child);
            return this;
        }
    }
}
