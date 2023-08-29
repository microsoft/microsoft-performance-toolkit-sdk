// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    /// <summary>
    ///     Validates the options provided by the command line interface.
    /// </summary>
    /// <typeparam name="TOptions">
    ///     The type of options to validate.
    /// </typeparam>
    /// <typeparam name="TArgs">
    ///     The type of arguments to pass to the application.
    /// </typeparam>
    internal interface IOptionsValidator<TOptions, TArgs>
    {
        /// <summary>
        ///     Validates the options provided by the command line interface and translates them into application arguments.
        /// </summary>
        /// <param name="cliOptions">
        ///     The options provided by the command line interface.
        /// </param>
        /// <param name="validatedAppArgs">
        ///     The application arguments, if the options are valid.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the options are valid; <c>false</c> otherwise.
        /// </returns>
        bool TryValidate(TOptions cliOptions, out TArgs? validatedAppArgs);
    }
}
