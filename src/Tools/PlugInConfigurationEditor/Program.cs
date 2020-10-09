// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Performance.SDK.PlugInConfiguration;
using NuGet.Versioning;

namespace PlugInConfigurationEditor
{
    internal class Program
    {
        // When a System.CommandLine invoked method throws an exception, that exception is caught and displayed before
        // ever returning control to our code. The result is an ugly stack trace that the user doesn't need to see. I
        // haven't found a way around that other than catching the exception and storing it.
        //
        private Exception _parsingException;

        internal static void Main(string[] args)
        {
            var program = new Program();
            program.Run(args);
        }

        private void Run(string[] args)
        {
            try
            {
                ParseArgs(args);
            }
            catch (ConfigurationException e)
            {
                Logger.Error("{0}", e.Message);
            }
            catch (Exception e)
            {
                // We don't expect to hit this. If we do, the code should be updated to catch it at its source.
                Logger.Fatal($"Uncaught exception: {e}");
            }
        }

        private void ParseArgs(string[] args)
        {
            var rootCommand = new RootCommand
            {
                Description =
                    "Create or modify a PerfToolkit plug-in configuration file. If a directory is not " +
                    "specified, the current directory is assumed."
            };

            var verboseOption = new Option(
                    new[] { "-v", "--verbose" },
                    description: "Enable verbose logging");
            rootCommand.AddGlobalOption(verboseOption);

            var dirArgument = new Argument<string>(name: "dir", description: "Directory of configuration file.");

            rootCommand.AddCommand(CreateCommandCreate(dirArgument));
            rootCommand.AddCommand(CreateCommandPrint(dirArgument));
            rootCommand.AddCommand(CreateCommandAdd(dirArgument));
            rootCommand.AddCommand(CreateCommandDel(dirArgument));

            try
            {
                rootCommand.Invoke(args);
                if (_parsingException != null)
                {
                    throw _parsingException;
                }
            }
            catch (ConfigurationException e)
            {
                Logger.Error("{0}", e.Message);
            }
            catch (Exception e)
            {
                // We don't expect to hit this. If we do, the code should be updated to catch it at its source.
                Logger.Fatal($"Uncaught exception: {e}");
            }
        }

        private Command CreateCommandCreate(Argument dirArgument)
        {
            var command = new Command("Create", "Create a plug-in configuration file.")
            {
                Handler = GetCommandHandler(nameof(ProcessCommandCreate)),
            };

            command.AddArgument(dirArgument);
            command.AddArgument(new Argument<string>(name: "name", description: "Custom data source name"));

            command.AddOption(new Option<SemanticVersion>(
                new[] { "-c", "--version" },
                ParseVersion,
                isDefault: true,
                description: "Sets the configuration version."));
            command.AddOption(new Option<bool>(
                new[] { "-o", "--overwrite" },
                getDefaultValue: () => false,
                description: "Overwrite an existing file."));

            return command;
        }

        private Command CreateCommandPrint(Argument dirArgument)
        {
            var command = new Command("Print", "Prints contents of a configuration file.")
            {
                Handler = GetCommandHandler(nameof(ProcessCommandPrint)),
            };

            command.AddArgument(dirArgument);

            return command;
        }

        private Command CreateCommandAdd(Argument dirArgument)
        {
            var command = new Command("Add", "Adds an element to an existing configuration file.");
            command.AddCommand(CreateCommandAddOption(dirArgument));
            command.AddCommand(CreateCommandAddApplication(dirArgument));
            command.AddCommand(CreateCommandAddRuntime(dirArgument));

            return command;
        }

        private Command CreateCommandAddOption(Argument dirArgument)
        {
            var command = new Command("Option", "Adds an option to an existing configuration file.")
            {
                Handler = GetCommandHandler(nameof(ProcessesCommandAddOption)),
            };

            command.AddArgument(dirArgument);
            command.AddArgument(new Argument<string>("name", "Name of the option."));
            command.AddArgument(new Argument<string>("description", "Description of the option."));

            return command;
        }

        private Command CreateCommandAddApplication(Argument dirArgument)
        {
            var command = new Command("App", "Adds an application to one or more elements.")
            {
                Handler = GetCommandHandler(nameof(ProcessesCommandAddAppToOption)),
            };

            command.AddArgument(dirArgument);
            command.AddArgument(new Argument<string>("option", "Name of the option."));
            command.AddArgument(new Argument<string>("name", "Name of the application."));

            return command;
        }

