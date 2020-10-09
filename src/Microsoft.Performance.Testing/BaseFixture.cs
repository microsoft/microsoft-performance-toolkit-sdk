// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Testing
{
    public abstract class BaseFixture
    {
        private readonly List<DirectoryInfo> temporaryDirectories;

        public TestContext TestContext { get; set; }

        protected BaseFixture()
        {
            this.temporaryDirectories = new List<DirectoryInfo>();
        }

        [TestInitialize]
        public void Setup()
        {
            this.temporaryDirectories.Clear();

            //
            // Any other fixture specific setup goes here.
            //

            //
            // Let sub-classes do their setup
            //

            this.OnSetup();
        }

        [TestCleanup]
        public void Cleanup()
        {
            //
            // Let sub-classes clean up first.
            //

            this.OnCleanup();

            var directoriesCleaned = this.CleanupDirectories();

            //
            // Any other fixture cleanup would go here.
            //

            Assert.IsTrue(directoriesCleaned);
        }

        public virtual void OnSetup()
        {
        }

        public virtual void OnCleanup()
        {
        }

        protected DirectoryInfo RequestTemporaryDirectory()
        {
            var tempDir = Path.Combine(
                Path.GetTempPath(),
                Path.GetDirectoryName(Path.GetTempFileName()));

            //
            // if we add it, but creation fails, then delete
            // will fail with DirectoryNotFound, which is okay.
            //

            var d = new DirectoryInfo(tempDir);
            this.temporaryDirectories.Add(d);
            d.Create();

            return d;
        }

        private bool CleanupDirectories()
        {
            var anyFailed = false;
            foreach (var d in this.temporaryDirectories)
            {
                try
                {
                    d.Delete(true);
                }
                catch (DirectoryNotFoundException)
                {
                    //
                    // This is okay, it is already gone.
                    //
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Failed to delete {0}: {1}", d.FullName, e);
                    anyFailed = true;
                }
            }

            this.temporaryDirectories.Clear();
            return !anyFailed;
        }
    }
}
