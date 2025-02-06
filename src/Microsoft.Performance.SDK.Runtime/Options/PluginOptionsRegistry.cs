// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     Represents a registry of <see cref="PluginOption"/> instances.
/// </summary>
public sealed class PluginOptionsRegistry
{
    private readonly object mutex = new();
    private readonly Dictionary<Guid, PluginOption> optionByGuid = new();
    private readonly ILogger logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginOptionsRegistry"/> class.
    /// </summary>
    /// <param name="logger">
    ///     The logger to use.
    /// </param>
    public PluginOptionsRegistry(ILogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    ///     Represents a class that can provide <see cref="PluginOption"/> instances to register to this class. Do not
    ///     implement this interface directly. Instead, use public methods on <see cref="PluginOptionsSystem"/> to register
    ///     options from supported sources.
    /// </summary>
    internal interface IProvider
    {
        /// <summary>
        ///     Gets the <see cref="PluginOption"/> instances to register.
        /// </summary>
        /// <returns>
        ///     The <see cref="PluginOption"/> instances to register.
        /// </returns>
        IEnumerable<PluginOption> GetOptions();
    }

    /// <summary>
    ///     Gets the <see cref="PluginOption"/> instances that are currently registered. The returned
    ///     collection is a snapshot of the current state of the registry and will not update as new
    ///     <see cref="PluginOption"/> instances are registered.
    /// </summary>
    public IReadOnlyCollection<PluginOption> Options
    {
        get
        {
            lock (this.mutex)
            {
                return this.optionByGuid.Values.ToList();
            }
        }
    }

    /// <summary>
    ///     Registers the <see cref="PluginOption"/> instances provided by the given <see cref="IProvider"/>.
    /// </summary>
    /// <param name="provider">
    ///     The <see cref="IProvider"/> that provides the <see cref="PluginOption"/> instances to register.
    /// </param>
    internal void RegisterFrom(IProvider provider)
    {
        lock (this.mutex)
        {
            this.logger.Info($"Registering plugin options from {provider.GetType().FullName}.");

            foreach (var option in provider.GetOptions())
            {
                this.optionByGuid[option.Guid] = option;
            }
        }
    }

    /// <summary>
    ///     Updates the state of the <see cref="PluginOption"/> instances in this registry from the given <see cref="PluginOptionsDto"/>. This
    ///     will have the following effects:
    ///     <list type="bullet">
    ///         <item>
    ///             Any registered <see cref="PluginOption"/> instances that have a <see cref="Guid"/> which corresponds to a <see cref="PluginOptionDto"/>
    ///             of the same type in the given <see cref="PluginOptionsDto"/> will either
    ///             <list type="bullet">
    ///                 <item>
    ///                     Have its <see cref="PluginOption{T}.CurrentValue"/> set to the DTO's serialized value if the DTO's <see cref="PluginOptionDto.IsDefault"/>
    ///                     is <c>false</c>.
    ///                 </item>
    ///                 <item>
    ///                     Be reset to the <see cref="PluginOption{T}"/>'s default state if the DTO's <see cref="PluginOptionDto.IsDefault"/>
    ///                     is <c>true</c>, disregarding the serialized value.
    ///                 </item>
    ///             </list>
    ///         </item>
    ///         <item>
    ///             Any registered <see cref="PluginOption"/> instances that do not have a corresponding <see cref="PluginOptionDto"/> in the given
    ///             <see cref="PluginOptionsDto"/> will be left unchanged.
    ///         </item>
    ///         <item>
    ///             Any serialized <see cref="PluginOptionDto"/> instances in the given <see cref="PluginOptionsDto"/> that do not have a corresponding
    ///             registered <see cref="PluginOption"/> instance will be ignored.
    ///         </item>
    ///     </list>
    /// </summary>
    /// <param name="dto">
    ///     The <see cref="PluginOptionsDto"/> from which to update the state of the <see cref="PluginOption"/> instances in this registry.
    /// </param>
    internal void UpdateFromDto(PluginOptionsDto dto)
    {
        lock (this.mutex)
        {
            this.logger.Verbose($"Updating {this.Options.Count} plugin options from DTO.");

            ApplyBooleanDtos(dto.BooleanOptions);
            ApplyFieldDtos(dto.FieldOptions);
            ApplyFieldArrayDtos(dto.FieldArrayOptions);
        }
    }

    private void ApplyFieldArrayDtos(IReadOnlyCollection<FieldArrayPluginOptionDto> dtoFieldArrayOptions)
    {
        ApplyT<FieldArrayOption, FieldArrayPluginOptionDto>(
            dtoFieldArrayOptions,
            (option, dto) =>
            {
                option.CurrentValue = dto.Value;
                this.logger.Verbose($"Plugin option {option} was updated to the saved value [{string.Join(", ", dto.Value)}].");
            });
    }

    private void ApplyFieldDtos(IReadOnlyCollection<FieldPluginOptionDto> dtoFieldOptions)
    {
        ApplyT<FieldOption, FieldPluginOptionDto>(
            dtoFieldOptions,
            (option, dto) =>
            {
                option.CurrentValue = dto.Value;
                this.logger.Verbose($"Plugin option {option} was updated to the saved value {dto.Value}.");
            });
    }

    private void ApplyBooleanDtos(IReadOnlyCollection<BooleanPluginOptionDto> dtoBooleanOptions)
    {
        ApplyT<BooleanOption, BooleanPluginOptionDto>(
            dtoBooleanOptions,
            (option, dto) =>
            {
                option.CurrentValue = dto.Value;
                this.logger.Verbose($"Plugin option {option} was updated to the saved value {dto.Value}.");
            });
    }

    private void ApplyT<T, TDTO>(
        IReadOnlyCollection<TDTO> dtoOptions,
        Action<T, TDTO> applySaved)
        where T : PluginOption
        where TDTO : PluginOptionDto
    {
        Debug.Assert(Monitor.IsEntered(this.mutex));

        foreach (var dto in dtoOptions)
        {
            if (this.optionByGuid.TryGetValue(dto.Guid, out var option) && option is T asT)
            {
                if (dto.IsDefault)
                {
                    asT.ApplyDefault();
                    this.logger.Verbose($"Option {asT} was reset to default.");
                }
                else
                {
                    this.logger.Verbose($"Updating option {asT} from a saved value.");
                    applySaved(asT, dto);
                }
            }
            else
            {
                this.logger.Verbose($"No option with GUID {dto.Guid} was found in the registry.");
            }
        }
    }
}
