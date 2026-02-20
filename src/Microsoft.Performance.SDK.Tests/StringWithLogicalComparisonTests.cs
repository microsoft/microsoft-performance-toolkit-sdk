// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests;

[TestClass]
public class StringWithLogicalComparisonTests
{
    [TestMethod]
    [DataRow("item20", "item1", 1, DisplayName = "Numeric segments compared by value")]
    [DataRow("apple", "0x10", 1, DisplayName = "Hex less than with non-hex")]
    [DataRow("0x1f", "0x0a", 1, DisplayName = "Hex comparison")]
    [DataRow("0xB0", "0xaB", 1, DisplayName = "Hex comparison ignoring cases")]
    [DataRow("0xa0", "0xA0", 1, DisplayName = "Hex compare cases when numerically equal")]
    [DataRow("00000000-0000-0000-ABCD-000000000001", "00000000-0000-0000-0000-ABCDEF000001", 1, DisplayName = "GUID comparison")]
    [DataRow("Apple", "F0000000-0000-0000-0000-000000000000", 1, DisplayName = "GUID less than non-GUID")]
    [DataRow("-1", "-2", 1, DisplayName = "Negative number comparison")]
    [DataRow("0", "-2", 1, DisplayName = "Negative number compared with non-negative")]
    [DataRow("-0xa", "-0xf", 1, DisplayName = "Negative hex comparison")]
    [DataRow("-0xa", "0xa", 1, DisplayName = "Negative hex compared with non-negative hex")]
    public void CompareTo_FollowsLogicalOrdering(string first, string second, int expectedSign)
    {
        var left = new StringWithLogicalComparison(first);
        var right = new StringWithLogicalComparison(second);

        Assert.AreEqual(expectedSign, left.CompareTo(right));
        Assert.AreEqual(-expectedSign, right.CompareTo(left));
    }

    [TestMethod]
    public void DefaultValue_CompareToBehavesConsistently()
    {
        var defaultValue = default(StringWithLogicalComparison);
        var other = new StringWithLogicalComparison("apple");

        Assert.AreEqual(0, defaultValue.CompareTo(default(StringWithLogicalComparison)));
        Assert.IsTrue(defaultValue < other);
        Assert.IsTrue(other > defaultValue);
    }
}
