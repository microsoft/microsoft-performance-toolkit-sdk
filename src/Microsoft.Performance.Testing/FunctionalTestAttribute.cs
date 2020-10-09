// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Testing
{
    /// <summary>
    ///     Marks a test as being a functional test. This gives
    ///     the test method the "FunctionalTest" trait.
    /// </summary>
    public class FunctionalTestAttribute
        : TestCategoryBaseAttribute
    {
        private static readonly ReadOnlyCollection<string> testCategories =
            new ReadOnlyCollection<string>(
                new[]
                {
                    "FunctionalTest",
                });

        public FunctionalTestAttribute()
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
