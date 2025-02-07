// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

/// <summary>
///     A DTO for serializing and deserializing plugin options.
/// </summary>
public sealed class PluginOptionsDto
{
    /// <summary>
    ///     Gets or initializes the <see cref="BooleanPluginOptionDto"/> instances.
    /// </summary>
    public IReadOnlyCollection<BooleanPluginOptionDto> BooleanOptions { get; init; } = new List<BooleanPluginOptionDto>();

    /// <summary>
    ///     Gets or initializes the <see cref="FieldPluginOptionDto"/> instances.
    /// </summary>
    public IReadOnlyCollection<FieldPluginOptionDto> FieldOptions { get; init; } = new List<FieldPluginOptionDto>();

    /// <summary>
    ///     Gets or initializes the <see cref="FieldArrayPluginOptionDto"/> instances.
    /// </summary>
    public IReadOnlyCollection<FieldArrayPluginOptionDto> FieldArrayOptions { get; init; } = new List<FieldArrayPluginOptionDto>();

    public bool IsEmpty()
    {
        return !this.BooleanOptions.Any() &&
               !this.FieldOptions.Any() &&
               !this.FieldArrayOptions.Any();
    }

    /// <summary>
    ///     Merges the values from the given <see cref="PluginOptionsDto"/> into this one, returning the new instance.
    ///     DTOs in <paramref name="newDto"/> that have the same <see cref="PluginOptionDto.Guid"/> as a DTO in this
    ///     instance will replace the old DTO. DTOs in this instance which are not present in in <paramref name="newDto"/>
    ///     will be included in the returned instance.
    /// </summary>
    /// <param name="newDto">
    ///     The DTO to merge into this one.
    /// </param>
    /// <returns>
    ///     A new <see cref="PluginOptionsDto"/> instance with the merged values.
    /// </returns>
    public PluginOptionsDto UpdateTo(PluginOptionsDto newDto)
    {
        return new PluginOptionsDto
        {
            BooleanOptions = WithoutUpdatedValues(this.BooleanOptions, newDto.BooleanOptions).Concat(newDto.BooleanOptions).ToList(),
            FieldOptions = WithoutUpdatedValues(this.FieldOptions, newDto.FieldOptions).Concat(newDto.FieldOptions).ToList(),
            FieldArrayOptions = WithoutUpdatedValues(this.FieldArrayOptions, newDto.FieldArrayOptions).Concat(newDto.FieldArrayOptions).ToList(),
        };
    }

    private IEnumerable<T> WithoutUpdatedValues<T>(IEnumerable<T> oldDtos, IEnumerable<T> newDtos)
        where T : PluginOptionDto
    {
        HashSet<Guid> newGuids = new(newDtos.Select(dto => dto.Guid));
        return oldDtos.Where(oldDto => !newGuids.Contains(oldDto.Guid));
    }
}
