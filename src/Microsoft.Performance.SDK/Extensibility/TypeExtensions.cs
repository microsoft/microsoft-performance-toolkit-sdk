// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods
    ///     for interacting with <see cref="Type"/>s.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     Determines if the candidate type is public, instantiable and of the expected type.
        /// </summary>
        /// <param name="candidateType">
        ///     Candidate type.
        /// </param>
        /// <param name="expectedType">
        ///     Expected type.
        /// </param>
        /// <returns>
        ///     True if criteria is met.
        /// </returns>
        public static bool IsPublicAndInstantiatableOfType(this Type candidateType, Type expectedType)
        {
            Guard.NotNull(candidateType, nameof(candidateType));
            Guard.NotNull(expectedType, nameof(expectedType));

            if (candidateType.IsPublic() &&
                candidateType.IsInstantiatable())
            {
                if (candidateType.Implements(expectedType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
