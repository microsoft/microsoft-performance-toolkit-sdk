// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    // TODO: #293 This class should be moved to the SDK.
    /// <summary>
    ///     This class represents all of the error codes that can be
    ///     emitted by the plugins system runtime. This class acts as a "type-safe"
    ///     enumeration. All new explicit error conditions should be
    ///     added to this class.
    /// </summary>
    public sealed class ErrorCodes
    {
        //
        // Plugins System Errors
        //

        /// <summary>
        ///    An error occurred when interacting with a plugin source.
        /// </summary>
        public static ErrorCodes PLUGINS_SYSTEM_PluginSourceException = new ErrorCodes(
            50001,
            "PLUGINS_MANAGER_PluginSourceException",
            "An error occurred when interacting with a plugin source.");

        /// <summary>
        ///     No available plugin system resource was found to perform requests against the plugin source.
        /// </summary>
        public static ErrorCodes PLUGINS_SYSTEM_PluginsSystemResourceNotFound = new ErrorCodes(
            50002,
            "PLUGINS_SYSTEM_PluginsSystemResourceNotFound",
            "No available plugin system resource was found to perform requests against the plugin source.");

        //
        // We do duplicate checking on the numeric and string codes
        // as part of construction of this collection. If there are
        // any duplicates, then the ToDictionary calls will fail,
        // signalling a developer error.
        //
        public static readonly IReadOnlyCollection<ErrorCodes> All = typeof(ErrorCodes)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.FieldType == typeof(ErrorCodes))
                .Select(x => x.GetValue(null))
                .Cast<ErrorCodes>()
                .ToDictionary(x => x.numericCode, x => x)
                .Values
                .ToDictionary(x => x.code, x => x)
                .Values
                .ToList()
                .AsReadOnly();

        private readonly int numericCode;
        private readonly string code;
        private readonly string description;

        private ErrorCodes(
            int numericCode,
            string code,
            string description)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(code));
            Debug.Assert(!string.IsNullOrWhiteSpace(description));

            this.numericCode = numericCode;
            this.code = code.ToUpperInvariant();
            this.description = description;
        }

        /// <summary>
        ///     Gets the number of this error.
        /// </summary>
        public int Number => this.numericCode;

        /// <summary>
        ///     Gets the code of this error.
        /// </summary>
        public string Code => this.code;

        /// <summary>
        ///     Gets a human-readable description of this error.
        /// </summary>
        public string Description => this.description;

        /// <summary>
        ///     Implicitly casts an instance of the <see cref="ErrorCodes"/>
        ///     class to a <see cref="string"/>.
        /// </summary>
        /// <param name="code">
        ///     The <see cref="ErrorCodes"/> to cast to a <see cref="string"/>.
        /// </param>
        public static implicit operator string(ErrorCodes code)
        {
            return ToString(code);
        }

        /// <summary>
        ///     Implicitly casts an instance of the <see cref="ErrorCodes"/>
        ///     class to a <see cref="int"/>.
        /// </summary>
        /// <param name="code">
        ///     The <see cref="ErrorCodes"/> to cast to a <see cref="int"/>.
        /// </param>
        public static implicit operator int(ErrorCodes code)
        {
            return ToInt(code);
        }

        /// <summary>
        ///     Converts an instance of the <see cref="ErrorCodes"/>
        ///     class to a <see cref="string"/>.
        /// </summary>
        /// <param name="code">
        ///     The <see cref="ErrorCodes"/> to cast to a <see cref="string"/>.
        /// </param>
        public static string ToString(ErrorCodes code)
        {
            return code?.ToString() ?? string.Empty;
        }

        /// <summary>
        ///     Converts an instance of the <see cref="ErrorCodes"/>
        ///     class to a <see cref="int"/>.
        /// </summary>
        /// <param name="code">
        ///     The <see cref="ErrorCodes"/> to cast to a <see cref="int"/>.
        /// </param>
        public static int ToInt(ErrorCodes code)
        {
            return code?.numericCode ?? 0;
        }

        /// <summary>
        ///     Gets the <see cref="string"/> representation
        ///     of this instance.
        /// </summary>
        /// <returns>
        ///     The <see cref="string"/> representation of this
        ///     instance.
        /// </returns>
        public override string ToString()
        {
            return this.code;
        }

        /// <summary>
        ///     Determines whether this instance is considered
        ///     to be equal to the given <see cref="object"/>.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="object"/> to check for equality.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this instance is considered to
        ///     be equal to <paramref name="obj"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
            {
                return true;
            }

            var other = obj as ErrorCodes;

            return other != null &&
                this.code.Equals(other.code);
        }

        /// <summary>
        ///     Gets an integer hash code for this
        ///     instance.
        /// </summary>
        /// <returns>
        ///     An integer hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return this.code.GetHashCode();
        }
    }
}
