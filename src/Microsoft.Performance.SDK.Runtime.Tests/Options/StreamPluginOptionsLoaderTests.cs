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
///     Tests for the <see cref="StreamPluginOptionsLoader{T}"/>.
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
    ///     if <see cref="StreamPluginOptionsLoader{T}"/> is configured to do so.
    /// </summary>
    [TestMethod]
    public async Task CloseStreamOnRead_ClosesStream()
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
    ///     if <see cref="StreamPluginOptionsLoader{T}"/> is not configured to do so.
    /// </summary>
    [TestMethod]
    public async Task DoNotCloseStreamOnRead_DoesNotCloseStream()
    {
        using MemoryStream stream = new MemoryStream();
        var sut = new TestStreamPluginOptionsLoader(stream, false);

        await sut.TryLoadAsync();

        stream.Position = 0;
        Assert.IsTrue(stream.CanRead);
        Assert.IsTrue(stream.CanSeek);
        Assert.IsTrue(stream.CanWrite);
    }

    /// <summary>
    ///     Asserts that an empty <see cref="Stream"/> returns an empty <see cref="PluginOptionsDto"/>.
    /// </summary>
    [TestMethod]
    public async Task EmptyStream_ReturnsEmptyDto()
    {
        using MemoryStream stream = new MemoryStream();
        var sut = new TestStreamPluginOptionsLoader(stream, false);

        var loadedDto = await sut.TryLoadAsync();

        Assert.IsNotNull(loadedDto);
        Assert.IsTrue(loadedDto.IsEmpty());
    }

    /// <summary>
    ///     Asserts that <see cref="StreamPluginOptionsLoader{T}.TryLoadAsync"/> returns <c>null</c> if an exception is thrown
    ///     when getting the <see cref="Stream"/> to load from.
    /// </summary>
    [TestMethod]
    public async Task GetStreamThrows_ReturnsNull()
    {
        var sut = new ThrowingStreamPluginOptionsLoader(true, false);

        var loadedDto = await sut.TryLoadAsync();

        Assert.IsNull(loadedDto);
    }

    /// <summary>
    ///     Asserts that <see cref="StreamPluginOptionsLoader{T}.TryLoadAsync"/> returns <c>null</c> if an exception is thrown
    ///     when checking if the <see cref="Stream"/> to load from has content.
    /// </summary>
    [TestMethod]
    public async Task HasContentThrows_ReturnsNull()
    {
        using MemoryStream stream = new MemoryStream();
        var sut = new ThrowingStreamPluginOptionsLoader(false, true);

        var loadedDto = await sut.TryLoadAsync();

        Assert.IsNull(loadedDto);
    }

    private sealed class ThrowingStreamPluginOptionsLoader
        : StreamPluginOptionsLoader<MemoryStream>
    {
        private readonly bool throwInGetStream;
        private readonly bool throwInHasContent;
        public ThrowingStreamPluginOptionsLoader(bool throwInGetStream, bool throwInHasContent)
            : base(true, Logger.Null)
        {
            this.throwInGetStream = throwInGetStream;
            this.throwInHasContent = throwInHasContent;
        }

        protected override MemoryStream GetStream()
        {
            if (this.throwInGetStream)
            {
                throw new InvalidOperationException();
            }

            return new MemoryStream();
        }

        protected override bool HasContent(MemoryStream stream)
        {
            if (this.throwInHasContent)
            {
                throw new InvalidOperationException();
            }

            return true;
        }
    }

    private sealed class TestStreamPluginOptionsLoader
        : StreamPluginOptionsLoader<MemoryStream>
    {
        private readonly MemoryStream memoryStream;

        public TestStreamPluginOptionsLoader(MemoryStream memoryStream, bool closeStreamOnRead)
            : base(closeStreamOnRead, Logger.Null)
        {
            this.memoryStream = memoryStream;
        }

        protected override MemoryStream GetStream()
        {
            return this.memoryStream;
        }

        protected override bool HasContent(MemoryStream stream)
        {
            return stream.Length > 0;
        }
    }
}
