// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Runtime.Options;
using Microsoft.Performance.SDK.Runtime.Options.Visitors;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests.Options;

/// <summary>
///     Tests that <see cref="IPluginOptionVisitorBase{TBase,TBool,TField,TFieldArray}"/> implementations are forced
///     to handle every concrete type of plugin option class.
/// </summary>
[TestClass]
public class PluginOptionVisitorBaseTests
{
    [UnitTest]
    [TestMethod]
    public void AllOptionDefinitions_CanBeVisited()
    {
        DoTest(
            typeof(PluginOptionDefinition),
            GetPluginOptionDefinitionForType,
            (instance) =>
            {
                var visitor = new DefinitionVisitor();
                new IPluginOptionDefinitionVisitor.Executor(visitor).Visit(instance);
            });
    }

    [UnitTest]
    [TestMethod]
    public void AllOptions_CanBeVisited()
    {
        DoTest(
            typeof(PluginOption),
            GetPluginOptionForType,
            (instance) =>
            {
                var visitor = new OptionVisitor();
                new IPluginOptionVisitor.Executor(visitor).Visit(instance);
            });
    }

    private void DoTest<T>(Type baseType, Func<Type, T> factory, Action<T> visitFunc)
    {
        // Use reflection to get every concrete type defined in the base type's assembly
        var optionTypes = baseType.Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType))
            .ToList();

        foreach (var optionType in optionTypes)
        {
            var instance = factory(optionType);
            visitFunc(instance);
        }
    }

    private PluginOptionDefinition GetPluginOptionDefinitionForType(Type type)
    {
        switch (type)
        {
            case Type boolType when boolType == typeof(BooleanOptionDefinition):
                return TestPluginOptionDefinition.BooleanOptionDefinition(true);
            case Type fieldType when fieldType == typeof(FieldOptionDefinition):
                return TestPluginOptionDefinition.FieldOptionDefinition("Foo");
            case Type fieldArrayType when fieldArrayType == typeof(FieldArrayOptionDefinition):
                return TestPluginOptionDefinition.FieldArrayOptionDefinition(["foo"]);
            default:
                Assert.Fail($"Unknown option type {type.Name}. If you are adding a new option type, please add it to this method.");
                return null;
        }
    }

    private PluginOption GetPluginOptionForType(Type type)
    {
        switch (type)
        {
            case Type boolType when boolType == typeof(BooleanOption):
                return TestPluginOption.BooleanOption(true);
            case Type fieldType when fieldType == typeof(FieldOption):
                return TestPluginOption.FieldOption("Foo");
            case Type fieldArrayType when fieldArrayType == typeof(FieldArrayOption):
                return TestPluginOption.FieldArrayOption(["foo"]);
            default:
                Assert.Fail($"Unknown option type {type.Name}. If you are adding a new option type, please add it to this method.");
                return null;
        }
    }

    private class DefinitionVisitor
        : IPluginOptionDefinitionVisitor
    {
        public void Visit(BooleanOptionDefinition option)
        {
        }

        public void Visit(FieldOptionDefinition option)
        {
        }

        public void Visit(FieldArrayOptionDefinition option)
        {
        }
    }

    private class OptionVisitor
        : IPluginOptionVisitor
    {
        public void Visit(BooleanOption option)
        {
        }

        public void Visit(FieldOption option)
        {
        }

        public void Visit(FieldArrayOption option)
        {
        }
    }
}
