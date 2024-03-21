// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents any error that can be thrown by a user
    ///     extension (ProcessingSource, CustomDataProcessor, etc) to
    ///     uniformly report fatal error conditions.
    /// </summary>
    public class ExtensionException
        : Exception
    {
        private static readonly ErrorInfo UnknownError = new ErrorInfo("unknown", "An unknown error occurred.");

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionException"/>
        ///     class.
        /// </summary>
        public ExtensionException()
            : this("An error occurred.")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionException"/>
        ///     class.
        /// </summary>
        public ExtensionException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionException"/>
        ///     class.
        /// </summary>
        public ExtensionException(string message, Exception inner)
            : this(UnknownError, message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionException"/>
        ///     class.
        /// </summary>
        public ExtensionException(ErrorInfo error)
            : this(UnknownError, error?.Message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionException"/>
        ///     class.
        /// </summary>
        public ExtensionException(ErrorInfo error, string message)
            : this(UnknownError, message, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionException"/>
        ///     class.
        /// </summary>
        public ExtensionException(ErrorInfo error, string message, Exception inner)
            : base(message, inner)
        {
            this.Error = error;
        }

        /// <summary>
        ///     Gets the <see cref="ErrorInfo"/> associated with this exception.
        /// </summary>
        public ErrorInfo Error { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(base.ToString());
            sb.AppendLine();
            sb.AppendLine("Error Detail:");
            sb.Append((object)this.Error ?? "<none>");

            return sb.ToString();
        }
    }
}
