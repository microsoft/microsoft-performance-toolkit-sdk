// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    public static partial class Projection
    {
        public static class Percent
        {
            /// <summary>
            ///     Creates a projector that results in a dividing the value of the numerator projection
            ///     by the value of the denominator projection.
            /// </summary>
            /// <param name="numeratorColumn">
            ///     Numerator projection.
            /// </param>
            /// <param name="divisorColumn">
            ///     Denominator projection.
            /// </param>
            /// <returns>
            ///     Projection of the computed division.
            /// </returns>
            public static IProjection<int, double> Create(
                IProjection<int, double> numeratorColumn, 
                IProjection<int, double> divisorColumn)
            {
                Guard.NotNull(numeratorColumn, nameof(numeratorColumn));
                Guard.NotNull(divisorColumn, nameof(divisorColumn));

                var typeArgs = new[]
                {
                    numeratorColumn.GetType(),
                    divisorColumn.GetType(),
                };

                var constructorArgs = new object[]
                {
                    numeratorColumn,
                    divisorColumn,
                };

                var type = typeof(PercentGenerator<,>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, double>)instance;
            }

            /// <summary>
            ///     Creates a projector that results in a dividing the value of the numerator projection
            ///     by the value of the denominator projection.
            /// </summary>
            /// <param name="numeratorColumn">
            ///     Numerator projection.
            /// </param>
            /// <param name="divisorColumn">
            ///     Denominator projection.
            /// </param>
            /// <returns>
            ///     Projection of the computed division.
            /// </returns>
            public static IProjection<int, double> Create(
                IProjection<int, TimestampDelta> numeratorColumn, 
                IProjection<int, TimestampDelta> divisorColumn)
            {
                Guard.NotNull(numeratorColumn, nameof(numeratorColumn));
                Guard.NotNull(divisorColumn, nameof(divisorColumn));

                var numeratorAsDouble = TimestampDeltaToDouble.Create(numeratorColumn);
                var divisorAsDouble = TimestampDeltaToDouble.Create(divisorColumn);
                return Create(numeratorAsDouble, divisorAsDouble);
            }

            /// <summary>
            ///     Creates a projector that results in a dividing the value of the numerator projection
            ///     by the value of the denominator projection.
            /// </summary>
            /// <param name="numeratorColumn">
            ///     Numerator projection.
            /// </param>
            /// <param name="divisorColumn">
            ///     Denominator projection.
            /// </param>
            /// <returns>
            ///     Projection of the computed division.
            /// </returns>
            public static IProjection<int, double> Create(
                IProjection<int, int> numeratorColumn, 
                IProjection<int, ulong> divisorColumn)
            {
                Guard.NotNull(numeratorColumn, nameof(numeratorColumn));
                Guard.NotNull(divisorColumn, nameof(divisorColumn));

                var numeratorAsDouble = IntToDouble.Create(numeratorColumn);
                var divisorAsDouble = UlongToDouble.Create(divisorColumn);
                return Create(numeratorAsDouble, divisorAsDouble);
            }

            /// <summary>
            ///     Creates a projector that results in a dividing the value of the numerator projection
            ///     by the value of the denominator projection.
            /// </summary>
            /// <param name="numeratorColumn">
            ///     Numerator projection.
            /// </param>
            /// <param name="divisorColumn">
            ///     Denominator projection.
            /// </param>
            /// <returns>
            ///     Projection of the computed division.
            /// </returns>
            public static IProjection<int, double> Create(
                IProjection<int, ulong> numeratorColumn, 
                IProjection<int, ulong> divisorColumn)
            {
                Guard.NotNull(numeratorColumn, nameof(numeratorColumn));
                Guard.NotNull(divisorColumn, nameof(divisorColumn));

                var numeratorAsDouble = UlongToDouble.Create(numeratorColumn);
                var divisorAsDouble = UlongToDouble.Create(divisorColumn);
                return Create(numeratorAsDouble, divisorAsDouble);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            private struct PercentGenerator<TGenerator1, TGenerator2>
                : IProjection<int, double>,
                  IViewportSensitiveProjection
                  where TGenerator1 : IProjection<int, double>
                  where TGenerator2 : IProjection<int, double>
            {
                private TGenerator1 generatorNumerator;
                private TGenerator2 generatorDenominator;

                public PercentGenerator(TGenerator1 generatorNumerator, TGenerator2 generatorDenominator)
                {
                    this.generatorNumerator = generatorNumerator;
                    this.generatorDenominator = generatorDenominator;
                }

                public double this[int value]
                {
                    get
                    {
                        double denominator = this.generatorDenominator[value];

                        if (denominator > 0.0)
                        {
                            return this.generatorNumerator[value] / denominator * 100;
                        }

                        return 0.0;
                    }
                }

                public Type SourceType
                {
                    get { return typeof(int); }
                }

                public Type ResultType
                {
                    get { return typeof(double); }
                }

                // IViewportSensitiveProjection
                public object Clone()
                {
                    if (this.DependsOnViewport)
                    {
                        return new PercentGenerator<TGenerator1, TGenerator2>(
                            ViewportSensitiveProjection.CloneIfViewportSensitive(this.generatorNumerator),
                            ViewportSensitiveProjection.CloneIfViewportSensitive(this.generatorDenominator));
                    }
                    else
                    {
                        return this;
                    }
                }

                public bool NotifyViewportChanged(IVisibleTableRegion viewport)
                {
                    bool result = false;
                    result |= ViewportSensitiveProjection.NotifyViewportChanged(this.generatorDenominator, viewport);
                    result |= ViewportSensitiveProjection.NotifyViewportChanged(this.generatorNumerator, viewport);
                    return result;
                }

                public bool DependsOnViewport
                {
                    get
                    {
                        bool result = false;
                        result |= ViewportSensitiveProjection.DependsOnViewport(this.generatorDenominator);
                        result |= ViewportSensitiveProjection.DependsOnViewport(this.generatorNumerator);
                        return result;
                    }
                }
            }
        }
    }
}
