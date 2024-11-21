// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// See https://stackoverflow.com/a/64749403
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}

    internal class ExtensionAttribute : Attribute { }

    internal class RequiredMemberAttribute : Attribute { }

    internal class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string name) { }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    internal class SetsRequiredMembersAttribute : Attribute {}
}