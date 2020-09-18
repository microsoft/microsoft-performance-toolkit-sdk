// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="ProcessorOptions"/> instances.
    /// </summary>
    public static class ProcessorOptionExtensions
    {
        /// <summary>
        ///     Determines whether the given instance contains the
        ///     given option.
        /// </summary>
        /// <param name="self">
        ///     The instance to interrogate.
        /// </param>
        /// <param name="flag">
        ///     The flag to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="flag"/> is present in
        ///     <paramref name="self"/>; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="flag"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static bool IsOptionPresent(
            this ProcessorOptions self,
            Option flag)
        {
            Guard.NotNull(flag, nameof(flag));

            return self.IsOptionPresent(flag.Id);
        }

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
            this ProcessorOptions self,
            object id)
        {
            Guard.NotNull(self, nameof(self));

            return self.Options.Any(x => x.Id.Equals(id));
        }

        /// <summary>
        ///     Attempts to get the argument of a given option.
        ///     If the option allows for multiple options or the
        ///     option is not present, then this method will return
        ///     <c>false</c>.
        /// </summary>
        /// <param name="self">
        ///     The instance to interrogate.
        /// </param>
        /// <param name="option">
        ///     The option whose argument is to be retrieved.
        /// </param>
        /// <param name="argument">
        ///     The argument, if it exists; <c>null</c> otherwise.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the argument was found; <c>false</c>
        ///     otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="option"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static bool TryGetOptionArgument(
            this ProcessorOptions self,
            Option option,
            out string argument)
        {
            return self.TryGetOptionArgument(option.Id, out argument);
        }

        /// <summary>
        ///     Attempts to get the argument of a given option.
        ///     If the option allows for multiple options or the
        ///     option is not present, then this method will return
        ///     <c>false</c>.
        /// </summary>
        /// <param name="self">
        ///     The instance to interrogate.
        /// </param>
        /// <param name="id">
        ///     The option whose argument is to be retrieved.
        /// </param>
        /// <param name="argument">
        ///     The argument, if it exists; <c>null</c> otherwise.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the argument was found; <c>false</c>
        ///     otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="id"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static bool TryGetOptionArgument(
            this ProcessorOptions self,
            object id,
            out string argument)
        {
            Guard.NotNull(self, nameof(self));
            Guard.NotNull(id, nameof(id));

            if (self.Options.TryGetOptionArguments(id, out IEnumerable<string> allArguments))
            {
                var enumerator = allArguments.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    // save the first argument.
                    argument = enumerator.Current;
                    if (enumerator.MoveNext())
                    {
                        // we have more than one argument, so fail.
                        argument = null;
                    }
                }
                else
                {
                    // no arguments
                    argument = null;
                }
            }
            else
            {
                // no arguments
                argument = null;
            }

            return argument != null;
        }

        /// <summary>
        ///     Attempts to get all of the arguments passed to a given
        ///     option.
        /// </summary>
        /// <param name="self">
        ///     The instance to interrogate.
        /// </param>
        /// <param name="option">
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
        ///     <paramref name="option"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static bool TryGetOptionArguments(
            this ProcessorOptions self,
            Option option,
            out IEnumerable<string> arguments)
        {
            Guard.NotNull(self, nameof(self));
            Guard.NotNull(option, nameof(option));

            return self.Options.TryGetOptionArguments(option.Id, out arguments);
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
            this ProcessorOptions self,
            char id,
            out IEnumerable<string> arguments)
        {
            Guard.NotNull(self, nameof(self));

            return self.Options.TryGetOptionArguments(id, out arguments);
        }
    }
}
