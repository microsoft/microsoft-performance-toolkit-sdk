// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.SDK.Tests.Options;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Options;

[TestClass]
[IntegrationTest]
public class PluginOptionsSystemTests
{
    [TestMethod]
    public void OptionReturnedBySystem_DoNotReflectChangeOnOriginalInstances()
    {
        const string defaultValue = "default value";
        var optionReturnedByProcessingSource = TestPluginOptions.CreateFieldOption(defaultValue);

        var sut = CreateSut(new StubProcessingSource(optionReturnedByProcessingSource));

        optionReturnedByProcessingSource.CurrentValue = "new value";

        var registeredOption = sut.Registry.Options.First() as FieldOption;

        Assert.IsNotNull(registeredOption);
        Assert.AreEqual(defaultValue, registeredOption.CurrentValue);
        Assert.IsTrue(registeredOption.IsUsingDefault);
    }

    [TestMethod]
    public void OptionReturnedBySystem_DoNotChangeOriginalInstance()
    {
        const string defaultValue = "default value";
        var optionReturnedByProcessingSource = TestPluginOptions.CreateFieldOption(defaultValue);

        var sut = CreateSut(new StubProcessingSource(optionReturnedByProcessingSource));
        var registeredOption = sut.Registry.Options.First() as FieldOption;

        Assert.IsNotNull(registeredOption);
        registeredOption.CurrentValue = "new value";

        Assert.AreEqual(defaultValue, optionReturnedByProcessingSource.CurrentValue);
    }

    private PluginOptionsSystem CreateSut(params IProcessingSource[] processingSources)
    {
        var sut = PluginOptionsSystem.CreateUnsaved();
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