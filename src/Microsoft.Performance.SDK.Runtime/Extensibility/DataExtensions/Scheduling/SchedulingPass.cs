// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Scheduling
{
    internal class SchedulingPass
        : IEquatable<SchedulingPass>
    {
        private LinkedListNode<SchedulingPass> node;

        internal static SchedulingPass CreatePass(int index, LinkedList<SchedulingPass> passes)
        {
            var pass = new SchedulingPass(index);
            pass.node = passes.AddLast(pass);
            return pass;
        }

        private SchedulingPass(int index)
        {
            this.Index = index;
            this.Blocks = new LinkedList<SchedulingBlock>();
            SchedulingBlock.CreateBlock(0, this.Blocks);
        }

        internal int Index { get; }

        internal SchedulingBlock Block0 => this.Blocks.First.Value;

        internal SchedulingPass Next => this.node.Next?.Value;

        internal SchedulingPass Previous => this.node.Previous?.Value;

        internal SchedulingPass CreateNext()
        {
            return CreatePass(this.Index + 1, this.node.List);
        }

        internal SchedulingBlock GetBlock(int blockIndex)
        {
            Debug.Assert(blockIndex < this.Blocks.Count);
            Debug.Assert(blockIndex >= 0);

            var block = this.Block0;

            while (blockIndex > 0)
            {
                block = block.Next;
                blockIndex--;
            }

            return block;
        }

        private LinkedList<SchedulingBlock> Blocks { get; }

        internal int BlockCount => this.Blocks.Count;

        internal int GetCookerCount()
        {
            int count = 0;

            for (var block = this.Block0; block != null; block = block.Next)
            {
                count = block.Nodes.Count;
            }

            return count;
        }

        internal List<IDataCookerDescriptor> GetCookers()
        {
            List<IDataCookerDescriptor> cookers = new List<IDataCookerDescriptor>(this.GetCookerCount());

            foreach (var block in this.Blocks)
            {
                foreach (var node in block.Nodes)
                {
                    cookers.Add(node.DataCooker);
                }
            }

            return cookers;
        }

        public bool Equals(SchedulingPass other)
        {
            if (ReferenceEquals(null, other))
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((SchedulingPass)obj);
        }

        public override int GetHashCode()
        {
            return this.Index;
        }
    }
}
