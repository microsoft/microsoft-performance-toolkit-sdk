using System;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

public record BooleanPluginOptionDto(Guid Guid, bool IsDefault, bool Value) : PluginOptionDto(Guid, IsDefault);