// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.Linq;
using System.Reflection;

namespace Microsoft.Performance.Toolkit.Engine.Tests.Driver
{
    public sealed partial class Program
    {
        public int Run(string[] args)
        {
            var commandTypes = this.GetType().Assembly
                .GetTypes()
                .Where(x => typeof(TestCommand).IsAssignableFrom(x))
                .Where(x => !x.IsAbstract);

            var createMethods = commandTypes
                .Select(x => x.GetMethod(
                    "Create",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    Type.EmptyTypes,
                    null))
                .OfType<MethodInfo>()
                .Where(x => x.ReturnType == typeof(Command));

            var commands = createMethods
                .Select(x => x.Invoke(null, null) as Command)
                .OfType<Command>();

            var command = new RootCommand();
            foreach (var c in commands)
            {
                command.AddCommand(c);
            }

            command.AddGlobalOption(new Option<bool>("verbose", "Whether to enable verbose output."));
            command.AddGlobalOption(new Option<bool>("debugger", "Whether to wait for a debugger to attach."));

            return command.Invoke(args);
        }
    }
}
