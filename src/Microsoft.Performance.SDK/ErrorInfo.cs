// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents an error related to processing specific 
    ///     data sources.  For example, when processing and ETW
    ///     trace, there could be time inversion, and so you would
    ///     need to surface that to the user. 
    /// </summary>
    [Serializable]
    public sealed class ErrorInfo
        : IFormattable,
          ISerializable
    {
        private const string TypeNameFieldName = "__ConcreteType";
        private const string HasInnerFieldName = "__HasInner";

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

        private ErrorInfo(SerializationInfo info, StreamingContext context)
        {
            Guard.NotNull(info, nameof(info));

            this.Code = info.GetString(nameof(Code));
            this.Message = info.GetString(nameof(Message));
            this.Target = info.GetString(nameof(Target));
            this.Details = (ErrorInfo[])info.GetValue(nameof(Details), typeof(ErrorInfo[]));
            if (info.GetBoolean(HasInnerFieldName))
            {
                var typeName = info.GetString(TypeNameFieldName);
                var innerErrorType = Type.GetType(typeName);
                this.Inner = (InnerError)info.GetValue(nameof(Inner), innerErrorType);
            }
            else
            {
                this.Inner = null;
            }
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
        ///     Gets or sets an object containing more specific information
        ///     that the current object about the error. This property may
        ///     be <c>null</c>.
        /// </summary>
        public InnerError Inner { get; set; }

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
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Guard.NotNull(info, nameof(info));

            info.AddValue(nameof(Code), this.Code);
            info.AddValue(nameof(Message), this.Message);
            info.AddValue(nameof(Target), this.Target);
            info.AddValue(nameof(Details), this.Details);

            if (this.Inner != null)
            {
                info.AddValue(HasInnerFieldName, true);
                info.AddValue(TypeNameFieldName, this.Inner.GetType().AssemblyQualifiedName);
                info.AddValue(nameof(Inner), this.Inner, this.Inner.GetType());
            }
            else
            {
                info.AddValue(HasInnerFieldName, false);
            }
        }

        /// <inheritdoc />
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.GetObjectData(info, context);
        }
    }

    /// <summary>
    ///     Represents additional information about an <see cref="ErrorInfo"/>
    ///     instance.
    /// </summary>
    [Serializable]
    public class InnerError
        : ISerializable
    {
        private const string TypeNameFieldName = "__ConcreteType";
        private const string HasInnerFieldName = "__HasInner";

        /// <summary>
        ///     Initializes a new instance of the <see cref="InnerError"/>
        ///     class.
        /// </summary>
        /// <param name="code">
        ///     A processor specific error code.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="code"/> is <c>null</c>.
        /// </exception>
        protected InnerError(string code)
        {
            Guard.NotNull(code, nameof(code));

            this.Code = code;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="InnerError"/>
        ///     from serialized data.
        /// </summary>
        /// <param name="info">
        ///     The serialization info.
        /// </param>
        /// <param name="context">
        ///     The streaming context.
        /// </param>
        protected InnerError(SerializationInfo info, StreamingContext context)
        {
            Guard.NotNull(info, nameof(info));

            this.Code = info.GetString(nameof(Code));

            if (info.GetBoolean(HasInnerFieldName))
            {
                var typeName = info.GetString(TypeNameFieldName);
                var innerErrorType = Type.GetType(typeName);
                this.Inner = (InnerError)info.GetValue(nameof(Inner), innerErrorType);
            }
            else
            {
                this.Inner = null;
            }
        }

        /// <summary>
        ///     Gets the processor specific code for this error.
        /// </summary>
        public string Code { get; }

        /// <summary>
        ///     Gets or sets an object containing more specific information
        ///     that the current object about the error. This property may
        ///     be <c>null</c>.
        /// </summary>
        public InnerError Inner { get; set; }

        /// <inheritdoc />
        [SecurityPermission(
            SecurityAction.LinkDemand,
            Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Guard.NotNull(info, nameof(info));

            info.AddValue(nameof(Code), this.Code);
            if (this.Inner != null)
            {
                info.AddValue(HasInnerFieldName, true);
                info.AddValue(TypeNameFieldName, this.Inner.GetType().AssemblyQualifiedName);
                info.AddValue(nameof(Inner), this.Inner, this.Inner.GetType());
            }
            else
            {
                info.AddValue(HasInnerFieldName, false);
            }
        }

        /// <inheritdoc />
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.GetObjectData(info, context);
        }
    }
}
