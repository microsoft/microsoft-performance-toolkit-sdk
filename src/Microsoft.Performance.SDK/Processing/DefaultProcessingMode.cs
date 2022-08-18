// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    public class DefaultProcessingMode
        : IProcessingMode
    {
        private static readonly Guid guid = new Guid("F97C3411-5DF5-4633-9EFD-470BEEBF40EF");

        public Guid Guid => DefaultProcessingMode.guid;

        /// <inheritdoc />
        public string Name => "Default";

        /// <inheritdoc />
        public string Description => "A non-special method of processing data sources.";

        /// <inheritdoc />
        public bool SupportsGroupCombining => true;
    }
}