// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.SDK.Runtime.Options;

public interface IPluginOptionVisitor
{
    void Visit(BooleanOption option);

    void Visit(FieldOption option);

    void Visit(FieldArrayOption option);
}