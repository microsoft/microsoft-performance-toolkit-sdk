// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Testing
{
    /// <summary>
    ///     Marks a test as being a unit test. This gives
    ///     the test method the "UnitTest" trait.
    /// </summary>
    public class UnitTestAttribute
        : TestCategoryBaseAttribute
    {
        private static readonly ReadOnlyCollection<string> testCategories =
            new ReadOnlyCollection<string>(
                new[]
                {
                    "UnitTest",
                });

        public UnitTestAttribute()
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
