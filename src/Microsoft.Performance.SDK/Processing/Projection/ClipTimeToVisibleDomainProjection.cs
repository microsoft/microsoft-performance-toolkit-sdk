// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

// Note: this namespace is intentional. From the developers perspective, there isn't a subfolder for partial classes.
namespace Microsoft.Performance.SDK.Processing
{
    public static partial class Projection
    {
        /// <summary>
        ///     Creates intances of <see cref="IProjection{TSource, TResult}"/> that are sensitive to the current visible domain.
        /// </summary>
        public static class ClipTimeToVisibleDomain
        {
            /// <summary>
            ///     Create a <see cref="Timestamp"/> projection that is clipped to a
            ///     <see cref="IVisibleDomainRegion"/>.
            ///     See also <seealso cref="IVisibleDomainSensitiveProjection"/>.
            /// </summary>
            /// <param name="timestampProjection">
            ///     Original projection.
            /// </param>
            /// <returns>
            ///     A visible domain sensitive projection.
            /// </returns>
            public static IProjection<int, Timestamp> Create(IProjection<int, Timestamp> timestampProjection)
            {
                Guard.NotNull(timestampProjection, nameof(timestampProjection));

                var typeArgs = new[]
                {
                    timestampProjection.GetType(),
                };

                var constructorArgs = new object[]
                {
                    timestampProjection,
                };

                var type = typeof(ClipTimeToVisibleTimestampDomainColumnGenerator<>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, Timestamp>)instance;
            }

            /// <summary>
            ///     Create a <see cref="TimeRange"/> projection that is clipped to a
            ///     <see cref="IVisibleDomainRegion"/>.
            ///     See also <seealso cref="IVisibleDomainSensitiveProjection"/>.
            /// </summary>
            /// <param name="timeRangeProjection">
            ///     Original projection.
            /// </param>
            /// <returns>
            ///     A visible domain sensitive projection.
            /// </returns>
            public static IProjection<int, TimeRange> Create(IProjection<int, TimeRange> timeRangeProjection)
            {
                Guard.NotNull(timeRangeProjection, nameof(timeRangeProjection));

                var typeArgs = new[]
                {
                    timeRangeProjection.GetType(),
                };

                var constructorArgs = new object[]
                {
                    timeRangeProjection,
                };

                var type = typeof(ClipTimeToVisibleTimeRangeDomainColumnGenerator<>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, TimeRange>)instance;
            }

            /// <summary>
            ///     Creates a new percent projection that is defined by the current viewport.
            /// </summary>
            /// <param name="timeRangeColumn">
            ///     Original projection.
            /// </param>
            /// <returns>
            ///     A viewport sensitive projection.
            /// </returns>
            [Obsolete("This method returns incorrect data and will be removed in version 2.0. For correct data, please use CreatePercentDouble.")]
            public static IProjection<int, TimeRange> CreatePercent(IProjection<int, TimeRange> timeRangeColumn)
            {
                return Projection.Constant(TimeRange.Zero);
            }

