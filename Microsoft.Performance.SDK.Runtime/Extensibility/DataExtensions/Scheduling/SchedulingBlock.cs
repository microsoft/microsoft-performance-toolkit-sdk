// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Scheduling
{
    internal class SchedulingBlock
    {
        private LinkedListNode<SchedulingBlock> node;

        internal static SchedulingBlock CreateBlock(int index, LinkedList<SchedulingBlock> blocks)
        {
            var block = new SchedulingBlock(index);
            block.node = blocks.AddLast(block);
            return block;
        }

        private SchedulingBlock(int index)
        {
            this.Index = index;
        }

        internal int Index { get; }

        internal SchedulingBlock Next => this.node.Next?.Value;

        internal SchedulingBlock Previous => this.node.Previous?.Value;

        internal SchedulingBlock CreateNext()
        {
            return CreateBlock(this.Index + 1, this.node.List);
        }

        internal List<DataCookerSchedulingNode> Nodes { get; } = new List<DataCookerSchedulingNode>();
    }
}
