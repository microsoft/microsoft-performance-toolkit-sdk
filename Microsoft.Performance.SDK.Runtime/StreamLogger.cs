// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     An implementation of a <see cref="Logger"/> that
    ///     logs to a stream.
    /// </summary>
    public sealed class StreamLogger
        : Logger
    {
        private readonly TextWriter stream;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StreamLogger"/>
        ///     class for the given <see cref="Type"/>, writing messages
        ///     via the given <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> emitting the log messages.
        /// </param>
        /// <param name="stream">
        ///     The stream to which messages are emitted.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="stream"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="type"/> is <c>null</c>.
        /// </exception>
        public StreamLogger(
            Type type,
            TextWriter stream)
            : base(type)
        {
            Guard.NotNull(stream, nameof(stream));

            this.stream = stream;
        }

        /// <summary>
        ///     Writes the given log data to the underlying text writer.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> logging the message.
        /// </param>
        /// <param name="level">
        ///     The logging level.
        /// </param>
        /// <param name="e">
        ///     The <see cref="Exception"/> being logged, if any.
        ///     This parameter may be <c>null</c>.
        /// </param>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to be formatted using <paramref name="fmt"/>.
        /// </param>
        protected override void Write(
            Type type,
            Level level,
            Exception e,
            string fmt,
            params object[] args)
        {
            var message = new StringBuilder();
            message.Append("[").Append(type.FullName).Append("]: ")
                .AppendFormat("{0:G}", level).Append(" - ")
                .AppendFormat(fmt, args);
            if (e != null)
            {
                // todo: inner exceptions
                message.AppendLine()
                    .Append("Exception detail: ").Append(e.GetType()).AppendLine()
                    .Append("    Message: ").Append(e.Message)
                    .Append("    Stack: ").Append(e.StackTrace);
            }

            this.stream.WriteLine(message);
        }
    }
}
