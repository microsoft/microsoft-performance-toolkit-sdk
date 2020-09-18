// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="OptionInstance"/>s.
    /// </summary>
    public static class OptionInstanceExtensions
    {
        /// <summary>
        ///     Determines whether the given instance contains the
        ///     given option.
        /// </summary>
        /// <param name="self">
        ///     The instance to interrogate.
        /// </param>
        /// <param name="id">
        ///     The flag to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="id"/> is present in
        ///     <paramref name="self"/>; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static bool IsOptionPresent(
            this IEnumerable<OptionInstance> self,
            object id)
        {
            Guard.NotNull(self, nameof(self));

            return self.Any(x => x.Id == id);
        }

        /// <summary>
        ///     Attempts to get all of the arguments passed to a given
        ///     option.
        /// </summary>
        /// <param name="self">
        ///     The instance to interrogate.
        /// </param>
        /// <param name="id">
        ///     The option whose arguments are to be retrieved.
        /// </param>
        /// <param name="arguments">
        ///     The arguments, if the option is found. If the option
        ///     exists but there are no arguments, this method will still
        ///     return <c>true</c> and this parameter will be set to
        ///     the empty collection.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the option was found; <c>false</c>
        ///     otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static bool TryGetOptionArguments(
            this IEnumerable<OptionInstance> self,
            object id,
            out IEnumerable<string> arguments)
        {
            Guard.NotNull(self, nameof(self));

            var option = self.SingleOrDefault(x => x.Id.Equals(id));
            arguments = option?.Arguments;
            return arguments != null;
        }
    }
}
