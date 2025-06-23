// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Landmarks;

namespace Microsoft.Performance.SDK.Processing.Landmarks;

public interface ILandmarkProvider<T>
    where T : Landmark
{
    LandmarkCollection<T> Landmarks { get; }
}

public abstract class LandmarkCollection<T>
    : IReadOnlyList<T> where T : Landmark
{
    /// <summary>
    ///     This event is raised when the landmarks are updated.
    /// </summary>
    public abstract event EventHandler? LandmarksUpdated;

    /// <inheritdoc cref="IReadOnlyList{T}.this[int]" />
    public abstract T this[int index] { get; }

    /// <inheritdoc cref="IReadOnlyCollection{T}.Count" />/>
    public abstract int Count { get; }

    /// <inheritdoc cref="IReadOnlyList{T}.GetEnumerator" />
    public abstract IEnumerator<T> GetEnumerator();

    /// <inheritdoc cref="IEnumerable.GetEnumerator" />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class LandmarkProvider<T>
    : ILandmarkProvider<T>,
      IDisposable
    where T : Landmark
{
    private readonly ObservableCollection<T> landmarks = [];
    private readonly ProviderLandmarkDataSource dataSource;
    private bool disposedValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LandmarkProvider{T}"/> class.
    /// </summary>
    public LandmarkProvider()
    {
        this.dataSource = new ProviderLandmarkDataSource(landmarks);
    }

    /// <inheritdoc/>
    public LandmarkCollection<T> Landmarks => this.dataSource;

    /// <summary>
    ///     Adds a landmark to the provider's collection of landmarks.
    /// </summary>
    /// <param name="landmark">
    ///     The landmark to be added.
    /// </param>
    /// <param name="cancellationToken">
    ///     The cancellation token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    ///     The task that represents the asynchronous operation.
    /// </returns>
    public async Task ProvideLandmarkAsync(T landmark, CancellationToken cancellationToken)
    {
        Guard.NotNull(landmark, nameof(landmark));

        await Task.Run(() =>
        {
            this.landmarks.Add(landmark);
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.dataSource.Dispose();
            }

            this.disposedValue = true;
        }
    }

    private sealed class ProviderLandmarkDataSource
        : LandmarkCollection<T>,
          IDisposable
    {
        private readonly ObservableCollection<T> landmarks;
        private bool disposedValue;

        /// <inheritdoc />
        public override event EventHandler? LandmarksUpdated;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProviderLandmarkDataSource{T}"/> class.
        /// </summary>
        /// <param name="landmarks">
        ///     A collection of landmarks to be used as the data source.
        /// </param>
        public ProviderLandmarkDataSource(ObservableCollection<T> landmarks)
        {
            Guard.NotNull(landmarks, nameof(landmarks));

            this.landmarks = landmarks;
            this.landmarks.CollectionChanged += Landmarks_CollectionChanged;
        }

        /// <inheritdoc />
        public override T this[int index] => ((IReadOnlyList<T>)landmarks)[index];

        /// <inheritdoc />
        public override int Count => ((IReadOnlyCollection<T>)landmarks).Count;

        /// <inheritdoc />
        public override IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)landmarks).GetEnumerator();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Landmarks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.LandmarksUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.landmarks.CollectionChanged -= Landmarks_CollectionChanged;
                }

                this.disposedValue = true;
            }
        }
    }
}