        private Command CreateCommandAddRuntime(Argument dirArgument)
        {
            var command = new Command("Runtime", "Adds a runtime to one or more elements.")
            {
                Handler = GetCommandHandler(nameof(ProcessesCommandAddRuntimeToOption)),
            };

            command.AddArgument(dirArgument);
            command.AddArgument(new Argument<string>("option", "Name of the option."));
            command.AddArgument(new Argument<string>("name", "Name of the runtime."));

            return command;
        }

        private Command CreateCommandDel(Argument dirArgument)
        {
            var command = new Command("Del", "Deletes an element from an existing configuration file.");
            command.AddCommand(CreateCommandDelOption(dirArgument));
            command.AddCommand(CreateCommandDelApp(dirArgument));
            command.AddCommand(CreateCommandDelRuntime(dirArgument));

            return command;
        }

        private Command CreateCommandDelOption(Argument dirArgument)
        {
            var command = new Command(
                "Option", "Deletes an option from an existing configuration file.")
            {
                Handler = GetCommandHandler(nameof(ProcessesCommandDelOption)),
            };

            command.AddArgument(dirArgument);
            command.AddArgument(new Argument<string>("name", "Name of the option."));

            return command;
        }

        private Command CreateCommandDelApp(Argument dirArgument)
        {
            var command = new Command("App", "Deletes an application from one or more elements.")
            {
                Handler = GetCommandHandler(nameof(ProcessesCommandDelAppFromOption)),
            };

            command.AddArgument(dirArgument);
            command.AddArgument(new Argument<string>("option", "Name of the option."));
            command.AddArgument(new Argument<string>("name", "Name of the application."));

            return command;
        }

        private Command CreateCommandDelRuntime(Argument dirArgument)
        {
            var command = new Command("Runtime", "Deletes runtime from one or more elements.")
            {
                Handler = GetCommandHandler(nameof(ProcessesCommandDelRuntimeFromOption)),
            };

            command.AddArgument(dirArgument);
            command.AddArgument(new Argument<string>("option", "Name of the option."));
            command.AddArgument(new Argument<string>("name", "Name of the runtime."));

            return command;
        }

        private void ProcessCommandCreate(
            bool verbose,
            string dir,
            bool overwrite,
            string name,
            SemanticVersion version)
        {
            try
            {
                if (verbose)
                {
                    Logger.EnableVerbose();
                }

                string file = GetFileName(dir, false);
                if (File.Exists(file))
                {
                    if (!overwrite)
                    {
                        Logger.Error($"File already exists: {file}. Use option '-o' to overwrite.");
                        return;
                    }
                }
                else if (string.IsNullOrWhiteSpace(file))
                {
                    Logger.Error($"Configuration file name is invalid: '{file}'.");
                    return;
                }

                var configuration = new PlugInConfiguration(name, version, new HashSet<ConfigurationOption>());

                using var stream = File.Create(file);
                PlugInConfigurationSerializer.WriteToStream(configuration, stream, new BridgeLogger());
                stream.Flush(true);

                Logger.Information($"New configuration logged to {file}.");
            }
            catch (Exception e)
            {
                _parsingException = e;
            }
        }

        private void ProcessCommandPrint(
            bool verbose,
            string dir)
        {
            try
            {
                if (verbose)
                {
                    Logger.EnableVerbose();
                }

                string file = GetFileName(dir);
                using var fileStream = File.Open(file, FileMode.Open, FileAccess.ReadWrite);

                var existingConfiguration = PlugInConfigurationSerializer.ReadFromStream(fileStream, new BridgeLogger());
                if (existingConfiguration == null)
                {
                    return;
                }

                Logger.Information($"Plug-In:\t{existingConfiguration.PlugInName}");
                Logger.Information($"Version:\t{existingConfiguration.Version}");

                foreach (var option in existingConfiguration.Options)
                {
                    Logger.Information("\n");
                    Logger.Information($"Option:\t{option.Name}");
                    Logger.Information($"Description:\t{option.Description}");

                    string applications = "none";
                    if (option.Applications.Any())
                    {
                        applications = string.Join(", ", option.Applications);
                    }
                    Logger.Information($"Applications: {applications}");

                    string runtimes = "none";
                    if (option.Runtimes.Any())
                    {
                        runtimes = string.Join(", ", option.Runtimes);
                    }
                    Logger.Information($"Runtimes: {runtimes}");
                }
            }
            catch (Exception e)
            {
                _parsingException = e;
            }
        }

