// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Options.Values;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;
using Microsoft.Performance.SDK.Runtime.Options.Visitors;

namespace Microsoft.Performance.Toolkit.Engine.Options;

internal class EnginePluginOptionValuesRegistry
{
    private readonly List<EnginePluginOptionValue> values = new();

    public void Add(EnginePluginOptionValue value)
    {
        this.values.Add(value);
    }

    public PluginOptionsDto ToDtoFor(PluginOptionsRegistry registry, ILogger logger)
    {
        var guidsInRegistry = registry.Options.Select(x => x.Guid).ToHashSet();

        var valuesNotInRegistry = this.values
            .Select(x => x.Guid)
            .Where(x => !guidsInRegistry.Contains(x))
            .ToList();

        foreach (var guid in valuesNotInRegistry)
        {
            logger.Warn(
                $"Plugin option value with GUID {guid} was not found in the registry. It will be ignored.");
        }

        // We use a visitor here instead of manually transforming each registered value so that when new
        // types are added and IPluginOptionVisitor is updated we get a build break. Otherwise, we might
        // forget to update the manual transformation in this class.
        var visitor = new Visitor(this.values);
        new IPluginOptionVisitor.Executor(visitor).Visit(registry.Options);

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
        private readonly List<EnginePluginOptionValue> values;

        public List<BooleanPluginOptionDto> BooleanOptions { get; } = new();

        public List<FieldPluginOptionDto> FieldOptions { get; } = new();

        public List<FieldArrayPluginOptionDto> FieldArrayOptions { get; } = new();

        public Visitor(List<EnginePluginOptionValue> values)
        {
            this.values = values;
        }

        public void Visit(BooleanOption option)
        {
            var optionValue = this.values.OfType<EnginePluginOptionValue<BooleanOptionValue, bool>>().FirstOrDefault(x => x.Guid == option.Guid);

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
            var optionValue = this.values.OfType<EnginePluginOptionValue<FieldOptionValue, string>>().FirstOrDefault(x => x.Guid == option.Guid);

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
            var optionValue = this.values.OfType<EnginePluginOptionValue<FieldArrayOptionValue, IReadOnlyList<string>>>().FirstOrDefault(x => x.Guid == option.Guid);

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
