// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common.Exceptions
{
    /// <summary>
    ///     An exception that is thrown when a repository cannot be read or written to.
    /// </summary>
    public class RepositoryDataAccessException
        : RepositoryException
    {
        public RepositoryDataAccessException()
        {
        }

        public RepositoryDataAccessException(string message)
            : base(message)
        {
        }

        public RepositoryDataAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
