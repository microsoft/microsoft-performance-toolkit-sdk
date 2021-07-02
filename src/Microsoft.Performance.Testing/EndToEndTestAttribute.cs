// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Testing
{
    /// <summary>
    ///     Marks a test as being an automated end to end test. This gives
    ///     the test method the "EndToEndTest" trait.
    /// </summary>
    public class EndToEndTestAttribute
        : TestCategoryBaseAttribute
    {
        private static readonly ReadOnlyCollection<string> testCategories =
            new ReadOnlyCollection<string>(
                new[]
                {
                    "EndToEndTest",
                });

        /// <summary>
        ///     Initializes a new instance of the <see cref="EndToEndTestAttribute" />
        ///     class.
        /// </summary>
        public EndToEndTestAttribute()
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