            /// <summary>
            ///     Creates a new projection that maps the portion of a given <see cref="TimeRange"/>
            ///     inside the current <see cref="IVisibleDomainRegion"/> to its percent relative to the entire <see cref="TimeRange"/>
            ///     of the <see cref="IVisibleDomainRegion"/>.
            /// </summary>
            /// <remarks>
            ///     For example, with a <see cref="IVisibleDomainRegion"/> of [50, 100):
            ///     <list type="bullet">
            ///         <item>
            ///             [50, 100) projects to 100.0 since it overlaps the entire <see cref="IVisibleDomainRegion"/>.
            ///         </item>
            ///         <item>
            ///             [50, 75) projects to 50.0 since it overlaps half of the <see cref="IVisibleDomainRegion"/>.
            ///         </item>
            ///         <item>
            ///             [0, 75) projects to 50.0 since its portion inside the visible domain is [50, 75),
            ///             which overlaps half of the <see cref="IVisibleDomainRegion"/>.
            ///         </item>
            ///     </list>
            /// </remarks>
            /// <param name="timeRangeColumn">
            ///     The original <see cref="TimeRange"/> projection.
            /// </param>
            /// <returns>
            ///     The created projection.
            /// </returns>
            public static IProjection<int, double> CreatePercentDouble(IProjection<int, TimeRange> timeRangeColumn)
            {
                Guard.NotNull(timeRangeColumn, nameof(timeRangeColumn));

                var typeArgs = new[]
                {
                    timeRangeColumn.GetType(),
                };

                var constructorArgs = new object[]
                {
                    timeRangeColumn,
                };

                var type = typeof(ClipTimeToVisibleTimeRangePercentColumnGenerator<>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, double>)instance;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            private struct ClipTimeToVisibleTimestampDomainColumnGenerator<TGenerator>
                : IProjection<int, Timestamp>,
                  IVisibleDomainSensitiveProjection
                  where TGenerator : IProjection<int, Timestamp>
            {
                private readonly TGenerator generator;
                private readonly VisibleDomainRegionContainer visibleDomain;

                public ClipTimeToVisibleTimestampDomainColumnGenerator(TGenerator timestampGenerator)
                {
                    this.generator = timestampGenerator;
                    this.visibleDomain = new VisibleDomainRegionContainer();
                }

                // IProjection<int, Timestamp>
                public Timestamp this[int value]
                {
                    get
                    {
                        Timestamp timestamp = this.generator[value];

                        return ClipTimestampToVisibleDomain(timestamp, this.visibleDomain.VisibleDomain);
                    }
                }

                public Type SourceType => typeof(int);

                public Type ResultType => typeof(Timestamp);

                public object Clone()
                {
                    var result = new ClipTimeToVisibleTimestampDomainColumnGenerator<TGenerator>(
                        VisibleDomainSensitiveProjection.CloneIfVisibleDomainSensitive(this.generator));
                    return result;
                }

                public bool NotifyVisibleDomainChanged(IVisibleDomainRegion visibleDomain)
                {
                    this.visibleDomain.VisibleDomainRegion = visibleDomain;
                    VisibleDomainSensitiveProjection.NotifyVisibleDomainChanged(this.generator, visibleDomain);
                    return true;
                }

                public bool DependsOnVisibleDomain => true;
            }

            private static Timestamp ClipTimestampToVisibleDomain(Timestamp timestamp, TimeRange visibleDomain)
            {
                if (timestamp < visibleDomain.StartTime)
                {
                    return visibleDomain.StartTime;
                }
                else if (timestamp > visibleDomain.EndTime)
                {
                    return visibleDomain.EndTime;
                }

                return timestamp;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            private struct ClipTimeToVisibleTimeRangeDomainColumnGenerator<TGenerator>
                : IProjection<int, TimeRange>,
                  IVisibleDomainSensitiveProjection
                  where TGenerator : IProjection<int, TimeRange>
            {
                internal TGenerator Generator { get; }

                internal VisibleDomainRegionContainer VisibleDomainContainer { get; }

                public ClipTimeToVisibleTimeRangeDomainColumnGenerator(TGenerator timestampGenerator)
                {
                    this.Generator = timestampGenerator;
                    this.VisibleDomainContainer = new VisibleDomainRegionContainer();
                }

                // IProjection<int, Timestamp>
                public TimeRange this[int value]
                {
                    get
                    {
                        TimeRange visibleDomain = this.VisibleDomainContainer.VisibleDomain;

                        TimeRange timeRange = this.Generator[value];

                        timeRange.StartTime = ClipTimestampToVisibleDomain(timeRange.StartTime, visibleDomain);
                        timeRange.EndTime = ClipTimestampToVisibleDomain(timeRange.EndTime, visibleDomain);

                        return timeRange;
                    }
                }

                public Type SourceType
                {
                    get
                    {
                        return typeof(int);
                    }
                }

                public Type ResultType
                {
                    get
                    {
                        return typeof(TimeRange);
                    }
                }

                public object Clone()
                {
                    var result = new ClipTimeToVisibleTimeRangeDomainColumnGenerator<TGenerator>(
                        VisibleDomainSensitiveProjection.CloneIfVisibleDomainSensitive(this.Generator));
                    return result;
                }


                public bool NotifyVisibleDomainChanged(IVisibleDomainRegion visibleDomain)
                {
                    this.VisibleDomainContainer.VisibleDomainRegion = visibleDomain;
                    VisibleDomainSensitiveProjection.NotifyVisibleDomainChanged(this.Generator, visibleDomain);
                    return true;
                }

                public bool DependsOnVisibleDomain => true;
            }

            private struct ClipTimeToVisibleTimeRangePercentColumnGenerator<TGenerator>
                : IProjection<int, double>,
                  IVisibleDomainSensitiveProjection
                  where TGenerator : IProjection<int, TimeRange>
            {
                private readonly ClipTimeToVisibleTimeRangeDomainColumnGenerator<TGenerator> timeRangeColumnGenerator;

                public ClipTimeToVisibleTimeRangePercentColumnGenerator(TGenerator timeRangeGenerator)
                {
                    this.timeRangeColumnGenerator = new ClipTimeToVisibleTimeRangeDomainColumnGenerator<TGenerator>(timeRangeGenerator);
                }

                public double this[int value]
                {
                    get
                    {
                        var numerator = this.timeRangeColumnGenerator[value].Duration;
                        var denominator = this.timeRangeColumnGenerator.VisibleDomainContainer.VisibleDomain.Duration;

                        return (denominator != TimestampDelta.Zero) ?
                            (100.0 * ((double)numerator.ToNanoseconds) / (denominator.ToNanoseconds)) :
                            100.0;
                    }
                }

                public Type SourceType => typeof(int);

                public Type ResultType => typeof(double);

                public bool DependsOnVisibleDomain => true;

                public object Clone()
                {
                    var result = new ClipTimeToVisibleTimeRangePercentColumnGenerator<TGenerator>(
                        VisibleDomainSensitiveProjection.CloneIfVisibleDomainSensitive(this.timeRangeColumnGenerator.Generator));
                    return result;
                }

                public bool NotifyVisibleDomainChanged(IVisibleDomainRegion visibleDomain)
                {
                    this.timeRangeColumnGenerator.NotifyVisibleDomainChanged(visibleDomain);
                    return true;
                }
            }
        }
    }
}
