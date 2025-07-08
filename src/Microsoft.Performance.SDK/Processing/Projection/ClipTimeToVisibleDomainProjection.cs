// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

// Note: this namespace is intentional. From the developers perspective, there isn't a subfolder for partial classes.
namespace Microsoft.Performance.SDK.Processing
{
    public static partial class Projection
    {
        /// <summary>
        ///     Creates instances of <see cref="IProjection{TSource, TResult}"/> that are sensitive to the current visible domain.
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
            ///     Create a <see cref="ResourceTimeRange"/> projection that is clipped to a
            ///     <see cref="VisibleDomainRegionContainer"/>.
            ///     See also <seealso cref="IVisibleDomainSensitiveProjection"/>.
            /// </summary>
            /// <param name="resourceTimeRangeProjection">
            ///     Original projection.
            /// </param>
            /// <returns>
            ///     A visible domain sensitive projection.
            /// </returns>
            public static IProjection<int, ResourceTimeRange> Create(IProjection<int, ResourceTimeRange> resourceTimeRangeProjection)
            {
                Guard.NotNull(resourceTimeRangeProjection, nameof(resourceTimeRangeProjection));

                var typeArgs = new[]
                {
                    resourceTimeRangeProjection.GetType(),
                };

                var constructorArgs = new object[]
                {
                    resourceTimeRangeProjection,
                };

                var type = typeof(ClipTimeToVisibleResourceTimeRangeDomainColumnGenerator<>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, ResourceTimeRange>)instance;
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

            /// <summary>
            ///     Creates a new percent projection that is defined by the current viewport.
            /// </summary>
            /// <param name="resourceTimeRangeColumn">
            ///     Original projection.
            /// </param>
            /// <returns>
            ///     A viewport sensitive projection.
            /// </returns>
            public static IProjection<int, ResourceTimeRange> CreatePercent(IProjection<int, ResourceTimeRange> resourceTimeRangeColumn)
            {
                Guard.NotNull(resourceTimeRangeColumn, nameof(resourceTimeRangeColumn));

                var typeArgs = new[]
                {
                    resourceTimeRangeColumn.GetType(),
                };

                var constructorArgs = new object[]
                {
                    resourceTimeRangeColumn,
                };

                var type = typeof(ClipTimeToVisibleResourceTimeRangePercentColumnGenerator<>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, ResourceTimeRange>)instance;
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

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            private struct ClipTimeToVisibleResourceTimeRangeDomainColumnGenerator<TGenerator>
                : IProjection<int, ResourceTimeRange>,
                  IVisibleDomainSensitiveProjection
                  where TGenerator : IProjection<int, ResourceTimeRange>
            {
                internal TGenerator Generator { get; }

                internal VisibleDomainRegionContainer VisibleDomainContainer { get; }

                public ClipTimeToVisibleResourceTimeRangeDomainColumnGenerator(TGenerator resourceTimeRangeGenerator)
                {
                    this.Generator = resourceTimeRangeGenerator;
                    this.VisibleDomainContainer = new VisibleDomainRegionContainer();
                }

                // IProjection<int, Timestamp>
                public ResourceTimeRange this[int value]
                {
                    get
                    {
                        TimeRange visibleDomain = this.VisibleDomainContainer.VisibleDomain;

                        ResourceTimeRange resourceTimeRange = this.Generator[value];

                        resourceTimeRange.StartTime = ClipTimestampToVisibleDomain(resourceTimeRange.StartTime, visibleDomain);
                        resourceTimeRange.EndTime = ClipTimestampToVisibleDomain(resourceTimeRange.EndTime, visibleDomain);

                        return resourceTimeRange;
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
                        return typeof(ResourceTimeRange);
                    }
                }

                public object Clone()
                {
                    var result = new ClipTimeToVisibleResourceTimeRangeDomainColumnGenerator<TGenerator>(
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
                {
                    var internalGenerator = new ClipTimeToVisibleTimeRangeDomainColumnGenerator<TGenerator>(timeRangeGenerator);
                    this.timeRangeColumnGenerator = internalGenerator;

                    this.customFormatter =
                        new ClipTimeToVisibleTimeRangePercentFormatProvider(() => internalGenerator.VisibleDomainContainer.VisibleDomain.Duration);
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
                        VisibleDomainSensitiveProjection.CloneIfVisibleDomainSensitive(this.timeRangeColumnGenerator.Generator));
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

            private struct ClipTimeToVisibleResourceTimeRangePercentColumnGenerator<TGenerator>
                : IProjection<int, ResourceTimeRange>,
                  IVisibleDomainSensitiveProjection,
                  IFormatProvider
                  where TGenerator : IProjection<int, ResourceTimeRange>
            {
                private readonly ClipTimeToVisibleResourceTimeRangeDomainColumnGenerator<TGenerator> resourceTimeRangeColumnGenerator;

                // IFormatProvider returns an object - cannot return 'this' struct. 
                // So implement ICustomFormatter in a private class and return that object.
                //
                private readonly ClipTimeToVisibleResourceTimeRangePercentFormatProvider customFormatter;

                public ClipTimeToVisibleResourceTimeRangePercentColumnGenerator(TGenerator timeRangeGenerator)
                {
                    var internalGenerator = new ClipTimeToVisibleResourceTimeRangeDomainColumnGenerator<TGenerator>(timeRangeGenerator);
                    this.resourceTimeRangeColumnGenerator = internalGenerator;

                    this.customFormatter =
                        new ClipTimeToVisibleResourceTimeRangePercentFormatProvider(() => internalGenerator.VisibleDomainContainer.VisibleDomain.Duration);
                }

                public ResourceTimeRange this[int value] => this.resourceTimeRangeColumnGenerator[value];

                public Type SourceType => typeof(int);

                public Type ResultType => typeof(ResourceTimeRange);

                public bool DependsOnVisibleDomain => true;

                public object GetFormat(Type formatType)
                {
                    return (formatType == typeof(ResourceTimeRange) ||
                            formatType == typeof(TimestampDelta)) ? this.customFormatter : null;
                }

                public object Clone()
                {
                    var result = new ClipTimeToVisibleResourceTimeRangePercentColumnGenerator<TGenerator>(
                        VisibleDomainSensitiveProjection.CloneIfVisibleDomainSensitive(this.resourceTimeRangeColumnGenerator.Generator));
                    return result;
                }

                public bool NotifyVisibleDomainChanged(IVisibleDomainRegion visibleDomain)
                {
                    this.resourceTimeRangeColumnGenerator.NotifyVisibleDomainChanged(visibleDomain);
                    return true;
                }

                private class ClipTimeToVisibleResourceTimeRangePercentFormatProvider
                    : ICustomFormatter
                {
                    private readonly Func<TimestampDelta> getVisibleDomainDuration;

                    public ClipTimeToVisibleResourceTimeRangePercentFormatProvider(Func<TimestampDelta> getVisibleDomainDuration)
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
                        if (arg is ResourceTimeRange)
                        {
                            numerator = ((ResourceTimeRange)arg).Duration;
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
