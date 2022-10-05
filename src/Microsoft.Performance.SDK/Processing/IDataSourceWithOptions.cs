﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.SDK.Processing
{
    public interface IDataSourceWithOptions : IDataSource
    {
        ProcessorOptions ProcessorOptions { get; }
    }
}
