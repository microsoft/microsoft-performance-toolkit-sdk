using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options;

public class PluginOptionsRegistrar
{
    private record PluginOptionInfo(PluginOption Option, bool IsUsingDefault);
    
    private readonly object mutex = new();
    private readonly Dictionary<Guid, PluginOptionInfo> optionByGuid = new();

    public IReadOnlyCollection<PluginOption> Options
    {
        get
        {
            lock (this.mutex)
            {
                return this.optionByGuid.Values.Select(x => x.Option).ToList();
            }
        }
    }

    public void Register(PluginOption option, bool isUsingDefault)
    {
        lock (this.mutex)
        {
            this.optionByGuid[option.Guid] = new PluginOptionInfo(option, isUsingDefault);

            option.OnChangedFromDefault += OnOptionChangedFromDefault;
            option.OnChangedToDefault += OnOptionChangedToDefault;
        }
    }

    public void Unregister(PluginOption option)
    {
        lock (this.mutex)
        {
            this.optionByGuid.Remove(option.Guid);

            option.OnChangedFromDefault -= OnOptionChangedFromDefault;
            option.OnChangedToDefault -= OnOptionChangedToDefault;   
        }
    }

    private void OnOptionChangedToDefault(object sender, EventArgs e)
    {
        OnDefaultChanged(sender, true);
    }

    private void OnOptionChangedFromDefault(object sender, EventArgs e)
    {
        OnDefaultChanged(sender, false);
    }
    
    private void OnDefaultChanged(object sender, bool isDefault)
    {
        if (sender is not PluginOption option)
        {
            return;
        }
       
        lock (this.mutex)
        {
            this.optionByGuid[option.Guid] = new PluginOptionInfo(option, isDefault);
        }

    }

    public void RegisterAsDefault(params PluginOption[] options)
    {
        foreach (var option in options)
        {
            Register(option, true);
        }
    }

    public bool IsUsingDefault(PluginOption option)
    {
        lock (this.mutex)
        {
            return this.optionByGuid.TryGetValue(option.Guid, out var info) && info.IsUsingDefault;
        }
    }

    public void UpdateFromDto(PluginOptionsDto dto)
    {
        ApplyBooleanDtos(dto.BooleanOptions);
        ApplyFieldDtos(dto.FieldOptions);
        ApplyFieldArrayDtos(dto.FieldArrayOptions);
    }

    private void ApplyFieldArrayDtos(IReadOnlyCollection<FieldArrayPluginOptionDto> dtoFieldArrayOptions)
    {
        ApplyT<FieldArrayOption, FieldArrayPluginOptionDto>(
            dtoFieldArrayOptions,
            (option, dto) =>
            {
                option.CurrentValue = dto.Value;
            });
    }

    private void ApplyFieldDtos(IReadOnlyCollection<FieldPluginOptionDto> dtoFieldOptions)
    {
        ApplyT<FieldOption, FieldPluginOptionDto>(
            dtoFieldOptions,
            (option, dto) =>
            {
                option.CurrentValue = dto.Value;
            });
    }

    private void ApplyBooleanDtos(IReadOnlyCollection<BooleanPluginOptionDto> dtoBooleanOptions)
    {
        ApplyT<BooleanOption, BooleanPluginOptionDto>(
            dtoBooleanOptions,
            (option, dto) =>
            {
                option.CurrentValue = dto.Value;
            });
    }

    private void ApplyT<T, TDTO>(
        IReadOnlyCollection<TDTO> dtoOptions,
        Action<T, TDTO> applySaved)
        where T : PluginOption
        where TDTO : PluginOptionDto
    {
        foreach (var dto in dtoOptions)
        {
            lock (this.mutex)
            {
                if (this.optionByGuid.TryGetValue(dto.Guid, out PluginOptionInfo info) && info.Option is T asT)
                {
                    if (dto.IsDefault)
                    {
                        asT.ApplyDefault();
                    }
                    else
                    {
                        applySaved(asT, dto);
                    }
                }
            }
        }
    }
}