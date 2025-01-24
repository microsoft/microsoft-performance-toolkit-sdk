using System;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

public abstract record PluginOptionDto(Guid Guid, bool IsDefault);