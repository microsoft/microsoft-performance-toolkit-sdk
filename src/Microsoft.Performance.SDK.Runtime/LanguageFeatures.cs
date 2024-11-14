// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// See https://stackoverflow.com/a/64749403
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}

    public class ExtensionAttribute : Attribute { }

    public class RequiredMemberAttribute : Attribute { }

    public class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string name) { }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    public class SetsRequiredMembersAttribute : Attribute {}
}