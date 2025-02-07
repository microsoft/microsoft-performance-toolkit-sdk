// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Options;
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
    /// <summary>
    ///     Asserts that changes to a <see cref="PluginOption"/> instance returned by a <see cref="IProcessingSource"/>
    ///     are not reflected by the <see cref="PluginOption"/> instance returned by the <see cref="PluginOptionsSystem"/>'s
    ///     <see cref="PluginOptionsRegistry"/>.
    /// </summary>
    [TestMethod]
    public void OptionReturnedBySystem_DoNotReflectChangeOnOriginalInstances()
    {
        const string defaultValue = "default value";
        var optionReturnedByProcessingSource = TestPluginOption.FieldOption(defaultValue);

        var sut = CreateSut(new StubProcessingSource(optionReturnedByProcessingSource));

        optionReturnedByProcessingSource.CurrentValue = "new value";

        var registeredOption = sut.Registry.Options.First() as FieldOption;

        Assert.IsNotNull(registeredOption);
        Assert.AreEqual(defaultValue, registeredOption.CurrentValue);
        Assert.IsTrue(registeredOption.IsUsingDefault);
    }

    /// <summary>
    ///     Asserts that changes to a <see cref="PluginOption"/> instance returned by the <see cref="PluginOptionsSystem"/>'s
    ///     <see cref="PluginOptionsRegistry"/> are not reflected by the <see cref="PluginOption"/> instance returned by the
    ///     <see cref="IProcessingSource"/>.
    /// </summary>
    [TestMethod]
    public void OptionReturnedBySystem_DoNotChangeOriginalInstance()
    {
        const string defaultValue = "default value";
        var optionReturnedByProcessingSource = TestPluginOption.FieldOption(defaultValue);

        var sut = CreateSut(new StubProcessingSource(optionReturnedByProcessingSource));
        var registeredOption = sut.Registry.Options.First() as FieldOption;

        Assert.IsNotNull(registeredOption);
        registeredOption.CurrentValue = "new value";

        Assert.AreEqual(defaultValue, optionReturnedByProcessingSource.CurrentValue);
    }

    [TestMethod]
    public async Task SavingNewOptions_DoesNotRemove_PreviouslySavedOption()
    {
        var previouslySavedOption = TestPluginOptionDto.BooleanOptionDto(Guid.NewGuid(), false, true);

        var sut = PluginOptionsSystem.CreateInMemory((_) => new NullLogger());
        await sut.Saver.TrySaveAsync(new PluginOptionsDto()
        {
            BooleanOptions = [previouslySavedOption],
        });

        var newOption = TestPluginOption.FieldOption("foo");
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
        var newOption = TestPluginOption.BooleanOption(false, optionGuid);
        newOption.CurrentValue = expectedValue;

        sut.RegisterOptionsFrom(new StubProcessingSource(newOption));
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

        sut.RegisterOptionsFrom(new StubProcessingSource(TestPluginOption.FieldOption("foo")));
        Assert.IsFalse(await sut.TrySaveCurrentRegistry());
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
            public StubProcessingSource(params PluginOption[] options)
            {
                this.pluginOptionsToReturn = new List<PluginOption>(options);
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

            private readonly List<PluginOption> pluginOptionsToReturn;
            public override IEnumerable<PluginOption> PluginOptions => this.pluginOptionsToReturn ?? Enumerable.Empty<PluginOption>();
        }
}
