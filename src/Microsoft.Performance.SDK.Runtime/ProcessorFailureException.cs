// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Represents the error that occurs when a processor
    ///     fails. This exception is used by the infrastructure
    ///     to denote when a processor has terminated abnormally
    ///     or unexpectedly.
    /// </summary>
    public class ProcessorFailureException
        : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessorFailureException"/>
        ///     class.
        /// </summary>
        public ProcessorFailureException()
            : this("The data processor encountered an unexpected exception.")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessorFailureException"/>
        ///     class with a message describing the error.
        /// </summary>
        /// <param name="message">
        ///     A message describing the error.
        /// </param>
        public ProcessorFailureException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessorFailureException"/>
        ///     class with a message describing the error, and the error that is the
        ///     cause of this error.
        /// </summary>
        /// <param name="message">
        ///     A message describing the error.
        /// </param>
        /// <param name="inner">
        ///     The <see cref="Exception"/> that is the cause of this error, if any.
        ///     This parameter may be <c>null</c>.
        /// </param>
        public ProcessorFailureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
