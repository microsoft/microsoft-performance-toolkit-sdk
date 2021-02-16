// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime;
using NuGet.Versioning;

namespace Microsoft.Performance.Testing.SDK
{
    public sealed class ConfigurableVersionChecker
        : VersionChecker
    {
        private SemanticVersion sdkOverride;
        private SemanticVersion lowestSupportedSdkOverride;

        public ConfigurableVersionChecker()
        {
        }

        public SemanticVersion SdkSetter
        {
            get
            {
                return this.sdkOverride;
            }
            set
            {
                this.sdkOverride = value;
            }
        }

        public override SemanticVersion Sdk
        {
            get
            {
                if (this.sdkOverride != null)
                {
                    return this.sdkOverride;
                }

                return base.Sdk;
            }
        }

        public SemanticVersion LowestSupportedSdkSetter
        {
            get
            {
                return this.lowestSupportedSdkOverride;
            }
            set
            {
                this.lowestSupportedSdkOverride = value;
            }
        }

        public override SemanticVersion LowestSupportedSdk
        {
            get
            {
                if (this.lowestSupportedSdkOverride != null)
                {
                    return this.lowestSupportedSdkOverride;
                }

                return base.LowestSupportedSdk;
            }
        }
    }
}
