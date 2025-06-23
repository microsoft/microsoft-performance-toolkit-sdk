// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Landmarks;

namespace Microsoft.Performance.SDK.Processing.Landmarks;

//public interface ILandmarkProvider
//{
//    Task ProvideLandmarkAsync(Landmark landmark, CancellationToken cancellationToken);
//}

public interface ILandmarkProvider<in T>
    where T : Landmark
{
    Task ProvideLandmarkAsync(T landmark, CancellationToken cancellationToken);
}

//public interface ILandmarkConsumer
//{
//    Task ConsumeLandmarkAsync(Landmark landmark, CancellationToken cancellationToken);
//}

public interface ILandmarkConsumer<in T>
    where T : Landmark
{
    Task ConsumeLandmarkAsync(T landmark, CancellationToken cancellationToken);
}

public class LandmarkProvider<T>
    : ILandmarkProvider<T>
    where T : Landmark
{

    public LandmarkProvider()
    {
    }

    public async Task ProvideLandmarkAsync(T landmark, CancellationToken cancellationToken)
    {
    }
}