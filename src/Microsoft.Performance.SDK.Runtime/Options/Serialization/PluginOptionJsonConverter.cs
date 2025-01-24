using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization;

public sealed class PluginOptionToOptionDtoConverter
{
    public PluginOptionsDto Convert(PluginOptionsRegistrar registrar)
    {
        List<BooleanPluginOptionDto> booleanOptions = new();
        List<FieldPluginOptionDto> fieldOptions = new();
        List<FieldArrayPluginOptionDto> fieldArrayOptions = new();

        var visitor = new Visitor(booleanOptions, fieldOptions, fieldArrayOptions, registrar);

        var switcher = new PluginOptionVisitorExecutor(visitor);

        switcher.Visit(registrar.Options);

        return new PluginOptionsDto(
            booleanOptions,
            fieldOptions,
            fieldArrayOptions);
    }

    private class Visitor
        : IPluginOptionVisitor
    {
        private readonly List<BooleanPluginOptionDto> booleanOptions;
        private readonly List<FieldPluginOptionDto> fieldOptions;
        private readonly List<FieldArrayPluginOptionDto> fieldArrayOptions;
        private readonly PluginOptionsRegistrar registrar;

        public Visitor(
            List<BooleanPluginOptionDto> booleanOptions,
            List<FieldPluginOptionDto> fieldOptions,
            List<FieldArrayPluginOptionDto> fieldArrayOptions,
            PluginOptionsRegistrar registrar)
        {
            this.booleanOptions = booleanOptions;
            this.fieldOptions = fieldOptions;
            this.fieldArrayOptions = fieldArrayOptions;
            this.registrar = registrar;
        }

        public void Visit(BooleanOption option)
        {
            bool isDefault = this.registrar.IsUsingDefault(option);
            this.booleanOptions.Add(new BooleanPluginOptionDto(option.Guid, isDefault, option.CurrentValue));
        }

        public void Visit(FieldOption option)
        {
            bool isDefault = this.registrar.IsUsingDefault(option);
            this.fieldOptions.Add(new FieldPluginOptionDto(option.Guid, isDefault, option.CurrentValue));
        }

        public void Visit(FieldArrayOption option)
        {
            bool isDefault = this.registrar.IsUsingDefault(option);
            this.fieldArrayOptions.Add(new FieldArrayPluginOptionDto(option.Guid, isDefault, option.CurrentValue.ToArray()));
        }
    }
}
