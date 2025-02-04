// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization;

/// <summary>
///     A converter from a <see cref="PluginOptionsRegistry"/> to a <see cref="PluginOptionsDto"/>.
/// </summary>
public sealed class PluginOptionsRegistryToDtoConverter
{
    public PluginOptionsDto ConvertToDto(PluginOptionsRegistry registry)
    {
        List<BooleanPluginOptionDto> booleanOptions = new();
        List<FieldPluginOptionDto> fieldOptions = new();
        List<FieldArrayPluginOptionDto> fieldArrayOptions = new();

        var visitor = new Visitor(booleanOptions, fieldOptions, fieldArrayOptions);

        var switcher = new PluginOptionVisitorExecutor(visitor);

        switcher.Visit(registry.Options);

        return new PluginOptionsDto
        {
            BooleanOptions = booleanOptions,
            FieldOptions = fieldOptions,
            FieldArrayOptions = fieldArrayOptions,
        };
    }

    private class Visitor
        : IPluginOptionVisitor
    {
        private readonly List<BooleanPluginOptionDto> booleanOptions;
        private readonly List<FieldPluginOptionDto> fieldOptions;
        private readonly List<FieldArrayPluginOptionDto> fieldArrayOptions;

        public Visitor(
            List<BooleanPluginOptionDto> booleanOptions,
            List<FieldPluginOptionDto> fieldOptions,
            List<FieldArrayPluginOptionDto> fieldArrayOptions)
        {
            this.booleanOptions = booleanOptions;
            this.fieldOptions = fieldOptions;
            this.fieldArrayOptions = fieldArrayOptions;
        }

        public void Visit(BooleanOption option)
        {
            this.booleanOptions.Add(
                new BooleanPluginOptionDto
                {
                    Guid = option.Guid,
                    IsDefault = option.IsUsingDefault,
                    Value = option.CurrentValue,
                });
        }

        public void Visit(FieldOption option)
        {
            this.fieldOptions.Add(
                new FieldPluginOptionDto
                {
                    Guid = option.Guid,
                    IsDefault = option.IsUsingDefault,
                    Value = option.CurrentValue,
                });
        }

        public void Visit(FieldArrayOption option)
        {
            this.fieldArrayOptions.Add(
                new FieldArrayPluginOptionDto
                {
                    Guid = option.Guid,
                    IsDefault = option.IsUsingDefault,
                    Value = option.CurrentValue.ToArray(),
                });
        }
    }
}
