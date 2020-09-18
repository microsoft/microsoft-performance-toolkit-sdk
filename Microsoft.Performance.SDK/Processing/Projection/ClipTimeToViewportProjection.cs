// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

// Note: this namespace is intentional. From the developers perspective, there isn't a subfolder for partial classes.
namespace Microsoft.Performance.SDK.Processing
{
    public static partial class Projection
    {
        public static class ClipTimeToViewport
        {
            /// <summary>
            ///     Create a <see cref="Timestamp"/> projection that is clipped to a
            ///     <see cref="VisibleTableRegionContainer"/>.
            ///     See also <seealso cref="IViewportSensitiveProjection"/>.
            /// </summary>
            /// <param name="timestampProjection">
            ///     Original projection.
            /// </param>
            /// <returns>
            ///     A viewport sensitive projection.
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

                var type = typeof(ClipTimeToViewportTimestampColumnGenerator<>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, Timestamp>)instance;
            }

            /// <summary>
            ///     Create a <see cref="TimeRange"/> projection that is clipped to a
            ///     <see cref="VisibleTableRegionContainer"/>.
            ///     See also <seealso cref="IViewportSensitiveProjection"/>.
            /// </summary>
            /// <param name="timeRangeProjection">
            ///     Original projection.
            /// </param>
            /// <returns>
            ///     A viewport sensitive projection.
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

                var type = typeof(ClipTimeToViewportTimeRangeColumnGenerator<>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, TimeRange>)instance;
            }

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

                var type = typeof(ClipTimeToViewportTimeRangePercentColumnGenerator<>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, TimeRange>)instance;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            private struct ClipTimeToViewportTimestampColumnGenerator<TGenerator>
                : IProjection<int, Timestamp>,
                  IViewportSensitiveProjection
                  where TGenerator : IProjection<int, Timestamp>
            {
                private readonly TGenerator generator;
                private readonly VisibleTableRegionContainer viewport;

                public ClipTimeToViewportTimestampColumnGenerator(TGenerator timestampGenerator)
                {
                    this.generator = timestampGenerator;
                    this.viewport = new VisibleTableRegionContainer();
                }

                // IProjection<int, Timestamp>
                public Timestamp this[int value]
                {
                    get
                    {
                        Timestamp timestamp = this.generator[value];

                        return ClipTimestampToViewport(timestamp, this.viewport.Viewport);
                    }
                }

                public Type SourceType => typeof(int);

                public Type ResultType => typeof(Timestamp);

                public object Clone()
                {
                    var result = new ClipTimeToViewportTimestampColumnGenerator<TGenerator>(
                        ViewportSensitiveProjection.CloneIfViewportSensitive(this.generator));
                    return result;
                }

                public bool NotifyViewportChanged(IVisibleTableRegion viewport)
                {
                    this.viewport.VisibleTableRegion = viewport;
                    ViewportSensitiveProjection.NotifyViewportChanged(this.generator, viewport);
                    return true;
                }

                public bool DependsOnViewport => true;
            }

            private static Timestamp ClipTimestampToViewport(Timestamp timestamp, TimeRange viewport)
            {
                if (timestamp < viewport.StartTime)
                {
                    return viewport.StartTime;
                }
                else if (timestamp > viewport.EndTime)
                {
                    return viewport.EndTime;
                }

                return timestamp;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            private struct ClipTimeToViewportTimeRangeColumnGenerator<TGenerator>
                : IProjection<int, TimeRange>,
                  IViewportSensitiveProjection
                  where TGenerator : IProjection<int, TimeRange>
            {
                internal TGenerator Generator { get; }

                internal VisibleTableRegionContainer ViewPortContainer { get; }

                public ClipTimeToViewportTimeRangeColumnGenerator(TGenerator timestampGenerator)
                {
                    this.Generator = timestampGenerator;
                    this.ViewPortContainer = new VisibleTableRegionContainer();
                }

                // IProjection<int, Timestamp>
                public TimeRange this[int value]
                {
                    get
                    {
                        TimeRange viewport = this.ViewPortContainer.Viewport;

                        TimeRange timeRange = this.Generator[value];

                        timeRange.StartTime = ClipTimestampToViewport(timeRange.StartTime, viewport);
                        timeRange.EndTime = ClipTimestampToViewport(timeRange.EndTime, viewport);

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
                    var result = new ClipTimeToViewportTimeRangeColumnGenerator<TGenerator>(
                        ViewportSensitiveProjection.CloneIfViewportSensitive(this.Generator));
                    return result;
                }


                public bool NotifyViewportChanged(IVisibleTableRegion viewport)
                {
                    this.ViewPortContainer.VisibleTableRegion = viewport;
                    ViewportSensitiveProjection.NotifyViewportChanged(this.Generator, viewport);
                    return true;
                }

                public bool DependsOnViewport => true;
            }

            private struct ClipTimeToViewportTimeRangePercentColumnGenerator<TGenerator>
                : IProjection<int, TimeRange>,
                  IViewportSensitiveProjection,
                  IFormatProvider
                  where TGenerator : IProjection<int, TimeRange>
            {
                private readonly ClipTimeToViewportTimeRangeColumnGenerator<TGenerator> timeRangeColumnGenerator;

                // IFormatProvider returns an object - cannot return 'this' struct. 
                // So implement ICustomFormatter in a private class and return that object.
                //
                private readonly ClipTimeToViewportTimeRangePercentFormatProvider customFormatter;

                private ClipTimeToViewportTimeRangePercentColumnGenerator(TGenerator timeRangeGenerator)
                {
                    var internalGenerator = new ClipTimeToViewportTimeRangeColumnGenerator<TGenerator>(timeRangeGenerator);
                    this.timeRangeColumnGenerator = internalGenerator;

                    this.customFormatter =
                        new ClipTimeToViewportTimeRangePercentFormatProvider(() => internalGenerator.ViewPortContainer.Viewport.Duration);
                }

                public TimeRange this[int value] => this.timeRangeColumnGenerator[value];

                public Type SourceType => typeof(int);

                public Type ResultType => typeof(TimeRange);

                public bool DependsOnViewport => true;

                public object GetFormat(Type formatType)
                {
                    return (formatType == typeof(TimeRange) ||
                            formatType == typeof(TimestampDelta)) ? this.customFormatter : null;
                }

                public object Clone()
                {
                    var result = new ClipTimeToViewportTimeRangePercentColumnGenerator<TGenerator>(
                        ViewportSensitiveProjection.CloneIfViewportSensitive(this.timeRangeColumnGenerator.Generator));
                    return result;
                }

                public bool NotifyViewportChanged(IVisibleTableRegion viewport)
                {
                    this.timeRangeColumnGenerator.NotifyViewportChanged(viewport);
                    return true;
                }

                private class ClipTimeToViewportTimeRangePercentFormatProvider
                    : ICustomFormatter
                {
                    private readonly Func<TimestampDelta> getViewportDuration;

                    public ClipTimeToViewportTimeRangePercentFormatProvider(Func<TimestampDelta> getViewportDuration)
                    {
                        Guard.NotNull(getViewportDuration, nameof(getViewportDuration));

                        this.getViewportDuration = getViewportDuration;
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

                        TimestampDelta viewportDuration = getViewportDuration();
                        double percent = (viewportDuration != TimestampDelta.Zero) ?
                            (100.0 * ((double)numerator.ToNanoseconds) / (viewportDuration.ToNanoseconds)) :
                            100.0;

                        return percent.ToString(format, formatProvider);
                    }
                }
            }
        }
    }
}
