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
            ///     <see cref="VisibleDomainRegionContainer"/>.
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
            ///     <see cref="VisibleDomainRegionContainer"/>.
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
            public static IProjection<int, TimeRange> CreatePercent(IProjection<int, TimeRange> timeRangeColumn)
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
                return (IProjection<int, TimeRange>)instance;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            private struct ClipTimeToVisibleTimestampDomainColumnGenerator<TGenerator>
                : IProjection<int, Timestamp>,
                  IVisibleDomainSensitiveProjection
                  where TGenerator : IProjection<int, Timestamp>
            {
                private readonly TGenerator generator;
                private readonly VisibleDomainRegionContainer visibleDomainContainer;

                public ClipTimeToVisibleTimestampDomainColumnGenerator(TGenerator timestampGenerator)
                    : this(timestampGenerator, new VisibleDomainRegionContainer())
                {
                }

                private ClipTimeToVisibleTimestampDomainColumnGenerator(TGenerator timestampGenerator, VisibleDomainRegionContainer visibleDomainContainer)
                {
                    this.generator = timestampGenerator;
                    this.visibleDomainContainer = visibleDomainContainer;
                }

                // IProjection<int, Timestamp>
                public Timestamp this[int value]
                {
                    get
                    {
                        Timestamp timestamp = this.generator[value];

                        return ClipTimestampToVisibleDomain(timestamp, this.visibleDomainContainer.VisibleDomain);
                    }
                }

                public Type SourceType => typeof(int);

                public Type ResultType => typeof(Timestamp);

                public object Clone()
                {
                    var result = new ClipTimeToVisibleTimestampDomainColumnGenerator<TGenerator>(
                        VisibleDomainSensitiveProjection.CloneIfVisibleDomainSensitive(this.generator), this.visibleDomainContainer);
                    return result;
                }

                public bool NotifyVisibleDomainChanged(IVisibleDomainRegion visibleDomain)
                {
                    this.visibleDomainContainer.VisibleDomainRegion = visibleDomain;
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
                    : this(timestampGenerator, new VisibleDomainRegionContainer())
                {
                }

                private ClipTimeToVisibleTimeRangeDomainColumnGenerator(TGenerator timestampGenerator, VisibleDomainRegionContainer visibleDomainRegionContainer)
                {
                    this.Generator = timestampGenerator;
                    this.VisibleDomainContainer = visibleDomainRegionContainer;
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
                        VisibleDomainSensitiveProjection.CloneIfVisibleDomainSensitive(this.Generator), this.VisibleDomainContainer);
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
                : IProjection<int, TimeRange>,
                  IVisibleDomainSensitiveProjection,
                  IFormatProvider
                  where TGenerator : IProjection<int, TimeRange>
            {
                private readonly ClipTimeToVisibleTimeRangeDomainColumnGenerator<TGenerator> timeRangeColumnGenerator;

                // IFormatProvider returns an object - cannot return 'this' struct. 
                // So implement ICustomFormatter in a private class and return that object.
                //
                private readonly ClipTimeToVisibleTimeRangePercentFormatProvider customFormatter;

                public ClipTimeToVisibleTimeRangePercentColumnGenerator(TGenerator timeRangeGenerator)
                    : this(new ClipTimeToVisibleTimeRangeDomainColumnGenerator<TGenerator>(timeRangeGenerator))
                {
                }

                private ClipTimeToVisibleTimeRangePercentColumnGenerator(ClipTimeToVisibleTimeRangeDomainColumnGenerator<TGenerator> timeRangeColumnGenerator)
                {
                    this.timeRangeColumnGenerator = timeRangeColumnGenerator;

                    this.customFormatter =
                        new ClipTimeToVisibleTimeRangePercentFormatProvider(() => timeRangeColumnGenerator.VisibleDomainContainer.VisibleDomain.Duration);
                }

                public TimeRange this[int value] => this.timeRangeColumnGenerator[value];

                public Type SourceType => typeof(int);

                public Type ResultType => typeof(TimeRange);

                public bool DependsOnVisibleDomain => true;

                public object GetFormat(Type formatType)
                {
                    return (formatType == typeof(TimeRange) ||
                            formatType == typeof(TimestampDelta)) ? this.customFormatter : null;
                }

                public object Clone()
                {
                    var result = new ClipTimeToVisibleTimeRangePercentColumnGenerator<TGenerator>(
                        VisibleDomainSensitiveProjection.CloneIfVisibleDomainSensitive(this.timeRangeColumnGenerator));
                    return result;
                }

                public bool NotifyVisibleDomainChanged(IVisibleDomainRegion visibleDomain)
                {
                    this.timeRangeColumnGenerator.NotifyVisibleDomainChanged(visibleDomain);
                    return true;
                }

                private class ClipTimeToVisibleTimeRangePercentFormatProvider
                    : ICustomFormatter
                {
                    private readonly Func<TimestampDelta> getVisibleDomainDuration;

                    public ClipTimeToVisibleTimeRangePercentFormatProvider(Func<TimestampDelta> getVisibleDomainDuration)
                    {
                        Guard.NotNull(getVisibleDomainDuration, nameof(getVisibleDomainDuration));

                        this.getVisibleDomainDuration = getVisibleDomainDuration;
                    }

                    public string Format(string format, object arg, IFormatProvider formatProvider)
                    {
                        if (arg == null)
                        {
                            return string.Empty;
                        }

                        TimestampDelta numerator;
                        if (arg is TimeRange)
                        {
                            numerator = ((TimeRange)arg).Duration;
                        }
                        else if (arg is TimestampDelta)
                        {
                            numerator = (TimestampDelta)arg;
                        }
                        else
                        {
                            throw new FormatException();
                        }

                        TimestampDelta visibleDomainDuration = getVisibleDomainDuration();
                        double percent = (visibleDomainDuration != TimestampDelta.Zero) ?
                            (100.0 * ((double)numerator.ToNanoseconds) / (visibleDomainDuration.ToNanoseconds)) :
                            100.0;

                        return percent.ToString(format, formatProvider);
                    }
                }
            }
        }
    }
}
