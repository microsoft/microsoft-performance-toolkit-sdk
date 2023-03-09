// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata
{
    /// <summary>
    ///     Represents project information about a processing source.
    /// </summary>
    public sealed class ProjectInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProjectInfo"/> class with the specified parameters.
        /// </summary>
        /// <param name="uri">
        ///     The URI to the page for this project.
        /// </param>
        [JsonConstructor]
        public ProjectInfo(string uri)
        {
            this.Uri = uri;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProjectInfo"/> class from a <see cref="SDK.Processing.ProjectInfo"/> instance.
        /// </summary>
        /// <param name="projectInfo"></param>
        public ProjectInfo(SDK.Processing.ProjectInfo projectInfo)
            : this(projectInfo.Uri)
        {
        }

        /// <summary>
        ///     Gets the URI to the page for this project.
        /// </summary>
        public string Uri { get; }
    }
}
