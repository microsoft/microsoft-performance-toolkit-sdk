using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnVariants;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding
{
    internal class IdentityDataColumnVariantsProcessor
        : IDataColumnVariantsProcessor
    {
        public IColumnVariant Output { get; private set; }

        public void ProcessColumnVariants(IColumnVariant variants)
        {
            this.Output = variants;
        }
    }

    internal class ColumnVariantsModesBuilder
        : IColumnVariantsModesBuilder
    {
        private readonly IDataColumnVariantsProcessor processor;
        private readonly IDataColumn baseColumn;

        private readonly IReadOnlyList<(ColumnVariantWithColumn, BuilderCallbackRunner<IColumnVariantsRootBuilder>)> modes;
        private readonly int? defaultModeIndex;

        public ColumnVariantsModesBuilder(
            IDataColumnVariantsProcessor processor,
            IDataColumn baseColumn,
            IReadOnlyList<(ColumnVariantWithColumn, BuilderCallbackRunner<IColumnVariantsRootBuilder>)> modes,
            int? defaultModeIndex)
        {
            this.processor = processor;
            this.modes = modes;
            this.defaultModeIndex = defaultModeIndex;
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

            List<IColumnVariant> modeVariants = new List<IColumnVariant>();

            for (int i = 0; i < this.modes.Count; i++)
            {
                var (columnVariantWithColumn, modeCallbackRunner) = this.modes[i];

                IColumnVariant subVariant = null;
                if (modeCallbackRunner != null)
                {
                    subVariant = modeCallbackRunner.RunCallback();
                }

                var modeVariant = new ModeColumnVariant(columnVariantWithColumn.Identifier, subVariant);
                modeVariants.Add(modeVariant);
            }

            this.processor.ProcessColumnVariants(
                new ModesColumnVariant(
                    modeVariants.ToArray(),
                    this.defaultModeIndex ?? 0));
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
            Action<IColumnVariantsRootBuilder> builderCallback)
        {
            if (this.modes.Any(m => m.Item1.Identifier.Id == modeIdentifier.Id))
            {
                throw new InvalidOperationException($"A mode with identifier {modeIdentifier} has already been added.");
            }

            // Make new column
            var newColumn =
                new ColumnVariantWithColumn(modeIdentifier, new DataColumn<T>(baseColumn.Configuration, column));

            BuilderCallbackRunner<IColumnVariantsRootBuilder> callbackRunner = null;
            if (builderCallback != null)
            {
                callbackRunner = new BuilderCallbackRunner<IColumnVariantsRootBuilder>(
                    this.baseColumn,
                    (newProcessor) => new ColumnVariantsTogglesBuilder(
                        newProcessor,
                        baseColumn,
                        new List<ColumnVariantWithColumn>(),
                        null),
                    builderCallback);
            }

            return new ColumnVariantsModesBuilder(
                this.processor,
                this.baseColumn,
                this.modes.Concat((newColumn, callbackRunner)).ToList(),
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
                if (this.modes[i].Item1.Identifier.Id == modeIdentifierGuid)
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
                index);
        }
    }

    internal class ColumnVariantsTogglesBuilder
        : IColumnVariantsRootBuilder
    {
        private readonly IDataColumnVariantsProcessor processor;
        private readonly IDataColumn baseColumn;
        private readonly IReadOnlyList<ColumnVariantWithColumn> toggleLevels;
        private readonly BuilderCallbackRunner<IColumnVariantsModesBuilder> builderCallbackRunner;

        public ColumnVariantsTogglesBuilder(
            IDataColumnVariantsProcessor processor,
            IDataColumn baseColumn,
            IReadOnlyList<ColumnVariantWithColumn> toggleLevels,
            BuilderCallbackRunner<IColumnVariantsModesBuilder> builderCallbackRunner)
        {
            this.processor = processor;
            this.baseColumn = baseColumn;
            this.toggleLevels = toggleLevels;
            this.builderCallbackRunner = builderCallbackRunner;
        }

        public void Build()
        {
            IColumnVariant variant = null;
            if (this.builderCallbackRunner != null)
            {
                variant = this.builderCallbackRunner.RunCallback();
            }

            for (var i = this.toggleLevels.Count - 1; i >= 0; i--)
            {
                var toggle = this.toggleLevels[i];

                variant = new ToggleableColumnVariant(toggle.Identifier, variant);
            }

            if (variant == null)
            {
                return;
            }

            this.processor.ProcessColumnVariants(variant);
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
                this.builderCallbackRunner);
        }

        public IColumnVariantsBuilder WithModes(Action<IColumnVariantsModesBuilder> builderCallback)
        {
            Debug.Assert(this.builderCallbackRunner == null);

            if (this.builderCallbackRunner != null)
            {
                throw new InvalidOperationException("Modes have already been set.");
            }

            var callbackRunner = new BuilderCallbackRunner<IColumnVariantsModesBuilder>(
                this.baseColumn,
                (newProcessor) => new ColumnVariantsModesBuilder(
                    newProcessor,
                    baseColumn,
                    new List<(ColumnVariantWithColumn, BuilderCallbackRunner<IColumnVariantsRootBuilder>)>(),
                    null),
                builderCallback);

            return new ColumnVariantsTogglesBuilder(
                this.processor,
                this.baseColumn,
                this.toggleLevels,
                callbackRunner);
        }
    }

    // Runs the callback for the modes builder and returns the built variant (if any)
    internal class BuilderCallbackRunner<TBuilder>
        where TBuilder : IColumnVariantsBuilder
    {
        private readonly TBuilder builder;
        private readonly Action<TBuilder> builderCallback;
        private readonly IdentityDataColumnVariantsProcessor processor;

        public BuilderCallbackRunner(
            IDataColumn baseColumn,
            Func<IDataColumnVariantsProcessor, TBuilder> builderFactory,
            Action<TBuilder> builderCallback)
        {
            this.builderCallback = builderCallback;

            this.processor = new IdentityDataColumnVariantsProcessor();
            this.builder = builderFactory(this.processor);
        }

        public IColumnVariant RunCallback()
        {
            this.builderCallback.Invoke(this.builder);
            return this.processor.Output;
        }
    }
}