// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.TableCommands;
using Microsoft.Performance.SDK.Runtime.TableCommands;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.TableCommands
{
    [TestClass]
    public sealed class TableCommand3AdapterTests
    {
        [TestMethod]
        [UnitTest]
        public void TableCommand3NullNameThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new TableCommandCallbackAdapter(null, _ => { }));
        }

        [TestMethod]
        [UnitTest]
        public void TableCommand3WhitespaceNameThrows()
        {
            Assert.ThrowsException<ArgumentException>(
                () => new TableCommandCallbackAdapter("   ", _ => { }));
        }

        [TestMethod]
        [UnitTest]
        public void TableCommand3TrimsCommandName()
        {
            var sut = new TableCommandCallbackAdapter("  name  ", _ => { });
            Assert.AreEqual("name", sut.CommandName);
        }

        [TestMethod]
        [UnitTest]
        public void TableCommandCallbackAdapterCanExecuteAlwaysTrue()
        {
            var sut = new TableCommandCallbackAdapter("n", _ => { });
            Assert.IsTrue(sut.CanExecute(new TableCommandContext(null, null, new List<int>())));
            Assert.IsTrue(sut.CanExecute(null));
        }

        [TestMethod]
        [UnitTest]
        public void TableCommandCallbackAdapterExecuteForwardsSelectedRows()
        {
            IReadOnlyList<int> captured = null;
            var sut = new TableCommandCallbackAdapter("n", rows => captured = rows);
            var rows = new List<int> { 1, 2 }.AsReadOnly();

            var result = sut.Execute(new TableCommandContext(null, null, rows));

            Assert.AreSame(rows, captured);
            Assert.AreEqual(VoidTableCommandResult.Default, result);
        }

        [TestMethod]
        [UnitTest]
        public void TableCommandCallbackAdapterNullCallbackThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new TableCommandCallbackAdapter("n", null));
        }

        [TestMethod]
        [UnitTest]
        public void TableCommand2AdapterForwardsPredicateAndAction()
        {
            TableCommandContext canCtx = null;
            TableCommandContext execCtx = null;
            var sut = new TableCommand2Adapter(
                "n",
                ctx => { canCtx = ctx; return false; },
                ctx => { execCtx = ctx; });

            var input = new TableCommandContext(null, null, new List<int>());
            Assert.IsFalse(sut.CanExecute(input));
            Assert.AreSame(input, canCtx);

            var result = sut.Execute(input);
            Assert.AreSame(input, execCtx);
            Assert.AreEqual(VoidTableCommandResult.Default, result);
        }

        [TestMethod]
        [UnitTest]
        public void TableCommand2AdapterNullDelegatesThrow()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new TableCommand2Adapter("n", null, _ => { }));
            Assert.ThrowsException<ArgumentNullException>(
                () => new TableCommand2Adapter("n", _ => true, null));
        }
    }
}
