// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    public class DefaultProcessingMode
        : IProcessingMode
    {
        public static readonly Guid Id = new Guid("F97C3411-5DF5-4633-9EFD-470BEEBF40EF");

        /// <inheritdoc />
        public Guid Guid => Id;

        /// <inheritdoc />
        public string Name => "Default";

        /// <inheritdoc />
        public string Description => "A non-special method of processing data sources.";
    }
}