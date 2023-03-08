// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///     An exception that is thrown when the repository is in an invalid state.
    /// </summary>
    public class RepositoryCorruptedException
           : RepositoryException
    {
        public RepositoryCorruptedException()
        {
        }

        public RepositoryCorruptedException(string message)
            : base(message)
        {
        }

        public RepositoryCorruptedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
