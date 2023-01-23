// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core
{
    /// <summary>
    /// Represents the core identity of a plugin
    /// </summary>
    public class PluginIdentity : IEquatable<PluginIdentity>
    {
        public PluginIdentity(string id, Version version)
        {
            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        /// The identifer of this plugin
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The version of this plugin
        /// </summary>
        public Version Version { get; }

        public bool Equals(PluginIdentity other)
        {
            return Equals(this, other);
        }

        public static bool Equals(PluginIdentity a, PluginIdentity b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return string.Equals(a.Id, b.Id, StringComparison.OrdinalIgnoreCase)
                && Version.Equals(a.Version, b.Version);
        }
    }
}
