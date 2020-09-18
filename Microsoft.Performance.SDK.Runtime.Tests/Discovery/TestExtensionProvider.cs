// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.SDK.Runtime.Tests.Discovery
{
    public class TestExtensionProvider
        : IExtensionTypeProvider
    {
        public void RegisterTypeConsumer(IExtensionTypeObserver observer)
        {
            this.Observer = observer;
        }

        public IExtensionTypeObserver Observer { get; private set; }
    }
}
