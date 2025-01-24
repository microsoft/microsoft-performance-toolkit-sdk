using System;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

public record FieldPluginOptionDto(Guid Guid, bool IsDefault, string Value) : PluginOptionDto(Guid, IsDefault);