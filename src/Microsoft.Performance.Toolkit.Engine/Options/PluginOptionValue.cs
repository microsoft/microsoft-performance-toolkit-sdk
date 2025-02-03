using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.Toolkit.Engine.Options
{
    internal record PluginOptionValue(Guid Guid);

    internal record PluginOptionValue<T, TValue>(Guid Guid, TValue Value) : PluginOptionValue(Guid)
        where T : PluginOption<TValue>;

    internal class PluginOptionValuesRegistry
    {
        private readonly List<PluginOptionValue> values = new();

        public void Add(PluginOptionValue value)
        {
            this.values.Add(value);
        }

        public PluginOptionsDto ToDtoFor(PluginOptionsRegistry registry, ILogger logger)
        {
            var allGuids = registry.Options.Select(x => x.Guid).ToHashSet();

            var missingGuids = this.values
                .Select(x => x.Guid)
                .Where(x => !allGuids.Contains(x))
                .ToList();

            foreach (var missingGuid in missingGuids)
            {
                logger.Warn(
                    $"Plugin option value with GUID {missingGuid} was not found in the registry. It will be ignored.");
            }

            var visitor = new Visitor(this.values);
            var switcher = new PluginOptionVisitorExecutor(visitor);
            switcher.Visit(registry.Options);

            return new PluginOptionsDto()
            {
                BooleanOptions = visitor.BooleanOptions,
                FieldOptions = visitor.FieldOptions,
                FieldArrayOptions = visitor.FieldArrayOptions,
            };
        }

        private class Visitor
            : IPluginOptionVisitor
        {
            private readonly List<PluginOptionValue> values;

            public List<BooleanPluginOptionDto> BooleanOptions { get; } = new();

            public List<FieldPluginOptionDto> FieldOptions { get; } = new();

            public List<FieldArrayPluginOptionDto> FieldArrayOptions { get; } = new();

            public Visitor(List<PluginOptionValue> values)
            {
                this.values = values;
            }

            public void Visit(BooleanOption option)
            {
                var optionValue = this.values.OfType<PluginOptionValue<BooleanOption, bool>>().FirstOrDefault(x => x.Guid == option.Guid);

                if (optionValue != null)
                {
                    this.BooleanOptions.Add(new BooleanPluginOptionDto()
                    {
                        Guid = optionValue.Guid,
                        IsDefault = false,
                        Value = optionValue.Value,
                    });
                }
            }

            public void Visit(FieldOption option)
            {
                var optionValue = this.values.OfType<PluginOptionValue<FieldOption, string>>().FirstOrDefault(x => x.Guid == option.Guid);

                if (optionValue != null)
                {
                    this.FieldOptions.Add(new FieldPluginOptionDto()
                    {
                        Guid = optionValue.Guid,
                        IsDefault = false,
                        Value = optionValue.Value,
                    });
                }
            }

            public void Visit(FieldArrayOption option)
            {
                var optionValue = this.values.OfType<PluginOptionValue<FieldArrayOption, IReadOnlyList<string>>>().FirstOrDefault(x => x.Guid == option.Guid);

                if (optionValue != null)
                {
                    this.FieldArrayOptions.Add(new FieldArrayPluginOptionDto()
                    {
                        Guid = optionValue.Guid,
                        IsDefault = false,
                        Value = optionValue.Value.ToArray(),
                    });
                }
            }
        }
    }
}
