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
        public static class IntToDouble
        {
            /// <summary>
            ///     This will create a projection that typecasts an <see cref="int"/> value to a <see cref="double"/>.
            /// </summary>
            /// <param name="column">
            ///     Projection that returns an integer.
            /// </param>
            /// <returns>
            ///     Projection that returns a double.
            /// </returns>
            public static IProjection<int, double> Create(IProjection<int, int> column)
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

                var type = typeof(IntToDoubleColumnGenerator<>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, double>)instance;
            }

            private struct IntToDoubleColumnGenerator<TGenerator>
                : IProjection<int, double>,
                  IVisibleDomainSensitiveProjection
                  where TGenerator : IProjection<int, int>
            {
                private readonly TGenerator generator;

                public IntToDoubleColumnGenerator(TGenerator generator)
                {
                    this.generator = generator;
                }

                public double this[int value]
                {
                    get
                    {
                        return (double)this.generator[value];
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
                        return new IntToDoubleColumnGenerator<TGenerator>(
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
