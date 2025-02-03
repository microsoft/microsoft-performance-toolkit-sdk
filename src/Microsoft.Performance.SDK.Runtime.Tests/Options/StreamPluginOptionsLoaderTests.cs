// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Options;

/// <summary>
///     Tests for the <see cref="StreamPluginOptionsLoader"/>.
/// </summary>
[TestClass]
[UnitTest]
public class StreamPluginOptionsLoaderTests
{
    /// <summary>
    ///     Asserts that the loaded <see cref="PluginOptionDto"/> contains data that was present in the
    ///     <see cref="Stream"/>.
    /// </summary>
    [TestMethod]
    public async Task LoadedDto_ContainsCorrectValues()
    {
        var guid = Guid.NewGuid();
        const string value = "saved value";
        var optionsDtoToSave = TestPluginOptionDto.PluginOptionsDto(TestPluginOptionDto.FieldOptionDto(guid, false, value));

        using MemoryStream stream = new MemoryStream();


        await JsonSerializer.SerializeAsync(stream, optionsDtoToSave);
        stream.Position = 0;

        var sut = new TestStreamPluginOptionsLoader(stream, false);


        var loaded = await sut.TryLoadAsync();

        Assert.IsNotNull(loaded);
        Assert.AreEqual(0, loaded.BooleanOptions.Count);
        Assert.AreEqual(0, loaded.FieldArrayOptions.Count);
        Assert.AreEqual(1, loaded.FieldOptions.Count);
        Assert.AreEqual(guid, loaded.FieldOptions.First().Guid);
        Assert.AreEqual(value, loaded.FieldOptions.First().Value);
        Assert.IsFalse(loaded.FieldOptions.First().IsDefault);
    }

    /// <summary>
    ///     Asserts that the <see cref="Stream"/> used to load the <see cref="PluginOptionsDto"/> is closed after loading
    ///     if <see cref="StreamPluginOptionsLoader"/> is configured to do so.
    /// </summary>
    [TestMethod]
    public async Task CloseStreamOnWrite_ClosesStream()
    {
        using MemoryStream stream = new MemoryStream();
        var sut = new TestStreamPluginOptionsLoader(stream, true);

        await sut.TryLoadAsync();

        Assert.ThrowsException<ObjectDisposedException>(() => stream.Position);
        Assert.IsFalse(stream.CanRead);
        Assert.IsFalse(stream.CanSeek);
        Assert.IsFalse(stream.CanWrite);
    }

    /// <summary>
    ///     Asserts that the <see cref="Stream"/> used to load the <see cref="PluginOptionsDto"/> is not closed after loading
    ///     if <see cref="StreamPluginOptionsLoader"/> is not configured to do so.
    /// </summary>
    [TestMethod]
    public async Task DoNotCloseStreamOnWrite_DoesNotCloseStream()
    {
        using MemoryStream stream = new MemoryStream();
        var sut = new TestStreamPluginOptionsLoader(stream, false);

        await sut.TryLoadAsync();

        stream.Position = 0;
        Assert.IsTrue(stream.CanRead);
        Assert.IsTrue(stream.CanSeek);
        Assert.IsTrue(stream.CanWrite);
    }

    private sealed class TestStreamPluginOptionsLoader
        : StreamPluginOptionsLoader
    {
        private readonly Stream stream;

        public TestStreamPluginOptionsLoader(MemoryStream stream, bool closeStreamOnWrite)
            : base(closeStreamOnWrite, Logger.Null)
        {
            this.stream = stream;
        }

        protected override Stream GetStream()
        {
            return this.stream;
        }
    }
}
