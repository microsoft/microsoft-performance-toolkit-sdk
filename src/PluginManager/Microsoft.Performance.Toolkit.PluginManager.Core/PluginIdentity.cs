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
        public PluginIdentity(string name, Version version)
            : this(name, version, null)
        {

        }

        public PluginIdentity(string name, Version version, string group)
        {
            this.Group = group;
            this.Id = name;
            this.Version = version;
        }

        /// <summary>
        /// The group this plugin belongs to
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// The identifer of the plugin
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Plugin Version
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

            return string.Equals(a.Group, b.Group, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.Id, b.Id, StringComparison.OrdinalIgnoreCase)
                && Version.Equals(a.Version, b.Version);
        }
    }
}
