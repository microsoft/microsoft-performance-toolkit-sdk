// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.Performance.SDK.Runtime
{
    public sealed class ErrorCodes
    {
        public static readonly ErrorCodes SdkVersionIncompatible;
        public static readonly ErrorCodes UnableToReflectAssemblyTypes;
        public static readonly ErrorCodes PluginRegistrationFailed;
        public static readonly ErrorCodes LoaderException;
        public static readonly ErrorCodes AssemblyLoadFailed;
        public static readonly ErrorCodes TypeInspectionFailure;
        public static readonly ErrorCodes NoObserversRegistered;
        public static readonly ErrorCodes DiscoveryFailed;
        public static readonly ErrorCodes DirectoryNotFound;
        public static readonly ErrorCodes Unexpected;

        public static readonly IReadOnlyCollection<ErrorCodes> All;

        private static readonly Dictionary<string, ErrorCodes> CodeMap;

        private readonly string code;

        static ErrorCodes()
        {
            SdkVersionIncompatible = new ErrorCodes("SDK_VERSION_INCOMPATIBLE");
            UnableToReflectAssemblyTypes = new ErrorCodes("UNABLE_TO_REFLECT_ASSEMBLY_TYPES");
            PluginRegistrationFailed = new ErrorCodes("PLUGIN_REGISTRATION_FAILED");
            LoaderException = new ErrorCodes("LOADER_EXCEPTION");
            AssemblyLoadFailed = new ErrorCodes("ASSEMBLY_LOAD_FAILED");
            TypeInspectionFailure = new ErrorCodes("TYPE_INSPECTION_FAILED");
            NoObserversRegistered = new ErrorCodes("NO_OBSERVERS_REGISTERED");
            DiscoveryFailed = new ErrorCodes("DISCOVERY_FAILED");
            DirectoryNotFound = new ErrorCodes("DIRECTORY_NOT_FOUND");
            Unexpected = new ErrorCodes("UNEXPECTED_ERROR");

            CodeMap = typeof(ErrorCodes)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.FieldType == typeof(ErrorCodes))
                .Select(x => x.GetValue(null))
                .Cast<ErrorCodes>()
                .ToDictionary(
                    x => x.ToString(),
                    x => x);

            All = CodeMap.Values;
        }

        private ErrorCodes(string code)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(code));

            this.code = code;
        }

        public string Code => this.code;

        public static implicit operator string(ErrorCodes code)
        {
            return ToString(code);
        }

        public static string ToString(ErrorCodes code)
        {
            return code?.ToString() ?? string.Empty; 
        }

        public override string ToString()
        {
            return this.code;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals (obj, this))
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
