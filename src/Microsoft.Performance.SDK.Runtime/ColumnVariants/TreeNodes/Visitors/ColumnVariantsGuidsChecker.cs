// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes.Visitors;

internal sealed class ColumnVariantsGuidsChecker
{
    internal HashSet<Guid> FindDuplicateGuids(IColumnVariantsTreeNode root)
    {
        var visitor = new Visitor();
        root.Accept(visitor);
        return visitor.duplicateGuids;
    }

    private class Visitor
        : IColumnVariantsTreeNodesVisitor
    {
        private HashSet<Guid> seenGuids = new();
        public HashSet<Guid> duplicateGuids = new();

        public void Visit(NullColumnVariantsTreeNode nullColumnVariantsTreeNode)
        {
            // NOOP
        }

        public void Visit(ToggleableColumnVariantsTreeNode toggleableColumnVariantsTreeNode)
        {
            Process(toggleableColumnVariantsTreeNode.Descriptor.Guid);
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
            Process(modeColumnVariantsTreeNode.ModeDescriptor.Guid);
            modeColumnVariantsTreeNode.SubVariantsTreeNode.Accept(this);
        }

        private void Process(Guid guid)
        {
            if (!this.seenGuids.Add(guid))
            {
                this.duplicateGuids.Add(guid);
            }
        }
    }
}