// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Testing.SDK
{
    public sealed class FakeSerializer
        : ISerializer
    {
        public IEnumerable<TableConfigurations> DeserializeTableConfigurations(Stream stream)
        {
            return Enumerable.Empty<TableConfigurations>();
        }

        public IEnumerable<TableConfigurations> DeserializeTableConfigurations(Stream stream, ILogger logger)
        {
            return Enumerable.Empty<TableConfigurations>();
        }
    }
}
