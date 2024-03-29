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
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="candidateType"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="expectedType"/> is <c>null</c>.
        /// </exception>
        public static bool IsPublicAndInstantiatableOfType(this Type candidateType, Type expectedType)
        {
            Guard.NotNull(candidateType, nameof(candidateType));
            Guard.NotNull(expectedType, nameof(expectedType));

            return candidateType.IsPublic() && IsInstantiatableOfType(candidateType, expectedType);
        }

        /// <summary>
        ///     Determines if the candidate type is instantiable and of the expected type.
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
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="candidateType"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="expectedType"/> is <c>null</c>.
        /// </exception>
        public static bool IsInstantiatableOfType(this Type candidateType, Type expectedType)
        {
            Guard.NotNull(candidateType, nameof(candidateType));
            Guard.NotNull(expectedType, nameof(expectedType));

            return candidateType.IsInstantiatable() && candidateType.Implements(expectedType);
        }
    }
}
