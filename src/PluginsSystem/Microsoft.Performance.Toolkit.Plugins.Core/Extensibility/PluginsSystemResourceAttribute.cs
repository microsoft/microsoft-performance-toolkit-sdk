﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Extensibility
{
    /// <summary>
    ///     This attribute is used to mark a concrete class as an <see cref="IPluginsSystemResource"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PluginsSystemResourceAttribute
        : Attribute
    {
        public PluginsSystemResourceAttribute(string guid)
        {
            Guard.NotNullOrWhiteSpace(guid, nameof(guid));

            this.Guid = Guid.Parse(guid);

            if (this.Guid == default(Guid))
            {
                throw new ArgumentException($"The default GUID `{default(Guid)}` is not allowed.", nameof(guid));
            }
        }

        /// <summary>
        ///     Gets the unique identifier for this <see cref="IPluginsSystemResource"/>.
        /// </summary>
        public Guid Guid { get; }
    }
}
