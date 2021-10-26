// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Interactive;

namespace Microsoft.Performance.Toolkit.Engine.Tests.Driver
{
    /// <summary>
    ///     This command is used to drive a plugin performing interaction with a user
    ///     via the engine. This exercises the engine's message box implementation.
    ///     This command requires that the tester be present to test the inputs; it
    ///     does not run headless. Further improvements would include replacing stdin
    ///     and stdout to drive this programmatically.
    /// </summary>
    internal sealed class InteractivePluginTestCommand
        : TestCommand
    {
        private readonly FileInfo testFile;

        public InteractivePluginTestCommand(
            string pluginDirectory, 
            bool debugger,
            bool verbose) 
            : base(pluginDirectory, debugger, verbose)
        {
            this.testFile = new FileInfo("test" + InteractiveProcessingSource.Extension);
        }

        internal static Command Create()
        {
            var command = new Command("interactive-test")
            {
                new Argument<string>("plugin-directory", "The directory containing plugins."),
            };

            command.Handler = CreateHandler(typeof(InteractivePluginTestCommand), nameof(InvokeAsync));

            return command;
        }

        internal static Task<int> InvokeAsync(
           bool verbose,
           bool debugger,
           string pluginDirectory)
        {
            var c = new InteractivePluginTestCommand(pluginDirectory, debugger, verbose);
            return c.ExecuteAsync(Signals.CancellationToken);
        }

        private protected override async Task<int> ExecuteCoreAsync(CancellationToken cancellationToken)
        {
            using var plugins = PluginSet.Load(this.PluginDirectory);

            var pluginIsLoaded = false;
            foreach (var p in plugins.ProcessingSourceReferences)
            {
                if (p.Type.GUID == typeof(InteractiveProcessingSource).GUID)
                {
                    pluginIsLoaded = true;
                    break;
                }
            }

            if (!pluginIsLoaded)
            {
                this.Log.Error("The {0} plugin did not load. The test cannot continue.", typeof(InteractiveProcessingSource));
                return 1;
            }

            using (var stream = this.testFile.OpenWrite())
            {
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteLineAsync("Test file");
                    await writer.FlushAsync();
                }
            }

            using var dataSet = DataSourceSet.Create(plugins);
            dataSet.AddFile(this.testFile.FullName);

            var createInfo = new EngineCreateInfo(dataSet.AsReadOnly())
            {
                IsInteractive = true,
            };

            using var engine = Engine.Create(createInfo);

            var results = engine.Process();

            return 0;
        }

        private protected override Task OnExecutedAsync()
        {
            this.testFile.Delete();

            return base.OnExecutedAsync();
        }
    }
}
