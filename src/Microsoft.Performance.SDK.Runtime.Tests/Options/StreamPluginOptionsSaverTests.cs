// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Options;

/// <summary>
///     Contains tests for the <see cref="StreamPluginOptionsSaver"/>.
/// </summary>
[TestClass]
[UnitTest]
public class StreamPluginOptionsSaverTests
{
    /// <summary>
    ///     Asserts that the <see cref="Stream"/> used to save the <see cref="PluginOptionsDto"/> contains
    ///     serialized data that matches the <see cref="PluginOptionsDto"/> that was saved.
    /// </summary>
    [TestMethod]
    public async Task SavedDto_ContainsCorrectValues()
    {
        using MemoryStream stream = new MemoryStream();
        var sut = new TestPluginOptionsSaver(stream, false);

        var guid = Guid.NewGuid();
        const string value = "saved value";

        var success = await sut.TrySave(TestPluginOptionDto.PluginOptionsDto(TestPluginOptionDto.FieldOptionDto(guid, true, value)));

        Assert.IsTrue(success);

        stream.Position = 0;

        using StreamReader reader = new StreamReader(stream);
        string contents = await reader.ReadToEndAsync();

        Assert.IsTrue(contents.Contains(value, StringComparison.InvariantCulture));
        Assert.IsTrue(contents.Contains(guid.ToString(), StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    ///     Asserts that the <see cref="Stream"/> used to save the <see cref="PluginOptionsDto"/> is closed after writing
    ///     if <see cref="StreamPluginOptionsSaver"/> is configured to do so.
    /// </summary>
    [TestMethod]
    public async Task CloseStreamOnWrite_ClosesStream()
    {
        using MemoryStream stream = new MemoryStream();
        var sut = new TestPluginOptionsSaver(stream, true);

        var success = await sut.TrySave(TestPluginOptionDto.PluginOptionsDto());

        Assert.IsTrue(success);
        Assert.ThrowsException<ObjectDisposedException>(() => stream.Position);
        Assert.IsFalse(stream.CanRead);
        Assert.IsFalse(stream.CanSeek);
        Assert.IsFalse(stream.CanWrite);
    }

    /// <summary>
    ///     Asserts that the <see cref="Stream"/> used to save the <see cref="PluginOptionsDto"/> is not closed after writing
    ///     if <see cref="StreamPluginOptionsSaver"/> is not configured to do so.
    /// </summary>
    [TestMethod]
    public async Task DoNotCloseStreamOnWrite_DoesNotCloseStream()
    {
        using MemoryStream stream = new MemoryStream();
        var sut = new TestPluginOptionsSaver(stream, false);

        var success = await sut.TrySave(TestPluginOptionDto.PluginOptionsDto());

        Assert.IsTrue(success);
        stream.Position = 0;
        Assert.IsTrue(stream.CanRead);
        Assert.IsTrue(stream.CanSeek);
        Assert.IsTrue(stream.CanWrite);
    }

    private sealed class TestPluginOptionsSaver
        : StreamPluginOptionsSaver
    {
        private readonly Stream stream;

        public TestPluginOptionsSaver(MemoryStream stream, bool closeStreamOnWrite)
            : base(closeStreamOnWrite)
        {
            this.stream = stream;
        }

        protected override Stream GetStream()
        {
            return this.stream;
        }
    }
}