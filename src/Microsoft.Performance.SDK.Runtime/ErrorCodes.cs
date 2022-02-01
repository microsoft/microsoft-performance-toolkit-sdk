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
        /// <summary>
        ///     No error occurred.
        /// </summary>
        public static readonly ErrorCodes NoError = new ErrorCodes(
            0,
            "NO_ERROR",
            "No error occurred.");

        //
        // General Errors
        //

        /// <summary>
        ///     The specified argument is invalid.
        /// </summary>
        public static readonly ErrorCodes InvalidArgument = new ErrorCodes(
            1,
            "INVALID_ARGUMENT",
            "The specified argument is invalid.");

        /// <summary>
        ///     The given file cannot be found.
        /// </summary>
        public static readonly ErrorCodes FileNotFound = new ErrorCodes(
            2,
            "FILE_NOT_FOUND",
            "The given file cannot be found.");

        /// <summary>
        ///     The given directory cannot be found.
        /// </summary>
        public static readonly ErrorCodes DirectoryNotFound = new ErrorCodes(
            3,
            "DIRECTORY_NOT_FOUND",
            "The given directory cannot be found.");

        //
        // Compatibility Errors
        //

        /// <summary>
        ///     The assembly references a version of the SDK that is not compatible with the host's version of the SDK.
        /// </summary>
        public static readonly ErrorCodes SdkVersionIncompatible = new ErrorCodes(
            10000,
            "SDK_VERSION_INCOMPATIBLE",
            "The assembly references a version of the SDK that is not compatible with the host's version of the SDK.");

        //
        // Loading Errors
        //

        /// <summary>
        ///     The assembly failed to load.
        /// </summary>
        public static readonly ErrorCodes AssemblyLoadFailed = new ErrorCodes(
            20000,
            "ASSEMBLY_LOAD_FAILED",
            "The assembly failed to load.");

        /// <summary>
        ///     The loader encountered an error while loading the assembly.
        /// </summary>
        public static readonly ErrorCodes LoaderException = new ErrorCodes(
            20001,
            "LOADER_EXCEPTION",
            "The loader encountered an error while loading the assembly.");

        /// <summary>
        ///     The Types in the given Assembly cannot be enumerated.
        /// </summary>
        public static readonly ErrorCodes UnableToReflectAssemblyTypes = new ErrorCodes(
            20002,
            "ASEEMBLY_TYPE_REFLECTION_FAILED",
            "The Types in the given Assembly cannot be enumerated.");

        /// <summary>
        ///     The given type cannot be inspected.
        /// </summary>
        public static readonly ErrorCodes TypeInspectionFailure = new ErrorCodes(
            20003,
            "TYPE_INSPECTION_FAILED",
            "The given type cannot be inspected.");

        /// <summary>
        ///     The assembly was found, but could not be loaded.
        /// </summary>
        public static readonly ErrorCodes FileLoadFailure = new ErrorCodes(
            20004,
            "FILE_LOAD_FAILURE",
            "The assembly was found, but could not be loaded.");

        /// <summary>
        ///     The file is not a valid CLI assembly.
        /// </summary>
        public static readonly ErrorCodes InvalidCliAssembly = new ErrorCodes(
            20005,
            "INVALID_CLI_ASSEMBLY",
            "The file is not a valid CLI assembly.");

        //
        // Discovery Errors
        //

        /// <summary>
        ///     Extension discovery could not complete due to one or more errors.
        /// </summary>
        public static readonly ErrorCodes DiscoveryFailed = new ErrorCodes(
            30000,
            "DISCOVERY_FAILED",
            "Extension discovery could not complete due to one or more errors.");

        /// <summary>
        ///     No observers are registered.
        /// </summary>
        public static readonly ErrorCodes NoObserversRegistered = new ErrorCodes(
            30001,
            "NO_OBSERVERS_REGISTERED",
            "No observers are registered.");

        //
        // Extension Errors
        //

        /// <summary>
        ///     A dependency cycle has been detected between extensions.
        /// </summary>
        public static ErrorCodes EXTENSION_DependencyCycle = new ErrorCodes(
            40001,
            "EXTENSION_DEPENDENCY_CYCLE",
            "A dependency cycle has been detected between extensions.");

        /// <summary>
        ///     One or more dependencies of the extension is missing.
        /// </summary>
        public static ErrorCodes EXTENSION_MissingRequirement = new ErrorCodes(
            40002,
            "EXTENSION_MISSING_REQUIREMENT",
            "One or more dependencies of the extension is missing.");

        /// <summary>
        ///     One or more dependencies in the extension's dependency chain is missing.
        /// </summary>
        public static ErrorCodes EXTENSION_MissingIndirectRequirement = new ErrorCodes(
            40003,
            "EXTENSION_MISSING_INDIRECT_REQUIREMENT",
            "One or more dependencies in the extension's dependency chain is missing.");

        /// <summary>
        ///     An error occurred when processing the dependencies of this extension.
        /// </summary>
        public static ErrorCodes EXTENSION_Error = new ErrorCodes(
            40004,
            "EXTENSION_ERROR",
            "An error occurred when processing the dependencies of this extension.");

        /// <summary>
        ///     One or more dependencies in the extension's dependency chain has an error.
        /// </summary>
        public static ErrorCodes EXTENSION_IndirectError = new ErrorCodes(
            40005,
            "EXTENSION_INDIRECT_ERROR",
            "One or more dependencies in the extension's dependency chain has an error.");

        /// <summary>
        ///     The Data cooker {0} referenced by {1} is unrecognized.
        /// </summary>
        public static ErrorCodes EXTENSION_UnrecognizedDataCookerPath = new ErrorCodes(
            40006,
            "EXTENSION_UNRECOGNIZED_PATH",
            "The referenced data cooker is unrecognized.");

        /// <summary>
        ///     A source data cooker may not depend on data cookers from other sources.
        /// </summary>
        public static ErrorCodes EXTENSION_CrossSourceDependency = new ErrorCodes(
            40007,
            "EXTENSION_CROSS_SOURCE_DEPENDENCY",
            "A source data cooker may not depend on data cookers from other sources.");

        /// <summary>
        ///     A source data cooker may not depend on a data processor.
        /// </summary>
        public static ErrorCodes EXTENSION_DisallowedDataProcessorDependency = new ErrorCodes(
            40008,
            "EXTENSION_DISALLOWED_DATA_PROCESSOR_DEPENDENCY",
            "A source data cooker may not depend on a data processor.");

        /// <summary>
        ///     A requested dependency on an unknown data extension type is not supported.
        /// </summary>
        public static ErrorCodes EXTENSION_UnknownDependencyType = new ErrorCodes(
            40009,
            "EXTENSION_UNKNOWN_DEPENDENCY_TYPE",
            "A requested dependency on an unknown data extension type is not supported.");

        //
        // Unexpected errors
        //

        /// <summary>
        ///     An unexpected error occurred.
        /// </summary>
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
