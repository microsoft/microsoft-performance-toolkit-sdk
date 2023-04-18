// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Reflection;

namespace Microsoft.Performance.Toolkit.Plugins.Publisher.Cli
{
    public sealed class CommandDispatcher
    {
        public static CommandDispatcher Default
        {
            get
            {
                return new(typeof(Pack), typeof(GenerateMetadata));
            }
        }

        private readonly IEnumerable<Type> commands;

        public CommandDispatcher(params Type[] commands)
        {
            this.commands = commands;
        }

        public void Main(string[] args)
        {
            bool onlyPositional = false;
            bool hadError = false;

            var positional = new List<string>();
            var extra = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string arg in args)
            {
                if (onlyPositional || !arg.StartsWith("--"))
                {
                    positional.Add(arg);
                }
                else if (arg == "--")
                {
                    onlyPositional = true;
                    continue;
                }
                else
                {
                    var parts = arg.Substring("--".Length).Split(new[] { '=' }, 2);
                    if (extra.ContainsKey(parts[0]))
                    {
                        hadError = true;
                    }

                    extra[parts[0]] = parts.Length == 1 ? null : parts[1];
                }
            }

            if (positional.Count > 0 && string.Equals("help", positional[0], StringComparison.OrdinalIgnoreCase))
            {
                hadError = true;
                positional.RemoveAt(0);
            }

            Command? cmd = null;
            if (positional.Count == 0)
            {
                hadError = true;
            }
            else
            {
                foreach (var command in commands)
                {
                    cmd = (Command)Activator.CreateInstance(command);
                    if (!string.Equals(cmd.DisplayName, positional[0], StringComparison.OrdinalIgnoreCase))
                    {
                        cmd = null;
                        continue;
                    }

                    if (hadError)
                    {
                        break;
                    }

                    positional.RemoveAt(0);

                    foreach (var arg in cmd.PositionalArguments)
                    {
                        if (arg.Index < positional.Count)
                        {
                            if (!arg.TrySetValue(cmd, positional[arg.Index]))
                            {
                                hadError = true;
                            }
                        }
                        else if (arg.EnvironmentVariable != null)
                        {
                            var value = Environment.GetEnvironmentVariable(arg.EnvironmentVariable, EnvironmentVariableTarget.Process) ?? Environment.GetEnvironmentVariable(arg.EnvironmentVariable, EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable(arg.EnvironmentVariable, EnvironmentVariableTarget.Machine);
                            if (value == null && !arg.Optional)
                                hadError = true;

                            if (value != null)
                            {
                                if (!arg.TrySetValue(cmd, value))
                                    hadError = true;
                            }
                        }
                        else if (!arg.Optional)
                        {
                            hadError = true;
                        }
                    }

                    if (positional.Count > cmd.PositionalArguments.Count())
                    {
                        hadError = true;
                    }

                    foreach (var arg in cmd.ExtraArguments)
                    {
                        var alt = arg.AlternateNames.FirstOrDefault(extra.ContainsKey);
                        if (extra.ContainsKey(arg.DisplayName) || alt != null)
                        {
                            if (!arg.TrySetValue(cmd, extra[alt ?? arg.DisplayName]))
                            {
                                hadError = true;
                            }
                            extra.Remove(alt ?? arg.DisplayName);
                        }
                        else if (arg.EnvironmentVariable != null)
                        {
                            var value = Environment.GetEnvironmentVariable(arg.EnvironmentVariable, EnvironmentVariableTarget.Process) ?? Environment.GetEnvironmentVariable(arg.EnvironmentVariable, EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable(arg.EnvironmentVariable, EnvironmentVariableTarget.Machine);
                            if (value == null && !arg.Optional)
                                hadError = true;

                            if (value != null)
                            {
                                if (!arg.TrySetValue(cmd, value))
                                    hadError = true;
                            }
                        }
                        else if (!arg.Optional)
                        {
                            hadError = true;
                        }
                    }

                    if (extra.Count != 0)
                    {
                        hadError = true;
                    }

                    break;
                }
            }

            if (hadError || cmd == null)
            {
                if (cmd != null)
                {
                    ShowHelp(cmd);
                }
                else
                {
                    ShowGenericHelp();
                }
                Environment.ExitCode = 2;
            }
            else
            {
                using (var consoleCancelTokenSource = new CancellationTokenSource())
                {
                    Console.CancelKeyPress +=
                        (s, e) =>
                        {
                            consoleCancelTokenSource.Cancel();
                        };

                    try
                    {
                        try
                        {
                            Environment.ExitCode = cmd.RunAsync(consoleCancelTokenSource.Token).GetAwaiter().GetResult();
                        }
                        catch (AggregateException ex)
                        {
                            throw ex.InnerException;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        Console.Error.WriteLine("Operation was canceled by the user.");
                        Environment.ExitCode = 3;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                        Environment.ExitCode = 1;
                    }
                }
            }
        }

        public void ShowGenericHelp()
        {
            Console.Error.WriteLine($"pluginCli {typeof(CommandDispatcher).Assembly.GetName().Version}");
            Console.Error.WriteLine("Usage: pluginCli «command»");
            Console.Error.WriteLine();

            foreach (var command in commands)
            {
                Console.Error.WriteLine($"{command.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? command.Name} - {command.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty}");
            }
        }

        public void ShowHelp(Command cmd)
        {
            Console.Error.WriteLine(cmd.GetHelp());
        }
    }
}
