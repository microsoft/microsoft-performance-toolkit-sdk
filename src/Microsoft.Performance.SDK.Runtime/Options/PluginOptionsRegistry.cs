// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options;

public class PluginOptionsRegistry
{
    private readonly object mutex = new();
    private readonly Dictionary<Guid, PluginOption> optionByGuid = new();

    public IReadOnlyCollection<PluginOption> Options
    {
        get
        {
            lock (mutex)
            {
                return optionByGuid.Values.ToList();
            }
        }
    }

    public void Register(params PluginOption[] options)
    {
        lock (mutex)
        {
            foreach (var option in options)
            {
                optionByGuid[option.Guid] = option;
            }
        }
    }

    public void Unregister(PluginOption option)
    {
        lock (mutex)
        {
            optionByGuid.Remove(option.Guid);
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
            (option, dto) => { option.CurrentValue = dto.Value; });
    }

    private void ApplyFieldDtos(IReadOnlyCollection<FieldPluginOptionDto> dtoFieldOptions)
    {
        ApplyT<FieldOption, FieldPluginOptionDto>(
            dtoFieldOptions,
            (option, dto) => { option.CurrentValue = dto.Value; });
    }

    private void ApplyBooleanDtos(IReadOnlyCollection<BooleanPluginOptionDto> dtoBooleanOptions)
    {
        ApplyT<BooleanOption, BooleanPluginOptionDto>(
            dtoBooleanOptions,
            (option, dto) => { option.CurrentValue = dto.Value; });
    }

    private void ApplyT<T, TDTO>(
        IReadOnlyCollection<TDTO> dtoOptions,
        Action<T, TDTO> applySaved)
        where T : PluginOption
        where TDTO : PluginOptionDto
    {
        foreach (var dto in dtoOptions)
            lock (mutex)
            {
                if (optionByGuid.TryGetValue(dto.Guid, out var option) && option is T asT)
                {
                    if (dto.IsDefault)
                        asT.ApplyDefault();
                    else
                        applySaved(asT, dto);
                }
            }
    }
}