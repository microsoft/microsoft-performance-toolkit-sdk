// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Discovery
{
    /// <summary>
    ///     Contains static methods related to AssemblyLoad functionality.
    /// </summary>
    public static class AssemblyLoadHelpers
    {
        /// <summary>
        ///     Find all conflicting DLLs in the current AppDomain.
        /// </summary>
        /// <returns>
        ///     Conflicting DLL paths, keyed by assembly name.
        /// </returns>
        public static IDictionary<string, IEnumerable<string>> FindDllConflicts()
        {
            var loadedDlls = new Dictionary<string, List<string>>();
            var loaded = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in loaded.Where(x => !x.IsDynamic))
            {
                var name = assembly.GetName().Name;
                var location = assembly.Location;
                if (!loadedDlls.TryGetValue(name, out List<string> paths))
                {
                    paths = new List<string>();
                    loadedDlls[name] = paths;
                }

                paths.Add(location);
            }

            var duplicates = loadedDlls.Where(x => x.Value.Count > 1)
                .ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            return duplicates;
        }

        internal static void EmitLoadContexts(
            TextWriter output)
        {
            if (output is null)
            {
                return;
            }

            void printAssemblies(AssemblyLoadContext context, bool isForLastContext)
            {
                var assemblies = context.Assemblies.OrderBy(x => x.FullName).ToArray();
                if (assemblies.Length == 0)
                {
                    return;
                }

                var leftMostPipe = !isForLastContext ? '|' : ' ';
                for (var j = 0; j < assemblies.Length - 1; ++j)
                {
                    var assembly = assemblies[j];
                    output.WriteLine(@"{0}     |-- {1}", leftMostPipe, assembly.FullName);
                    output.WriteLine(@"{0}     |   \-- {1}", leftMostPipe, assembly.GetCodeBaseAsLocalPath());
                }

                Debug.Assert(assemblies.Length > 0);
                output.WriteLine(@"{0}     \-- {1}", leftMostPipe, assemblies[assemblies.Length - 1].FullName);
                output.WriteLine(@"{0}         \-- {1}", leftMostPipe, assemblies[assemblies.Length - 1].GetCodeBaseAsLocalPath());
            }

            string contextName(AssemblyLoadContext context)
            {
                var n = new StringBuilder(context.Name?.Length ?? 0 + " (Default)".Length);
                if (!string.IsNullOrWhiteSpace(context.Name))
                {
                    n.Append(context.Name);
                }
                else
                {
                    n.Append("<NO NAME>");
                }

                if (context == AssemblyLoadContext.Default)
                {
                    n.Append(" (Default)");
                }


                return n.ToString();
            }

            var contexts = AssemblyLoadContext.All.OrderBy(x => x.Name).ToArray();
            if (contexts.Length == 0)
            {
                return;
            }

            output.WriteLine("Assembly Load Contexts");
            for (var i = 0; i < contexts.Length - 1; ++i)
            {
                var context = contexts[i];
                output.WriteLine("|-- {0}", contextName(context));
                printAssemblies(context, false);
            }

            Debug.Assert(contexts.Length > 0);
            output.WriteLine(@"\-- {0}", contextName(contexts[contexts.Length - 1]));
            printAssemblies(contexts[contexts.Length - 1], true);
        }
    }
}
