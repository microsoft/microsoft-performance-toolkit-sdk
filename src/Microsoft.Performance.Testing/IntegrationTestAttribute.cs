// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Testing
{
    /// <summary>
    ///     Marks a test as being a integration test. This gives
    ///     the test method the "IntegrationTest" trait.
    /// </summary>
    public class IntegrationTestAttribute
        : TestCategoryBaseAttribute
    {
        private static readonly ReadOnlyCollection<string> testCategories =
            new ReadOnlyCollection<string>(
                new[]
                {
                    "IntegrationTest",
                });

        public IntegrationTestAttribute()
        {
        }

        /// <inheritdoc />
        public override IList<string> TestCategories
        {
            get
            {
                return testCategories;
            }
        }
    }
}
