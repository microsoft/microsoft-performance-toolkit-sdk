﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    internal static class TableDescriptorUtils
    {
        internal static List<TableDescriptor> CreateTableDescriptors(
            ISerializer serializer,
            params Type[] types)
        {
            CreateTableDescriptors(serializer, out var descriptors, out _, out _, types);

            return descriptors;
        }

        internal static void CreateTableDescriptors(
            ISerializer serializer,
            out List<TableDescriptor> descriptors,
            out List<Action<ITableBuilder, IDataExtensionRetrieval>> buildActions,
            out List<bool> isInternal,
            params Type[] types)
        {
            Assert.IsNotNull(serializer);
            Assert.IsNotNull(types);

            descriptors = new List<TableDescriptor>(types.Length);
            buildActions = new List<Action<ITableBuilder, IDataExtensionRetrieval>>(types.Length);
            isInternal = new List<bool>(types.Length);
            for (var i = 0; i < types.Length; ++i)
            {
                Assert.IsTrue(
                    TableDescriptorFactory.TryCreate(
                        types[i],
                        serializer,
                        out var isInternalTable,
                        out var expected,
                        out var buildTableAction),
                    "Unable to create table descriptor for type `{0}`",
                    types[i]);

                Assert.IsNotNull(
                    expected,
                    "TableDescriptorFactory.TryCreate for type `{0}` returned true but yielded null.",
                    types[i]);

                descriptors.Add(expected);
                buildActions.Add(buildTableAction);
                isInternal.Add(isInternalTable);
            }
        }
    }
}