        private void ProcessesCommandAddOption(
            bool verbose,
            string dir,
            string name,
            string description)
        {
            try
            {
                if (verbose)
                {
                    Logger.EnableVerbose();
                }

                if (!PlugInConfigurationValidation.ValidateElementName(name))
                {
                    Logger.Error(
                        "The name is invalid '{0}'- " + PlugInConfigurationValidation.ValidCharactersMessage,
                        name);
                    return;
                }

                string file = GetFileName(dir);
                using var fileStream = File.Open(file, FileMode.Open, FileAccess.ReadWrite);

                var logger = new BridgeLogger();
                var existingConfiguration = PlugInConfigurationSerializer.ReadFromStream(fileStream, logger);
                if (existingConfiguration == null)
                {
                    return;
                }

                ConfigurationOption newOption = new ConfigurationOption(name, description);
                var options = new HashSet<ConfigurationOption>(existingConfiguration.Options);
                if (!options.Add(newOption))
                {
                    Logger.Error("Option {0} already exists.", name);
                    return;
                }

                fileStream.Position = 0;

                var configuration = new PlugInConfiguration(
                    existingConfiguration.PlugInName,
                    existingConfiguration.Version,
                    options);

                PlugInConfigurationSerializer.WriteToStream(configuration, fileStream, logger);

                Logger.Information($"Added option {name}.");
            }
            catch (Exception e)
            {
                _parsingException = e;
            }
        }

        private void ProcessesCommandDelOption(
            bool verbose,
            string dir,
            string name)
        {
            try
            {
                if (verbose)
                {
                    Logger.EnableVerbose();
                }

                string file = GetFileName(dir);
                using var fileStream = File.Open(file, FileMode.Open, FileAccess.ReadWrite);

                var logger = new BridgeLogger();
                var existingConfiguration = PlugInConfigurationSerializer.ReadFromStream(fileStream, logger);
                if (existingConfiguration == null)
                {
                    return;
                }

                HashSet<ConfigurationOption> options = new HashSet<ConfigurationOption>(existingConfiguration.Options);
                var targetOption = options.FirstOrDefault(o => StringComparer.InvariantCulture.Equals(o.Name, name));
                if (targetOption == null)
                {
                    Logger.Warning("Option {0} was not found.", name);
                    return;
                }

                if (!options.Remove(targetOption))
                {
                    Debug.Assert(false);
                    Logger.Error("Failed to remove option {0}.", name);
                    return;
                }

                fileStream.Position = 0;

                var configuration = new PlugInConfiguration(
                    existingConfiguration.PlugInName,
                    existingConfiguration.Version,
                    options);

                PlugInConfigurationSerializer.WriteToStream(configuration, fileStream, logger);

                Logger.Information($"Deleted option {name}.");
            }
            catch (Exception e)
            {
                _parsingException = e;
            }
        }

        private void ProcessesCommandAddAppToOption(
            bool verbose,
            string dir,
            string option,
            string name)
        {
            if (!PlugInConfigurationValidation.ValidateElementName(name))
            {
                Logger.Error(
                    "The application name is invalid '{0}'- " + PlugInConfigurationValidation.ValidCharactersMessage,
                    option);
                return;
            }

            bool result = UpdateOption(
                verbose,
                dir,
                option,
                (configurationOption) =>
                {
                    bool updateResult = configurationOption.AddApplication(name);
                    if (!updateResult)
                    {
                        Logger.Warning("Application {0} already uses option {1}", name, option);
                    }
                    return updateResult;
                });

            if (result)
            {
                Logger.Information($"Added application {name} to option {option}.");
            }
        }

        private void ProcessesCommandAddRuntimeToOption(
            bool verbose,
            string dir,
            string option,
            string name)
        {
            if (!PlugInConfigurationValidation.ValidateElementName(name))
            {
                Logger.Error(
                    "The runtime name is invalid '{0}'- " + PlugInConfigurationValidation.ValidCharactersMessage,
                    option);
                return;
            }

            bool result = UpdateOption(
                verbose,
                dir,
                option,
                (configurationOption) =>
                {
                    bool deleted = configurationOption.AddRuntime(name);
                    if (!deleted)
                    {
                        Logger.Warning("Runtime {0} already uses option {1}", name, option);
                    }

                    return deleted;
                });

            if (result)
            {
                Logger.Information($"Added runtime {name} to option {option}.");
            }
        }

