// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Encapsulates information around an error condition.
    ///     In order to expose more specific error information,
    ///     you may derive from this class and add your own
    ///     properties.
    ///     <para/>
    ///     Plugin authors may use this in conjunction with
    ///     <see cref="ExtensionException"/> to report errors
    ///     from their Plugins.
    /// </summary>
    [Serializable] // Remove this for 2.0
    public class ErrorInfo
        : IFormattable,
          ISerializable // Remove this for 2.0
    {
        /// <summary>
        ///     The <see cref="ErrorInfo"/> instance representing no errors.
        /// </summary>
        public static readonly ErrorInfo None = new ErrorInfo("NO_ERROR", "No error has occurred.");

        /// <summary>
        ///     Initializes a new instance of the <see cref="ErrorInfo"/>
        ///     class.
        /// </summary>
        /// <param name="code">
        ///     A processor specific error code.
        /// </param>
        /// <param name="message">
        ///     A human-readable representation of the error.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="code"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="message"/> is <c>null</c>.
        /// </exception>
        public ErrorInfo(string code, string message)
        {
            Guard.NotNull(code, nameof(code));
            Guard.NotNull(message, nameof(message));

            this.Code = code;
            this.Message = message;
        }

        /// <summary>
        ///     Serialization constructor.
        /// </summary>
        /// <param name="info">
        ///     The data to be deserialized.
        /// </param>
        /// <param name="context">
        ///     The context of the serialization.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="info"/> is <c>null</c>.
        /// </exception>
        [Obsolete("SYSLIB0051")] // Remove this method for 2.0
        protected ErrorInfo(SerializationInfo info, StreamingContext context)
        {
            Guard.NotNull(info, nameof(info));

            this.Code = info.GetString(nameof(Code));
            this.Message = info.GetString(nameof(Message));
            this.Target = info.GetString(nameof(Target));
            this.Details = (ErrorInfo[])info.GetValue(nameof(Details), typeof(ErrorInfo[]));
        }

        /// <summary>
        ///     Gets the processor specific code for this error.
        /// </summary>
        public string Code { get; }

        /// <summary>
        ///     Gets a human-readable representation of the error.
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///     Gets or sets the target of the error, if any. This
        ///     property may be <c>null</c>.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        ///     Gets or sets an array of details about specific errors that
        ///     led to this reported error. This property may be <c>null</c>
        ///     or empty.
        /// </summary>
        public ErrorInfo[] Details { get; set; }

        /// <summary>
        ///     Gets the string representation of this error object.
        /// </summary>
        /// <returns>
        ///     The string representation of this error object.
        /// </returns>
        public override string ToString()
        {
            return this.ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>
        ///     Formats the value of the current instance using the specified format.
        /// </summary>
        /// <param name="format">
        ///     The format to use.
        ///     - or -
        ///     A <c>null</c> reference (Nothing in Visual Basic) to use the
        ///     default format defined for the type of the <see cref="System.IFormattable"/>
        ///     implementation.
        /// </param>
        /// <param name="formatProvider">
        ///     The provider to use to format the value.
        ///     - or -
        ///     A <c>null</c> reference (Nothing in Visual Basic) to obtain the
        ///     numeric format information from the current locale setting of
        ///     the operating system.
        /// </param>
        /// <returns>
        ///     The value of the current instance in the specified format.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch (format)
            {
                case null:
                    // fall through to the default.
                case "G":
                    return ErrorInfoFormatter.PlainText.Format(format, this, formatProvider);

                default:
                    throw new FormatException();
            }
        }

        /// <inheritdoc />
        [SecurityPermission(
            SecurityAction.LinkDemand,
            Flags = SecurityPermissionFlag.SerializationFormatter)]
        [Obsolete("SYSLIB0051")] // Remove this method for 2.0
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Guard.NotNull(info, nameof(info));

            info.AddValue(nameof(Code), this.Code);
            info.AddValue(nameof(Message), this.Message);
            info.AddValue(nameof(Target), this.Target);
            info.AddValue(nameof(Details), this.Details);
        }

        /// <inheritdoc />
        [Obsolete("SYSLIB0051")] // Remove this method for 2.0
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.GetObjectData(info, context);
        }
    }
}
