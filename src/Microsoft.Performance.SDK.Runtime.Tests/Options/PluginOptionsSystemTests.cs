// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.SDK.Runtime.Options.Serialization;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;
using Microsoft.Performance.SDK.Tests.Options;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Options;

/// <summary>
///     Contains tests for the <see cref="PluginOptionsSystem"/>.
/// </summary>
[TestClass]
[IntegrationTest]
public class PluginOptionsSystemTests
{
    [TestMethod]
    public async Task SavingNewOptions_DoesNotRemove_PreviouslySavedOption()
    {
        var previouslySavedOption = TestPluginOptionDto.BooleanOptionDto(Guid.NewGuid(), false, true);

        var sut = PluginOptionsSystem.CreateInMemory((_) => new NullLogger());
        await sut.Saver.TrySaveAsync(new PluginOptionsDto()
        {
            BooleanOptions = [previouslySavedOption],
        });

        var newOption = TestPluginOptionDefinition.FieldOptionDefinition("foo");
        sut.RegisterOptionsFrom(new StubProcessingSource(newOption));
        await sut.TrySaveCurrentRegistry();

        var newDto = await sut.Loader.TryLoadAsync();

        Assert.IsTrue(newDto.BooleanOptions.Contains(previouslySavedOption));
        Assert.IsTrue(newDto.FieldOptions.Any(x => x.Guid == newOption.Guid));
    }

    [TestMethod]
    public async Task SavingNewOptionValue_OverwritesPreviouslySavedOption()
    {
        bool previouslySavedValue = false;
        var optionGuid = Guid.NewGuid();
        var previouslySavedOption = TestPluginOptionDto.BooleanOptionDto(optionGuid, false, previouslySavedValue);

        var sut = PluginOptionsSystem.CreateInMemory((_) => new NullLogger());
        await sut.Saver.TrySaveAsync(new PluginOptionsDto()
        {
            BooleanOptions = [previouslySavedOption],
        });

        bool expectedValue = !previouslySavedValue;
        var newOption = TestPluginOptionDefinition.BooleanOptionDefinition(false, optionGuid);

        sut.RegisterOptionsFrom(new StubProcessingSource(newOption));

        var option = sut.Registry.Options.First(o => o.Guid == optionGuid && o is BooleanOption) as BooleanOption;
        Assert.IsNotNull(option);

        option.SetValue(expectedValue);

        await sut.TrySaveCurrentRegistry();

        var newDto = await sut.Loader.TryLoadAsync();

        Assert.AreEqual(expectedValue, newDto.BooleanOptions.First(o => o.Guid == optionGuid).Value);
    }

    [TestMethod]
    public async Task TrySaveCurrentRegistry_LoaderFails_FailsToSave()
    {
        var sut = new PluginOptionsSystem(
            new NullPluginOptionsLoader(),
            new InMemoryPluginOptionsDtoRepository(),
            new PluginOptionsRegistry(Logger.Null));

        sut.RegisterOptionsFrom(new StubProcessingSource(TestPluginOptionDefinition.FieldOptionDefinition("foo")));
        Assert.IsFalse(await sut.TrySaveCurrentRegistry());
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("    ")]
    [DataRow("\t")]
    [DataRow(@"C:\Foo?<>.json")]
    [DataRow(@"C:\")]
    [DataRow(@"bar.json")]
    [DataRow(@":C:\Foo\bar.json")]
    [DataRow(@"C:\Foo\:bar.json")]
    public void BadFilePath_ThrowsArgumentException(string filePath)
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            PluginOptionsSystem.CreateForFile(null, (_) => new NullLogger());
        });
    }

    [TestMethod]
    public void LongFilePath_ThrowsPathTooLongException()
    {
        Assert.ThrowsException<PathTooLongException>(() =>
        {
            PluginOptionsSystem.CreateForFile(Path.Combine(@"C:\", new string('a', 50000), "foo.json"), (_) => new NullLogger());
        });
    }

    [TestMethod]
    public void UnknownNetworkPath_ThrowsDirectoryNotFoundException()
    {
        Assert.ThrowsException<DirectoryNotFoundException>(() =>
        {
            PluginOptionsSystem.CreateForFile(@"\\randomServer\C$\foo.json", (_) => new NullLogger());
        });
    }

    private PluginOptionsSystem CreateSut(params IProcessingSource[] processingSources)
    {
        var sut = PluginOptionsSystem.CreateUnsaved((_) => new NullLogger());
        sut.RegisterOptionsFrom(processingSources);
        return sut;
    }

    private sealed class StubProcessingSource
            : ProcessingSource
        {
            public StubProcessingSource(params PluginOptionDefinition[] options)
            {
                this.pluginOptionsToReturn = new List<PluginOptionDefinition>(options);
            }

            protected override ICustomDataProcessor CreateProcessorCore(
                IEnumerable<IDataSource> dataSources,
                IProcessorEnvironment processorEnvironment,
                ProcessorOptions options)
            {
                return null;
            }

            protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
            {
                return false;
            }

            private readonly List<PluginOptionDefinition> pluginOptionsToReturn;
            public override IEnumerable<PluginOptionDefinition> PluginOptions => this.pluginOptionsToReturn ?? Enumerable.Empty<PluginOptionDefinition>();
        }
}
