using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Processing.DataColumns;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding
{
    internal class DataColumnVariant
        : IDataColumnVariant
    {
        public DataColumnVariant(
            ColumnVariantIdentifier identifier,
            IDataColumnVariants subVariants)
        {
            Identifier = identifier;
            SubVariants = subVariants;
        }

        public ColumnVariantIdentifier Identifier { get; }
        public IDataColumnVariants SubVariants { get; }
    }
    internal class DataColumnVariants
        : IDataColumnVariants
    {
        public DataColumnVariants(
            ColumnVariantsType type,
            IDataColumnVariant[] possibleVariants)
        {
            Type = type;
            PossibleVariants = possibleVariants;
        }

        public ColumnVariantsType Type { get; }
        public IDataColumnVariant[] PossibleVariants { get; }
    }

    internal class IdentityDataColumnVariantsProcessor
        : IDataColumnVariantsProcessor
    {
        public IDataColumnVariants Output { get; private set; }

        public void ProcessColumnVariants(IDataColumnVariants variants)
        {
            this.Output = variants;
        }
    }

    internal class DataColumnVariantsCombiner
        : IDataColumnVariantsProcessor
    {
        private readonly IDataColumnVariant self;

        public DataColumnVariantsCombiner(IDataColumnVariant self)
        {
            this.self = self;
        }

        public IDataColumnVariant Output { get; private set; }

        public void ProcessColumnVariants(IDataColumnVariants variants)
        {
            this.Output =
                new DataColumnVariant(
                    this.self.Identifier, variants);
        }
    }

    internal class ColumnVariantsModesBuilder
        : IColumnVariantsModesBuilder
    {
        private readonly IDataColumnVariantsProcessor processor;
        private readonly IDataColumn baseColumn;
        private readonly IReadOnlyList<ColumnVariantWithColumn> modes;
        private readonly IReadOnlyList<Action<IColumnVariantsRootBuilder>> subBuilders;
        private readonly int? defaultModeIndex;

        public ColumnVariantsModesBuilder(
            IDataColumnVariantsProcessor processor,
            IDataColumn baseColumn,
            IReadOnlyList<ColumnVariantWithColumn> modes,
            IReadOnlyList<Action<IColumnVariantsRootBuilder>> subBuilders,
            int? defaultModeIndex)
        {
            this.processor = processor;
            this.modes = modes;
            this.defaultModeIndex = defaultModeIndex;
            this.subBuilders = subBuilders;
            this.baseColumn = baseColumn;
        }

        public void Build()
        {
            if (this.modes.Count == 0)
            {
                if (!this.defaultModeIndex.HasValue)
                {
                    // Nothing to do
                    return;
                }
                else
                {
                    Debug.Fail("This should never happen.");
                    throw new InvalidOperationException("Cannot have a default mode with no modes.");
                }
            }

            ColumnVariantWithColumn defaultMode = this.modes[this.defaultModeIndex ?? 0];



            List<IDataColumnVariant> variantsList = new List<IDataColumnVariant>();
            for (int i = 0; i < this.modes.Count; i++)
            {
                ColumnVariantWithColumn mode = this.modes[i];
                Action<IColumnVariantsRootBuilder> subBuilder = this.subBuilders[i];

                var modeVariant = new DataColumnVariant(mode.Identifier, null);

                if (subBuilder != null)
                {
                    // Invoke the sub-builder and append its output as the sub-variants
                    var processor = new DataColumnVariantsCombiner(modeVariant);

                }

                variantsList.Add(modeVariant);
            }

            this.processor.ProcessColumnVariants(
                new DataColumnVariants(
                    ColumnVariantsType.Modes,
                    variantsList.ToArray()));
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
            Action<IColumnVariantsRootBuilder> builder)
        {
            if (this.modes.Any(m => m.Identifier.Id == modeIdentifier.Id))
            {
                throw new InvalidOperationException($"A mode with identifier {modeIdentifier} has already been added.");
            }

            // Make new column
            var newColumn =
                new ColumnVariantWithColumn(modeIdentifier, new DataColumn<T>(baseColumn.Configuration, column));

            return new ColumnVariantsModesBuilder(
                this.processor,
                this.baseColumn,
                this.modes.Concat(newColumn).ToList(),
                this.subBuilders.Concat(builder).ToList(),
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
            for (int i = 0; i < this.modes.Count; i++)
            {
                if (this.modes[i].Identifier.Id == modeIdentifierGuid)
                {
                    index = i;
                    break;
                }
            }

            if (!index.HasValue)
            {
                throw new InvalidOperationException($"No mode with identifier {modeIdentifierGuid} has been added.");
            }

            return new ColumnVariantsModesBuilder(
                this.processor,
                this.baseColumn,
                this.modes,
                this.subBuilders,
                index);
        }
    }

    internal class ColumnVariantsTogglesBuilder
        : IColumnVariantsRootBuilder
    {
        private readonly IDataColumnVariantsProcessor processor;
        private readonly IDataColumn baseColumn;
        private readonly IReadOnlyList<ColumnVariantWithColumn> toggleLevels;
        private readonly ModesBuilderWrapper modesBuilderWrapper;

        public ColumnVariantsTogglesBuilder(
            IDataColumnVariantsProcessor processor,
            IDataColumn baseColumn,
            IReadOnlyList<ColumnVariantWithColumn> toggleLevels,
            ModesBuilderWrapper modesBuilderWrapper)
        {
            this.processor = processor;
            this.baseColumn = baseColumn;
            this.toggleLevels = toggleLevels;
            this.modesBuilderWrapper = modesBuilderWrapper;
        }

        public void Build()
        {
            IDataColumnVariants variants = null;
            if (this.modesBuilderWrapper != null)
            {
                variants = this.modesBuilderWrapper.Build();
            }

            for (var i = this.toggleLevels.Count - 1; i >= 0; i--)
            {
                var toggle = this.toggleLevels[i];

                var currentLevelVariant = new DataColumnVariant(toggle.Identifier, variants);

                variants = new DataColumnVariants(
                    ColumnVariantsType.Toggles,
                    new[] { currentLevelVariant });
            }

            if (variants == null)
            {
                return;
            }

            this.processor.ProcessColumnVariants(variants);
        }

        public IColumnVariantsRootBuilder WithToggle<T>(
            ColumnVariantIdentifier toggleIdentifier, IProjection<int, T> column)
        {
            // Make new column
            var newColumn = new ColumnVariantWithColumn(toggleIdentifier, new DataColumn<T>(this.baseColumn.Configuration, column));

            return new ColumnVariantsTogglesBuilder(
                this.processor,
                this.baseColumn,
                this.toggleLevels.Concat(newColumn).ToList(),
                this.modesBuilderWrapper);
        }

        public IColumnVariantsBuilder WithModes(Action<IColumnVariantsModesBuilder> builder)
        {
            Debug.Assert(this.modesBuilderWrapper == null);

            if (this.modesBuilderWrapper != null)
            {
                throw new InvalidOperationException("Modes have already been set.");
            }

            var newWrapper = new ModesBuilderWrapper(this.baseColumn, builder);

            return new ColumnVariantsTogglesBuilder(
                this.processor,
                this.baseColumn,
                this.toggleLevels,
                newWrapper);
        }

        internal class ModesBuilderWrapper
        {
            private readonly ColumnVariantsModesBuilder builder;
            private readonly Action<IColumnVariantsModesBuilder> builderCallback;
            private readonly IdentityDataColumnVariantsProcessor processor;

            public ModesBuilderWrapper(IDataColumn baseColumn, Action<IColumnVariantsModesBuilder> builderCallback)
            {
                this.builderCallback = builderCallback;

                this.processor = new IdentityDataColumnVariantsProcessor();

                this.builder = new ColumnVariantsModesBuilder(
                    this.processor,
                    baseColumn,
                    new List<ColumnVariantWithColumn>(),
                    new List<Action<IColumnVariantsRootBuilder>>(),
                    null);
            }

            public IDataColumnVariants Build()
            {
                this.builderCallback.Invoke(this.builder);
                return this.processor.Output;
            }
        }
    }
}