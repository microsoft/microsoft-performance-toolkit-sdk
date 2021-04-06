// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Testing
{
    /// <summary>
    ///     Marks a test as being an automated UI test. This gives
    ///     the test method the "UITest" trait.
    /// </summary>
    public class UITestAttribute
        : TestCategoryBaseAttribute
    {
        private static readonly ReadOnlyCollection<string> testCategories =
            new ReadOnlyCollection<string>(
                new[]
                {
                "UITest",
                });

        public UITestAttribute()
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
