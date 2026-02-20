// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests;

[TestClass]
public class StringWithCaseInsensitiveComparisonTests
{
    [TestMethod]
    [DataRow("Alphabet", "alphabet", 0, DisplayName = "Equal ignoring casing")]
    [DataRow("Bravo", "apple", 1, DisplayName = "Comparison ignoring casing")]
    public void CompareTo_ProvidesCaseInsensitiveOrdering(string first, string second, int expectedSign)
    {
        var left = new StringWithCaseInsensitiveComparison(first);
        var right = new StringWithCaseInsensitiveComparison(second);

        Assert.AreEqual(expectedSign, left.CompareTo(right));
        Assert.AreEqual(-expectedSign, right.CompareTo(left));
    }

    [TestMethod]
    public void DefaultValue_CompareToBehavesConsistently()
    {
        var defaultValue = default(StringWithCaseInsensitiveComparison);
        var other = new StringWithCaseInsensitiveComparison("apple");

        Assert.AreEqual(0, defaultValue.CompareTo(default(StringWithCaseInsensitiveComparison)));
        Assert.IsTrue(defaultValue < other);
        Assert.IsTrue(other > defaultValue);
    }
}
