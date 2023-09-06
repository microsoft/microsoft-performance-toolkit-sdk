// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    /// <summary>
    ///     Represents a command that can be executed via the CLI.
    /// </summary>
    /// <typeparam name="TArgs">
    ///     The type of arguments that the command accepts.
    /// </typeparam>
    internal interface ICommand<TArgs>
        where TArgs : class
    {
        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <param name="args">
        ///     The arguments to the command.
        /// </param>
        /// <returns>
        ///     The exit code of the command. A value of 0 indicates success. A value of 1 indicates failure.
        /// </returns>
        int Run(TArgs args);
    }
}
