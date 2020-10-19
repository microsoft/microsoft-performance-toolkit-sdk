// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;

namespace Microsoft.Performance.SDK.Runtime
{
    internal static class TypeExtensions
    {
        internal static bool TryGetEmptyPublicConstructor(this Type type, out ConstructorInfo constructor)
        {
            constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor != null && constructor.IsPublic)
            {
                return true;
            }

            constructor = null;
            return false;
        }
    }
}
