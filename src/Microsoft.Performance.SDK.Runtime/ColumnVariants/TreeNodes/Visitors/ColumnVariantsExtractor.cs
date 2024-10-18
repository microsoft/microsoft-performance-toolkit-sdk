// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes.Visitors;

internal sealed class ColumnVariantsExtractor
{
    internal IReadOnlyDictionary<ColumnVariantDescriptor, IDataColumn> ExtractVariants(
        IColumnVariantsTreeNode root)
    {
        var visitor = new Visitor();
        root.Accept(visitor);
        return visitor.foundVariants;
    }

    private class Visitor
        : IColumnVariantsTreeNodesVisitor
    {
        public Dictionary<ColumnVariantDescriptor, IDataColumn> foundVariants = new();

        public void Visit(NullColumnVariantsTreeNode nullColumnVariantsTreeNode)
        {
            // NOOP
        }

        public void Visit(ToggleableColumnVariantsTreeNode toggleableColumnVariantsTreeNode)
        {
            foundVariants.Add(toggleableColumnVariantsTreeNode.Descriptor, toggleableColumnVariantsTreeNode.ToggledColumn);
            toggleableColumnVariantsTreeNode.SubVariantsTreeNode.Accept(this);
        }

        public void Visit(ModesToggleColumnVariantsTreeNode modesToggle)
        {
            modesToggle.Modes.Accept(this);
        }

        public void Visit(ModesColumnVariantsTreeNode modesColumnVariantsTreeNode)
        {
            foreach (var mode in modesColumnVariantsTreeNode.Modes)
            {
                mode.Accept(this);
            }
        }

        public void Visit(ModeColumnVariantsTreeNode modeColumnVariantsTreeNode)
        {
            foundVariants.Add(modeColumnVariantsTreeNode.ModeDescriptor, modeColumnVariantsTreeNode.ModeColumn);
            modeColumnVariantsTreeNode.SubVariantsTreeNode.Accept(this);
        }
    }
}