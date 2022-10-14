// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    internal static class TableDescriptorUtils
    {
        internal static List<TableDescriptor> CreateTableDescriptors(
            ITableConfigurationsSerializer serializer,
            params Type[] types)
        {
            CreateTableDescriptors(serializer, out var descriptors, out _, types);

            return descriptors;
        }

        internal static void CreateTableDescriptors(
            ITableConfigurationsSerializer serializer,
            out List<TableDescriptor> descriptors,
            out List<Action<ITableBuilder, IDataExtensionRetrieval>> buildActions,
            params Type[] types)
        {
            Assert.IsNotNull(serializer);
            Assert.IsNotNull(types);

            descriptors = new List<TableDescriptor>(types.Length);
            buildActions = new List<Action<ITableBuilder, IDataExtensionRetrieval>>(types.Length);
            for (var i = 0; i < types.Length; ++i)
            {
                Assert.IsTrue(
                    TableDescriptorFactory.TryCreate(
                        types[i],
                        serializer,
                        out TableDescriptor expected,
                        out Action<ITableBuilder, IDataExtensionRetrieval> buildTableAction),
                    "Unable to create table descriptor for type `{0}`",
                    types[i]);

                Assert.IsNotNull(
                    expected,
                    "TableDescriptorFactory.TryCreate for type `{0}` returned true but yielded null.",
                    types[i]);

                descriptors.Add(expected);
                buildActions.Add(buildTableAction);
            }
        }
    }
}
