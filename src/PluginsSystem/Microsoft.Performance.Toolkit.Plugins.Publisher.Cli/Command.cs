using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Text;

namespace Microsoft.Performance.Toolkit.Plugins.Publisher.Cli
{
    public abstract class Command
    {
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public sealed class PositionalArgumentAttribute : Attribute
        {
            public int Index { get; }
            public bool Optional { get; set; } = false;

            public PositionalArgumentAttribute(int index)
            {
                this.Index = index;
            }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public sealed class ExtraArgumentAttribute : Attribute
        {
            public bool Optional { get; set; } = true;
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public sealed class ExpandPathAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public sealed class UseEnvironmentVariableAsDefaultAttribute : Attribute
        {
            public UseEnvironmentVariableAsDefaultAttribute(string environmentVariable)
            {
                this.EnvironmentVariable = environmentVariable;
            }

            public string EnvironmentVariable { get; }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
        public sealed class AlternateNameAttribute : Attribute
        {
            public string Name { get; }

            public AlternateNameAttribute(string name)
            {
                this.Name = name;
            }
        }

        public abstract class Argument
        {
            protected readonly PropertyInfo p;

            internal Argument(PropertyInfo p)
            {
                this.p = p;
            }

            public bool IsDeprecated => p.GetCustomAttribute<ObsoleteAttribute>() != null;
            public abstract bool Optional { get; }
            public string DisplayName => p.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? p.Name;
            public string Description => p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
            public object DefaultValue => p.GetCustomAttribute<DefaultValueAttribute>()?.Value;
            public bool ExpandPath => p.GetCustomAttribute<ExpandPathAttribute>() != null;
            public string EnvironmentVariable => p.GetCustomAttribute<UseEnvironmentVariableAsDefaultAttribute>()?.EnvironmentVariable;

            public abstract string GetUsage();

            public virtual string GetHelp()
            {
                return $"{this.DisplayName} - {this.Description}";
            }

            public bool TrySetValue(Command cmd, string value)
            {
                if (p.PropertyType == typeof(bool))
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        p.SetValue(cmd, true);
                        return true;
                    }
                    bool result;
                    if (bool.TryParse(value, out result))
                    {
                        p.SetValue(cmd, result);
                        return true;
                    }
                    Console.WriteLine($@"--{this.DisplayName} must be ""true"" or ""false"".");
                    return false;
                }

                if (p.PropertyType == typeof(string))
                {
                    if (this.ExpandPath)
                    {
                        string path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, value ?? string.Empty));
                        p.SetValue(cmd, path);
                    }
                    else
                    {
                        p.SetValue(cmd, value);
                    }
                    return true;
                }

                if (p.PropertyType == typeof(NetworkCredential))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        p.SetValue(cmd, null);
                        return true;
                    }

                    var parts = value.Split(new[] { ':' }, 2);
                    if (parts.Length != 2)
                    {
                        Console.WriteLine($"--{this.DisplayName} must be in the format \"«username»:«password»\" or \"api:«api-key»\".");
                        return false;
                    }

                    p.SetValue(cmd, new NetworkCredential(parts[0], parts[1]));
                    return true;
                }

                throw new ArgumentException(p.PropertyType.FullName);
            }
        }

        public sealed class PositionalArgument : Argument
        {
            internal PositionalArgument(PropertyInfo p) : base(p)
            {
            }

            public int Index => p.GetCustomAttribute<PositionalArgumentAttribute>().Index;
            public override bool Optional => p.GetCustomAttribute<PositionalArgumentAttribute>().Optional;

            public override string GetUsage()
            {
                var s = $"«{this.DisplayName}»";

                if (this.Optional)
                {
                    s = $"[{s}]";
                }

                return s;
            }
        }

        public sealed class ExtraArgument : Argument
        {
            internal ExtraArgument(PropertyInfo p) : base(p)
            {
            }

            public IEnumerable<string> AlternateNames => p.GetCustomAttributes<AlternateNameAttribute>().Select(a => a.Name);
            public override bool Optional => p.GetCustomAttribute<ExtraArgumentAttribute>().Optional;

            public override string GetUsage()
            {
                var s = $"--{this.DisplayName}=«{this.DisplayName}»";

                if (this.Optional)
                {
                    s = $"[{s}]";
                }

                if (p.PropertyType == typeof(bool) && !(this.DefaultValue as bool? ?? false) && this.Optional)
                {
                    s = $"[--{this.DisplayName}]";
                }

                return s;
            }
        }

        public string DisplayName => this.GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? this.GetType().Name;
        public string Description => this.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
        public IEnumerable<PositionalArgument> PositionalArguments => this.GetType().GetRuntimeProperties()
            .Where(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null)
            .Select(p => new PositionalArgument(p))
            .OrderBy(a => a.Index);

        public abstract Task<int> RunAsync(CancellationToken cancellationToken);

        public IEnumerable<ExtraArgument> ExtraArguments => this.GetType().GetRuntimeProperties()
            .Where(p => p.GetCustomAttribute<ExtraArgumentAttribute>() != null)
            .Select(p => new ExtraArgument(p));

        public string GetUsage()
        {
            var s = new StringBuilder("pluginTool ");

            s.Append(this.DisplayName);

            foreach (var arg in this.PositionalArguments)
            {
                if (!arg.IsDeprecated)
                    s.Append(' ').Append(arg.GetUsage());
            }

            foreach (var arg in this.ExtraArguments)
            {
                if (!arg.IsDeprecated)
                    s.Append(' ').Append(arg.GetUsage());
            }

            return s.ToString();
        }

        public string GetHelp()
        {
            var s = new StringBuilder("Usage: ");

            s.AppendLine(this.GetUsage()).AppendLine().AppendLine(this.Description);

            foreach (var arg in this.PositionalArguments)
            {
                if (!arg.IsDeprecated)
                    s.AppendLine().Append(arg.GetHelp());
            }

            foreach (var arg in this.ExtraArguments)
            {
                if (!arg.IsDeprecated)
                    s.AppendLine().Append(arg.GetHelp());
            }

            return s.ToString();
        }
    }
}
