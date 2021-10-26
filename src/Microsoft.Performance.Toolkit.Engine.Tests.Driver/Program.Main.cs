// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Engine.Tests.Driver
{
    public sealed partial class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Signals.Initialize();
                Console.CancelKeyPress += Console_CancelKeyPress;

                var p = new Program();
                return p.Run(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Unexpected Exception: {0}", e);
                return e.HResult;
            }
            finally
            {
                Console.CancelKeyPress -= Console_CancelKeyPress;
                Signals.Cleanup();
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Signals.Cancel();
        }
    }
}
