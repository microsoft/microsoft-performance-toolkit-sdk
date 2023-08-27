// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    internal interface ICommand<TArgs>
        where TArgs : class
    {
        int Run(TArgs args);
    }
}
