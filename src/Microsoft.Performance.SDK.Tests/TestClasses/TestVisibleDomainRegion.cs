// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    internal class TestVisibleDomainRegion
        : IVisibleDomainRegion
    {
        private int sentinel;

        public TestVisibleDomainRegion(int sentinel, TimeRange domain)
        {
            this.sentinel = sentinel;
            this.Domain = domain;
        }

        public TimeRange Domain
        {
            get;
            private set;
        }

        public TAggregate AggregateVisibleRows<T, TAggregate>(IProjection<int, T> projection, AggregationMode aggregationMode)
        {
            switch (typeof(TAggregate))
            {
                case Type doubleType when doubleType == typeof(double):
                    return (TAggregate)(object)(double)this.sentinel;
                case Type deltaType when deltaType == typeof(TimestampDelta):
                    return (TAggregate)(object)TimestampDelta.FromSeconds(this.sentinel);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TestVisibleDomainRegion"/> and notifies the given
        ///     <paramref name="projection"/> of the new region.
        /// </summary>
        /// <param name="projection">
        ///     The <see cref="IProjectionDescription"/> to notify the new region to.
        /// </param>
        /// <param name="sentinel">
        ///     The sentinel value to use to identify the created <see cref="TestVisibleDomainRegion"/>
        /// </param>
        public static void UpdateVisibleDomainRegion(IProjectionDescription projection, int sentinel)
        {
            UpdateVisibleDomainRegion(projection, sentinel, new TimeRange(Timestamp.FromMicroseconds(sentinel), Timestamp.FromMicroseconds(sentinel + 100)));
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TestVisibleDomainRegion"/> and notifies the given
        ///     <paramref name="projection"/> of the new region.
        /// </summary>
        /// <param name="projection">
        ///     The <see cref="IProjectionDescription"/> to notify the new region to.
        /// </param>
        /// <param name="sentinel">
        ///     The sentinel value to use to identify the created <see cref="TestVisibleDomainRegion"/>
        /// </param>
        public static void UpdateVisibleDomainRegion(IProjectionDescription projection, int sentinel, TimeRange timeRange)
        {
            var region = new TestVisibleDomainRegion(sentinel, timeRange);
            VisibleDomainSensitiveProjection.NotifyVisibleDomainChanged(projection, region);
        }
    }
}
