// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    public static partial class Projection
    {
        /// <summary>
        ///     Helper class containing useful methods for generating projections.
        /// </summary>
        public static class TimestampDeltaToDouble
        {
            /// <summary>
            ///     Creates a projection that returns the TimestampDelta in nanoseconds as a <see cref="double"/>.
            /// </summary>
            /// <param name="column">
            ///     Projection that returns a <see cref="TimestampDelta"/>.
            /// </param>
            /// <returns>
            ///     Projection that returns a nanosecond count as a double.
            /// </returns>
            public static IProjection<int, double> Create(IProjection<int, TimestampDelta> column)
            {
                Guard.NotNull(column, nameof(column));

                var typeArgs = new[]
                {
                    column.GetType(),
                };

                var constructorArgs = new object[]
                {
                    column,
                };

                var type = typeof(TimestampDeltaToDoubleColumnGenerator<>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, double>)instance;
            }

            private struct TimestampDeltaToDoubleColumnGenerator<TGenerator>
                : IProjection<int, double>,
                  IVisibleDomainSensitiveProjection
                  where TGenerator : IProjection<int, TimestampDelta>
            {
                private readonly TGenerator generator;

                public TimestampDeltaToDoubleColumnGenerator(TGenerator generator)
                {
                    this.generator = generator;
                }

                public double this[int value]
                {
                    get
                    {
                        return this.generator[value].ToNanoseconds;
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
                    if (DependsOnVisibleDomain)
                    {
                        return new TimestampDeltaToDoubleColumnGenerator<TGenerator>(
                            VisibleDomainSensitiveProjection.CloneIfVisibleDomainSensitive(this.generator));
                    }
                    else
                    {
                        return this;
                    }
                }

                public bool NotifyVisibleDomainChanged(IVisibleDomainRegion visibleDomain)
                {
                    return VisibleDomainSensitiveProjection.NotifyVisibleDomainChanged(this.generator, visibleDomain);
                }

                public bool DependsOnVisibleDomain
                {
                    get
                    {
                        return VisibleDomainSensitiveProjection.DependsOnVisibleDomain(this.generator);
                    }
                }
            }
        }
    }
}
