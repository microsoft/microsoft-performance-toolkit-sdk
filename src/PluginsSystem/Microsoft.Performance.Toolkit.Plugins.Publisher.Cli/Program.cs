// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Publisher.Cli
{
    public sealed class Program
    {
        static void Main(string[] args)
        {
            CommandDispatcher.Default.Main(args);
        }
    }
}
