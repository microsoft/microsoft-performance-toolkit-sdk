// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Base attribute for denoting a Data Source that feeds into
    ///     a <see cref="IProcessingSource"/>. This class cannot be instantiated.
    ///     <para/>
    ///     Because users can implement their own
    ///     <see cref="IDataSource"/>s, they can implement an
    ///     Attribute that implements this class with their
    ///     Data Source as the type parameter.
    ///     <para />
    ///     <example>
    ///     This example shows a sample implementation:
    ///     <code>
    ///         public sealed class SqlServerDataSourceAttribute
    ///             : DataSourceAttribute
    ///         {
    ///             public SqlServerDataSourceAttribute()
    ///                 : base(typeof(SqlServerDataSource))
    ///             {
    ///             }
    ///         }
    ///         
    ///         [ProcessingSource(/* properties elided */)]
    ///         [SqlServerDataSource(/* properties elided */)]
    ///         public sealed class SqlProcessingSource
    ///             : ProcessingSource
    ///         {
    ///             // implementation elided
    ///         }
    ///     </code>
    ///     Any instances of the SqlServerDataSource Data Source will be
    ///     routed to this <see cref="IProcessingSource"/> for further analysis during
    ///     processing.
    ///     </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class DataSourceAttribute
        : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSourceAttribute"/>
        ///     class.
        /// </summary>
        /// <param name="type">
        ///     The type of the <see cref="IDataSource"/> corresponding to this attribute.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="type"/> is <c>null</c>.
        /// </exception>
        protected DataSourceAttribute(Type type)
            : this(type, "No description.")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSourceAttribute"/>
        ///     class.
        /// </summary>
        /// <param name="type">
        ///     The type of the <see cref="IDataSource"/> corresponding to this attribute.
        /// </param>
        /// <param name="description">
        ///     A description of the Data Source denoted by this attribute.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="description"/> is whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="type"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="description"/> is null.
        /// </exception>
        protected DataSourceAttribute(Type type, string description)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNullOrWhiteSpace(description, nameof(description));

            if (!type.Is<IDataSource>())
            {
                throw new ArgumentException($"'{type}' must implement IDataSource.", nameof(type));
            }

            this.Type = type;
            this.Description = description;
        }

        /// <summary>
        ///     Gets the <see cref="System.Type"/> of Data Source denoted by
        ///     this <see cref="DataSourceAttribute"/>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     Gets the description of the Data Source.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     This method is used to filter any incoming <see cref="IDataSource"/>s so that only
        ///     <see cref="IDataSource"/>s that could conceivably be processed by the decorated
        ///     <see cref="IProcessingSource"/> are passed to the <see cref="IProcessingSource"/> for evaluation.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if it is feasible for this Data Source to be supported;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="dataSource"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     <paramref name="dataSource"/> is not assignable to the <see cref="System.Type"/>
        ///     specified by this instance. See <see cref="DataSourceAttribute.Type"/>.
        /// </exception>
        public bool Accepts(IDataSource dataSource)
        {
            Guard.NotNull(dataSource, nameof(dataSource));

            if (!dataSource.GetType().Is(this.Type))
            {
                Debug.Fail("The runtime should never pass an incompatible type to this method.");
                throw new InvalidOperationException(
                    $"The runtime should never pass an incompatible type to this method. Expected '{this.Type}', got '{dataSource.GetType()}'");
            }

            return this.AcceptsCore(dataSource);
        }

        /// <summary>
        ///     When overridden in a derived class, this method is used to filter any
        ///     incoming Data Sources so that only Data Sources that could conceivably
        ///     be processed by the decorated <see cref="IProcessingSource"/> are passed to the
        ///     <see cref="IProcessingSource"/> for evaluation.
        ///     <para />
        ///     This method returns <c>true</c> if it is not overridden. This method should
        ///     be overridden if your Data Source is potentially useful to lots of different
        ///     <see cref="IProcessingSource"/>s (e.g. a file) but you know for a fact that most Custom
        ///     Data Sources would not be able to do anything with them, and so you want to
        ///     filter a subset of said Data Sources to your <see cref="IProcessingSource"/>.
        ///     <para/>
        ///     For example, The <see cref="FileDataSourceAttribute" /> uses this method to
        ///     reject any <see cref="FileDataSource"/>s that do not have the prescribed extension.
        ///     This way, only files with the relevant extension get passed to the <see cref="IProcessingSource"/>
        ///     for further inspection. See the <see cref="FileDataSourceAttribute"/> class for
        ///     the implementation.
        ///     <para />
        ///     It is guaranteed that the runtime will only pass instances of <paramref name="dataSource"/> that
        ///     are of the <see cref="Type"/> specified by <see cref="DataSourceAttribute.Type"/>.
        ///     It is guaranteed that the runtime will not pass <c>null</c> to this method.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if it is feasible for this Data Source to be supported;
        ///     <c>false</c> otherwise.
        /// </returns>
        protected virtual bool AcceptsCore(IDataSource dataSource)
        {
            return true;
        }

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
                this.Type == other.Type &&
                this.Description == other.Description;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.GetType().GetHashCode(),
                this.Type.GetHashCode(),
                this.Description.GetHashCode());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Type} - {this.Description}";
        }
    }
}
