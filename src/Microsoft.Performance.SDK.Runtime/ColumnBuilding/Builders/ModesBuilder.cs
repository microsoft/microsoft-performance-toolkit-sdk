using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnVariants;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders
{
    public class ModesBuilder
        : IColumnVariantsModesBuilder
    {
        private readonly IDataColumn baseColumn;
        private readonly IDataColumnVariantsProcessor processor;
        private readonly List<AddedMode> addedModes;
        private readonly int? defaultModeIndex;

        public record AddedMode(
            ColumnVariantIdentifier Identifier,
            IDataColumn column,
            Action<IToggleableColumnVariantsBuilder> builder);

        public ModesBuilder(
            IDataColumnVariantsProcessor processor,
            List<AddedMode> addedModes,
            IDataColumn baseColumn,
            int? defaultModeIndex)
        {
            this.processor = processor;
            this.addedModes = addedModes;
            this.baseColumn = baseColumn;
            this.defaultModeIndex = defaultModeIndex;
        }

        public void Build()
        {
            if (this.addedModes.Count == 0)
            {
                this.processor.ProcessColumnVariants(NullColumnVariant.Instance);
                return;
            }

            List<ModeColumnVariant> modeVariants = new();
            foreach (AddedMode mode in this.addedModes)
            {
                var callbackInvoker = new ModeCallbackInvoker(mode.builder, this.baseColumn);

                IColumnVariant subVariant = NullColumnVariant.Instance;
                if (callbackInvoker.TryGet(out var builtVariant))
                {
                    subVariant = builtVariant;
                }
                modeVariants.Add(new ModeColumnVariant(mode.Identifier, mode.column, subVariant));
            }

            var variant = new ModesColumnVariant(modeVariants, this.defaultModeIndex ?? 0);
            this.processor.ProcessColumnVariants(variant);
        }

        public IColumnVariantsModesBuilder WithMode<T>(
            ColumnVariantIdentifier modeIdentifier,
            IProjection<int, T> column)
        {
            return WithMode(modeIdentifier, column, null);
        }

        public IColumnVariantsModesBuilder WithMode<T>(
            ColumnVariantIdentifier modeIdentifier,
            IProjection<int, T> column,
            Action<IToggleableColumnVariantsBuilder> builder)
        {
            AddedMode newMode = new(
                modeIdentifier,
                new DataColumn<T>(this.baseColumn.Configuration, column),
                builder);

            return new ModesBuilder(
                this.processor,
                this.addedModes.Append(newMode).ToList(),
                this.baseColumn,
                this.defaultModeIndex);
        }

        public IColumnVariantsBuilder WithDefaultMode(Guid modeIdentifierGuid)
        {
            Debug.Assert(!this.defaultModeIndex.HasValue);

            if (this.defaultModeIndex.HasValue)
            {
                throw new InvalidOperationException("Cannot have more than one default mode.");
            }

            int? index = null;
            for (int i = 0; i < this.addedModes.Count; i++)
            {
                if (this.addedModes[i].Identifier.Id == modeIdentifierGuid)
                {
                    index = i;
                    break;
                }
            }

            if (!index.HasValue)
            {
                throw new InvalidOperationException($"No mode with identifier {modeIdentifierGuid} has been added.");
            }

            return new ModesBuilder(
                this.processor,
                this.addedModes,
                this.baseColumn,
                index);
        }
    }
}