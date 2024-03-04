// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    [TestClass]
    public class EngineFixture
    {
        public TestContext TestContext { get; set; }

        protected DirectoryInfo Scratch { get; private set; }

        [TestInitialize]
        public void Initialize()
        {
            var scratchPath = Path.Combine(
                this.TestContext.TestResultsDirectory,
                $"Scratch-{this.GetType().Name}-{this.TestContext.TestName}");
            this.Scratch = new DirectoryInfo(scratchPath);
            try
            {
                this.Scratch.Delete(true);
            }
            catch (DirectoryNotFoundException)
            {
            }

            try
            {
                this.Scratch.Create();
            }
            catch (Exception e)
            {
                Assert.Fail("Initialize: Unable to create scratch directory `{0}`: {1}", this.Scratch.FullName, e);
            }

            this.OnInitialize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.OnCleanup();
            if (this.TestContext.CurrentTestOutcome == UnitTestOutcome.Passed)
            {
                try
                {
                    this.Scratch.Delete(true);
                }
                catch (DirectoryNotFoundException)
                {
                }
                catch (Exception e)
                {
                    Assert.Fail(
                        "Cleanup: Unable to delete scratch directory `{0}`: {1}",
                        this.Scratch.FullName,
                        e);
                }
            }
            else
            {
                this.TestContext.WriteLine(
                    "The test did not indicate success. The scratch directory has been preserved at `{0}`",
                    this.Scratch.FullName);
            }
        }

        public virtual void OnInitialize()
        {
        }

        public virtual void OnCleanup()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }

        protected static void CopyAssemblyContainingType(Type type, DirectoryInfo destDir)
        {
            Assert.IsNotNull(type);

            var assemblyFile = type.Assembly.GetCodeBaseAsLocalPath();
            var assemblyFileName = Path.GetFileName(assemblyFile);
            File.Copy(assemblyFile, Path.Combine(destDir.FullName, assemblyFileName), true);
        }
    }
}
