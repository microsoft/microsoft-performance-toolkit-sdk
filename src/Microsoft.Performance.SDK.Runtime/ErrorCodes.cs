// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     This class represents all of the error codes that can be
    ///     emitted by the runtime. This class acts as a "type-safe"
    ///     enumeration. All new explicit error conditions should be
    ///     added to this class.
    /// </summary>
    public sealed class ErrorCodes
    {
        public static readonly ErrorCodes NoError = new ErrorCodes(
            0,
            "NO_ERROR",
            "No error occurred.");

        //
        // General Errors
        //

        public static readonly ErrorCodes InvalidArgument = new ErrorCodes(
            1,
            "INVALID_ARGUMENT",
            "The specified argument is invalid.");

        public static readonly ErrorCodes FileNotFound = new ErrorCodes(
            2,
            "FILE_NOT_FOUND",
            "The given file cannot be found.");

        public static readonly ErrorCodes DirectoryNotFound = new ErrorCodes(
            3,
            "DIRECTORY_NOT_FOUND",
            "The given directory cannot be found.");

        //
        // Compatability Errors
        //

        public static readonly ErrorCodes SdkVersionIncompatible = new ErrorCodes(
            10000,
            "SDK_VERSION_INCOMPATIBLE",
            "The assembly references a version of the SDK that is not compatible with the host's version of the SDK.");

        //
        // Loading Errors
        //

        public static readonly ErrorCodes AssemblyLoadFailed = new ErrorCodes(
            20000,
            "ASSEMBLY_LOAD_FAILED",
            "The assembly failed to load.");

        public static readonly ErrorCodes LoaderException = new ErrorCodes(
            20001,
            "LOADER_EXCEPTION",
            "The loader encountered an error while loading the assembly.");

        public static readonly ErrorCodes UnableToReflectAssemblyTypes = new ErrorCodes(
            20002,
            "ASEEMBLY_TYPE_REFLECTION_FAILED",
            "The Types in the given Assembly cannot be enumerated.");

        public static readonly ErrorCodes TypeInspectionFailure = new ErrorCodes(
            20003,
            "TYPE_INSPECTION_FAILED",
            "The given type cannot be inspected.");

        public static readonly ErrorCodes FileLoadFailure = new ErrorCodes(
            20004,
            "FILE_LOAD_FAILURE",
            "The assembly was found, but could not be loaded.");

        public static readonly ErrorCodes InvalidCliAssembly = new ErrorCodes(
            20005,
            "INVALID_CLI_ASSEMBLY",
            "The file is not a valid CLI assembly.");

        //
        // Discovery Errors
        //

        public static readonly ErrorCodes DiscoveryFailed = new ErrorCodes(
            30000,
            "DISCOVERY_FAILED",
            "Extension discovery could not complete due to one or more errors.");

        public static readonly ErrorCodes NoObserversRegistered = new ErrorCodes(
            30001,
            "NO_OBSERVERS_REGISTERED",
            "No observers are registered.");

        //
        // Unexpected errors
        //

        public static readonly ErrorCodes Unexpected = new ErrorCodes(
            int.MinValue,
            "UNEXPECTED_ERROR",
            "An unexpected error occurred.");

        /// <summary>
        ///     Gets all of the <see cref="ErrorCodes"/> in the system.
        /// </summary>

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

        public int Number => this.numericCode;

        public string Code => this.code;

        public string Description => this.description;

        public static implicit operator string(ErrorCodes code)
        {
            return ToString(code);
        }

        public static implicit operator int(ErrorCodes code)
        {
            return ToInt(code);
        }

        public static string ToString(ErrorCodes code)
        {
            return code?.ToString() ?? string.Empty;
        }

        public static int ToInt(ErrorCodes code)
        {
            return code?.numericCode ?? 0;
        }

        public override string ToString()
        {
            return this.code;
        }

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

        public override int GetHashCode()
        {
            return this.code.GetHashCode();
        }
    }
}
