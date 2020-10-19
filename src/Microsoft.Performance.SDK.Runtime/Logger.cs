// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     A class for logging messages. A logger is meant to be used by
    ///     a specific <see cref="Type"/>, and logs messages for that type.
    ///     If you have multiple types logging messages, then they each should
    ///     have their own instance of a logger. However, they could all still
    ///     be writing to the same backing store, e.g. the console or a file.
    /// </summary>
    public abstract class Logger
        : ILogger
    {
        private static Func<Type, ILogger> factoryField = ConsoleLogger.Create;

        private readonly Type type;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Logger"/>
        ///     class, which logs messages for the given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> for which this logger is logging
        ///     messages.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="type"/> is <c>null</c>.
        /// </exception>
        protected Logger(Type type)
        {
            Guard.NotNull(type, nameof(type));

            this.type = type;
        }

        /// <summary>
        ///     Gets or sets the factory method that is used
        ///     to create loggers. Consumers should use <see cref="Logger.Create{T}"/>
        ///     to create a new logger.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        ///     value is <c>null</c>.
        /// </exception>
        public static Func<Type, ILogger> Factory
        {
            get
            {
                return factoryField;
            }
            set
            {
                Guard.NotNull(value, nameof(value));
                factoryField = value;
            }
        }

        /// <summary>
        ///     Creates a new logger that will log messages
        ///     for the specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> using the logger.
        /// </typeparam>
        /// <returns>
        ///     A new instance of a logger.
        /// </returns>
        public static ILogger Create<T>()
        {
            return Create(typeof(T));
        }

        /// <summary>
        ///     Creates a new logger that will log messages
        ///     for the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> using the logger.
        /// </param>
        /// <returns>
        ///     A new instance of a logger.
        /// </returns>
        public static ILogger Create(Type type)
        {
            Guard.NotNull(type, nameof(type));

            Debug.Assert(Factory != null);
            return Factory(type);
        }

        /// <summary>
        ///     Emits an error message.
        /// </summary>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to be formatted using <paramref name="fmt"/>.
        /// </param>
        public void Error(string fmt, params object[] args)
        {
            this.Error(null, fmt, args);
        }

        /// <summary>
        ///     Emits an error message.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="Exception"/> associated with the error, if any.
        ///     This parameter may be <c>null</c>.
        /// </param>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to be formatted using <paramref name="fmt"/>.
        /// </param>
        public void Error(Exception e, string fmt, params object[] args)
        {
            this.Write(this.type, Level.Error, e, fmt, args);
        }

        /// <summary>
        ///     Emits a fatal error message.
        /// </summary>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to be formatted using <paramref name="fmt"/>.
        /// </param>
        public void Fatal(string fmt, params object[] args)
        {
            this.Fatal(null, fmt, args);
        }

        /// <summary>
        ///     Emits a fatal error message.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="Exception"/> associated with the fatal error, if any.
        ///     This parameter may be <c>null</c>.
        /// </param>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to be formatted using <paramref name="fmt"/>.
        /// </param>
        public void Fatal(Exception e, string fmt, params object[] args)
        {
            this.Write(this.type, Level.Fatal, e, fmt, args);
        }

        /// <summary>
        ///     Emits an informational message.
        /// </summary>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to be formatted using <paramref name="fmt"/>.
        /// </param>
        public void Info(string fmt, params object[] args)
        {
            this.Info(null, fmt, args);
        }

        /// <summary>
        ///     Emits an informational message.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="Exception"/> associated with the message, if any.
        ///     This parameter may be <c>null</c>.
        /// </param>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to be formatted using <paramref name="fmt"/>.
        /// </param>
        public void Info(Exception e, string fmt, params object[] args)
        {
            this.Write(this.type, Level.Information, e, fmt, args);
        }

        /// <summary>
        ///     Emits a verbose message.
        /// </summary>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to be formatted using <paramref name="fmt"/>.
        /// </param>
        public void Verbose(string fmt, params object[] args)
        {
            this.Verbose(null, fmt, args);
        }

        /// <summary>
        ///     Emits a verbose message.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="Exception"/> associated with the message, if any.
        ///     This parameter may be <c>null</c>.
        /// </param>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to be formatted using <paramref name="fmt"/>.
        /// </param>
        public void Verbose(Exception e, string fmt, params object[] args)
        {
            this.Write(this.type, Level.Verbose, e, fmt, args);
        }

        /// <summary>
        ///     Emits a warning message.
        /// </summary>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to be formatted using <paramref name="fmt"/>.
        /// </param>
        public void Warn(string fmt, params object[] args)
        {
            this.Warn(null, fmt, args);
        }

        /// <summary>
        ///     Emits a warning message.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="Exception"/> associated with the warning, if any.
        ///     This parameter may be <c>null</c>.
        /// </param>
        /// <param name="fmt">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to be formatted using <paramref name="fmt"/>.
        /// </param>
        public void Warn(Exception e, string fmt, params object[] args)
        {
            this.Write(this.type, Level.Warning, e, fmt, args);
        }

        /// <summary>
        ///     When overridden in a derived class, emits
        ///     the given log message data.
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
        protected abstract void Write(
            Type type,
            Level level,
            Exception e,
            string fmt,
            params object[] args);

        /// <summary>
        ///     Represents the logging level of the message.
        /// </summary>
        protected enum Level
        {
            /// <summary>
            ///     There is no logging level.
            /// </summary>
            None,

            /// <summary>
            ///     The message is considered to be verbose.
            /// </summary>
            Verbose,

            /// <summary>
            ///     The message is considered to be informational.
            /// </summary>
            Information,

            /// <summary>
            ///     The message is signalling a warning.
            /// </summary>
            Warning,

            /// <summary>
            ///     The message is describing an error condition.
            /// </summary>
            Error,
            
            /// <summary>
            ///     The message is describing a condition that is
            ///     fatal to the stream of execution from which the
            ///     message is being logged.
            /// </summary>
            Fatal,
        }
    }
}
