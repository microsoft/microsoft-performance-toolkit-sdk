// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.PlugInConfiguration
{
    /// <summary>
    ///     A plug-in configuration option allows a plug-in to change behavior based on requirements of an application
    ///     or runtime without coding directly to either one.
    /// </summary>
    public class ConfigurationOption
        : IEquatable<ConfigurationOption>
    {
        private readonly HashSet<string> _applications = new HashSet<string>(PlugInConfiguration.Comparer);
        private readonly HashSet<string> _runtimes = new HashSet<string>(PlugInConfiguration.Comparer);

        /// <summary>
        ///     Create an configuration option.
        /// </summary>
        /// <param name="name">
        ///     Option name.
        /// </param>
        /// <param name="description">
        ///     Option description.
        /// </param>
        public ConfigurationOption(string name, string description)
        {
            if (!PlugInConfigurationValidation.ValidateElementName(name))
            {
                throw new ArgumentException(message:
                    string.Format("The name is invalid '{0}': {1}", name, PlugInConfigurationValidation.ValidCharactersMessage),
                    nameof(name));
            }

            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        ///     Gets the configuration option name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets a description of the configuration option.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the applications which opt-in to this option.
        /// </summary>
        public IEnumerable<string> Applications => this._applications;

        /// <summary>
        ///     The runtimes which opt-in to this option.
        /// </summary>
        /// <remarks>
        ///     Some runtimes exist as class libraries from which many application may derive. This area is provided for
        ///     options which are runtime specific.
        /// </remarks>
        public IEnumerable<string> Runtimes => this._runtimes;

        /// <summary>
        ///     Adds name of an application that elects to use this option.
        /// </summary>
        /// <param name="applicationName">
        ///     Application name.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the name was added, <c>false</c> otherwise.
        /// </returns>
        public bool AddApplication(string applicationName)
        {
            return _applications.Add(applicationName);
        }

        /// <summary>
        ///     Adds names of applications that elect to use this option.
        /// </summary>
        /// <param name="applicationNames">
        ///     Application name.
        /// </param>
        /// <returns>
        ///     <c>true</c> if at least one name was added, <c>false</c> otherwise.
        /// </returns>
        public bool AddApplications(IEnumerable<string> applicationNames)
        {
            bool addedName = false;
            foreach (var name in applicationNames)
            {
                addedName |= _applications.Add(name);
            }
            return addedName;
        }

        /// <summary>
        ///     Deletes name of an application currently listed under this option.
        /// </summary>
        /// <param name="applicationName">
        ///     Application name.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the name was removed, <c>false</c> otherwise.
        /// </returns>
        public bool DelApplication(string applicationName)
        {
            return _applications.Remove(applicationName);
        }

        /// <summary>
        ///     Adds name of a runtime that elects to use this option.
        /// </summary>
        /// <param name="runtimeName">
        ///     Runtime name.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the name was added, <c>false</c> otherwise.
        /// </returns>
        public bool AddRuntime(string runtimeName)
        {
            return _runtimes.Add(runtimeName);
        }

        /// <summary>
        ///     Adds names of runtimes that elect to use this option.
        /// </summary>
        /// <param name="runtimeNames">
        ///     Runtime names.
        /// </param>
        /// <returns>
        ///     <c>true</c> if at least one name was removed, <c>false</c> otherwise.
        /// </returns>
        public bool AddRuntimes(IEnumerable<string> runtimeNames)
        {
            bool addedName = false;
            foreach (var name in runtimeNames)
            {
                addedName |= _runtimes.Add(name);
            }
            return addedName;
        }

        /// <summary>
        ///     Deletes name of an runtime currently listed under this option.
        /// </summary>
        /// <param name="runtimeName">
        ///     Runtime name.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the name was removed, <c>false</c> otherwise.
        /// </returns>
        public bool DelRuntime(string runtimeName)
        {
            return _runtimes.Remove(runtimeName);
        }

        /// <inheritdoc />
        public bool Equals(ConfigurationOption other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Name == other.Name;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((ConfigurationOption)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}
