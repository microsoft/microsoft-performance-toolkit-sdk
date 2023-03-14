// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common.Exceptions
{
    /// <summary>
    ///     Base class for all exceptions that occur while interacting with a <see cref="IRepository{TEntity}"/>.
    /// </summary>
    public abstract class RepositoryException
        : Exception
    {
        protected RepositoryException()
        {
        }

        protected RepositoryException(string message)
            : base(message)
        {
        }

        protected RepositoryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
