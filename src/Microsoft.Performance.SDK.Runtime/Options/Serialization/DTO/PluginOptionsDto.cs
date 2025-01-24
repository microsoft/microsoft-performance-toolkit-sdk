using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

public record PluginOptionsDto(
    IReadOnlyCollection<BooleanPluginOptionDto> BooleanOptions,
    IReadOnlyCollection<FieldPluginOptionDto> FieldOptions,
    IReadOnlyCollection<FieldArrayPluginOptionDto> FieldArrayOptions);