        private void ProcessesCommandDelAppFromOption(
            bool verbose,
            string dir,
            string option,
            string name)
        {
            if (!PlugInConfigurationValidation.ValidateElementName(name))
            {
                Logger.Error(
                    "The application name is invalid '{0}'- " + PlugInConfigurationValidation.ValidCharactersMessage,
                    option);
                return;
            }

            bool result = UpdateOption(
                verbose,
                dir,
                option,
                (configurationOption) =>
                {
                    bool updateResult = configurationOption.DelApplication(name);
                    if (!updateResult)
                    {
                        Logger.Warning("Application {0} was not deleted from option {1}", name, option);
                    }
                    return updateResult;
                });

            if (result)
            {
                Logger.Information($"Deleted application {name} to option {option}.");
            }
        }

        private void ProcessesCommandDelRuntimeFromOption(
            bool verbose,
            string dir,
            string option,
            string name)
        {
            if (!PlugInConfigurationValidation.ValidateElementName(name))
            {
                Logger.Error(
                    "The runtime name is invalid '{0}'- " + PlugInConfigurationValidation.ValidCharactersMessage,
                    option);
                return;
            }

            bool result = UpdateOption(
                verbose,
                dir,
                option,
                (configurationOption) =>
                {
                    bool deleted = configurationOption.DelRuntime(name);
                    if (!deleted)
                    {
                        Logger.Warning("Runtime '{0}' was not removed from option {1}.", name, option);
                    }

                    return deleted;
                });

            if (result)
            {
                Logger.Information($"Deleted runtime {name} to option {option}.");
            }
        }

        private bool UpdateOption(
            bool verbose,
            string dir,
            string option,
            Func<ConfigurationOption, bool> updateOption)
        {
            try
            {
                if (verbose)
                {
                    Logger.EnableVerbose();
                }

                if (!PlugInConfigurationValidation.ValidateElementName(option))
                {
                    Logger.Error(
                        "The option name is invalid '{0}'- " + PlugInConfigurationValidation.ValidCharactersMessage,
                        option);
                    return false;
                }

                string file = GetFileName(dir);
                using var fileStream = File.Open(file, FileMode.Open, FileAccess.ReadWrite);

                var logger = new BridgeLogger();
                var existingConfiguration = PlugInConfigurationSerializer.ReadFromStream(fileStream, logger);
                if (existingConfiguration == null)
                {
                    return false;
                }

                var options = new HashSet<ConfigurationOption>(existingConfiguration.Options);
                var targetOption = options.FirstOrDefault(o => StringComparer.InvariantCulture.Equals(o.Name, option));
                if (targetOption == null)
                {
                    Logger.Error($"Option {option} was not found.");
                    return false;
                }

                if (!updateOption(targetOption))
                {
                    return false;
                }

                fileStream.Position = 0;

                var configuration = new PlugInConfiguration(
                    existingConfiguration.PlugInName,
                    existingConfiguration.Version,
                    options);

                PlugInConfigurationSerializer.WriteToStream(configuration, fileStream, logger);
            }
            catch (Exception e)
            {
                _parsingException = e;
            }

            return true;
        }

        private SemanticVersion ParseVersion(ArgumentResult result)
        {
            if (!result.Tokens.Any())
            {
                return new SemanticVersion(1, 0, 0);
            }

            if (result.Tokens.Count > 1)
            {
                throw new ConfigurationException(
                    string.Format("Failed to parse semantic version: {0}", result));
            }

            if (!SemanticVersion.TryParse(result.Tokens[0].Value, out var version))
            {
                throw new ConfigurationException(
                    string.Format("Failed to parse semantic version: {0}", result));
            }

            return version;
        }

        private ICommandHandler GetCommandHandler(string methodName)
        {
            var methodInfo = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(methodInfo != null);

            return CommandHandler.Create(methodInfo, this);
        }

        private string GetFileName(string directory, bool checkExists = true)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Directory.GetCurrentDirectory();
            }

            string file = Path.Combine(directory, PlugInConfigurationSerializer.DefaultFileName);

            if (checkExists && !File.Exists(file))
            {
                throw new ConfigurationException($"Configuration file {file} not found.");
            }

            return file;
        }
    }
}
