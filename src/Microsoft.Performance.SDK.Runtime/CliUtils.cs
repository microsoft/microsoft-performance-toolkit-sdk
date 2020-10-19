// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Utility helper class for Common Language Infrastructure (CLI) operations
    /// </summary>
    public static class CliUtils
    {
        /// <summary>
        ///     Detects if the assembly is a Common Language Infrastructure (CLI) binary.
        ///     For details, see <a href="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/assemblies-gac/how-to-determine-if-a-file-is-an-assembly">MSDN</a>.
        /// </summary>
        /// <param name="fullPathToCandidateDll">
        ///     Full file path of the assembly to evaluate.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the binary is a Common Language Infrastructure (CLI) binary;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsCliAssembly(
            string fullPathToCandidateDll)
        {
            if (fullPathToCandidateDll == null)
            {
                throw new ArgumentNullException(nameof(fullPathToCandidateDll));
            }

            bool isCliAssembly;
            try
            {
                AssemblyName.GetAssemblyName(fullPathToCandidateDll);
                isCliAssembly = true;
            }

            // let FileNotFound, SecurityException, and ArgumentException out
            catch (BadImageFormatException)
            {
                isCliAssembly = false;
            }
            catch (FileLoadException)
            {
                // the assembly is already loaded, so it must be an CLI assembly.
                // we only care if the file is an assembly; we are not actually
                // loading the assembly.
                isCliAssembly = true;
            }

            return isCliAssembly;
        }
    }
}
