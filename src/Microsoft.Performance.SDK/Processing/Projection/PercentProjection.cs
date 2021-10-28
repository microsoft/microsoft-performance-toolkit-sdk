// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    public static partial class Projection
    {
        /// <summary>
        ///     Defines <see cref="IProjection{TSource, TResult}"/> that project their values as percentages.
        /// </summary>
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
                  IVisibleDomainSensitiveProjection
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

                // IVisibleDomainSensitiveProjection
                public object Clone()
                {
                    if (this.DependsOnVisibleDomain)
                    {
                        return new PercentGenerator<TGenerator1, TGenerator2>(
                            VisibleDomainSensitiveProjection.CloneIfVisibleDomainSensitive(this.generatorNumerator),
                            VisibleDomainSensitiveProjection.CloneIfVisibleDomainSensitive(this.generatorDenominator));
                    }
                    else
                    {
                        return this;
                    }
                }

                public bool NotifyVisibleDomainChanged(IVisibleDomainRegion visibleDomain)
                {
                    bool result = false;
                    result |= VisibleDomainSensitiveProjection.NotifyVisibleDomainChanged(this.generatorDenominator, visibleDomain);
                    result |= VisibleDomainSensitiveProjection.NotifyVisibleDomainChanged(this.generatorNumerator, visibleDomain);
                    return result;
                }

                public bool DependsOnVisibleDomain
                {
                    get
                    {
                        bool result = false;
                        result |= VisibleDomainSensitiveProjection.DependsOnVisibleDomain(this.generatorDenominator);
                        result |= VisibleDomainSensitiveProjection.DependsOnVisibleDomain(this.generatorNumerator);
                        return result;
                    }
                }
            }
        }
    }
}
