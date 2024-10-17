// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes.Visitors;

internal sealed class ColumnVariantsExtractor
{
    internal IReadOnlyDictionary<ColumnVariantIdentifier, IDataColumn> ExtractVariants(
        IColumnVariantsTreeNode root)
    {
        var visitor = new Visitor();
        root.Accept(visitor);
        return visitor.foundVariants;
    }

    private class Visitor
        : IColumnVariantsTreeNodesVisitor
    {
        public Dictionary<ColumnVariantIdentifier, IDataColumn> foundVariants = new();

        public void Visit(NullColumnVariantsTreeNode nullColumnVariantsTreeNode)
        {
            // NOOP
        }

        public void Visit(ToggleableColumnVariantsTreeNode toggleableColumnVariantsTreeNode)
        {
            foundVariants.Add(toggleableColumnVariantsTreeNode.Identifier, toggleableColumnVariantsTreeNode.ToggledColumn);
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
            foundVariants.Add(modeColumnVariantsTreeNode.ModeIdentifier, modeColumnVariantsTreeNode.ModeColumn);
            modeColumnVariantsTreeNode.SubVariantsTreeNode.Accept(this);
        }
    }
}