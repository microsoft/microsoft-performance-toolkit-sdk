// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.TableCommands;
using Microsoft.Performance.SDK.Runtime.TableCommands;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public sealed class TableBuilderTests
    {
        private TableBuilder Sut { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            this.Sut = new TableBuilder();
        }

        [TestMethod]
        [UnitTest]
        public void WhenConstructedHasNoRowsOrColumns()
        {
            Assert.AreEqual(0, this.Sut.RowCount);
            Assert.AreEqual(0, this.Sut.Columns.Count());
        }

        [TestMethod]
        [UnitTest]
        public void SetRowCountSets()
        {
            this.Sut.SetRowCount(23);

            Assert.AreEqual(23, this.Sut.RowCount);
        }

        [TestMethod]
        [UnitTest]
        public void SetRowCountReturnsBuilder()
        {
            Assert.AreEqual(this.Sut, this.Sut.SetRowCount(23));
        }

        [TestMethod]
        [UnitTest]
        public void SetRowCountDoesNotAllowNegativeNumber()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => this.Sut.SetRowCount(-1));
        }

        [TestMethod]
        [UnitTest]
        public void AddColumnDoesNotAllowNulls()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => this.Sut.AddColumn(null));
        }

        [TestMethod]
        [UnitTest]
        public void AddColumnAdds()
        {
            var column = new DataColumn<string>(
                new ColumnMetadata(Guid.NewGuid(), "name"),
                new UIHints { Width = 200, },
                Projection.CreateUsingFuncAdaptor(i => "test"));

            this.Sut.AddColumn(column);

            Assert.IsTrue(this.Sut.Columns.Contains(column));
        }

        [TestMethod]
        [UnitTest]
        public void AddColumnReturnsBuilder()
        {
            var column = new DataColumn<string>(
               new ColumnMetadata(Guid.NewGuid(), "name"),
               new UIHints { Width = 200, },
               Projection.CreateUsingFuncAdaptor(i => "test"));

            Assert.AreEqual(this.Sut, this.Sut.AddColumn(column));
        }

        [TestMethod]
        [UnitTest]
        public void ReplaceColumnOldNullThrows()
        {
            var column = new DataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            Assert.ThrowsException<ArgumentNullException>(
                () => this.Sut.ReplaceColumn(null, column));
        }

        [TestMethod]
        [UnitTest]
        public void ReplaceColumnNewNullThrows()
        {
            var column = new DataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            Assert.ThrowsException<ArgumentNullException>(
                () => this.Sut.ReplaceColumn(column, null));
        }

        [TestMethod]
        [UnitTest]
        public void ReplaceColumnWithSelfNoOps()
        {
            var column = new DataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            this.Sut.AddColumn(column);
            this.Sut.ReplaceColumn(column, column);

            Assert.AreEqual(1, this.Sut.Columns.Count);
            Assert.AreEqual(column, this.Sut.Columns.Single());
        }

        [TestMethod]
        [UnitTest]
        public void ReplaceColumnThatExistsReplaces()
        {
            var columnOld = new DataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            var columnNew = new DataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            this.Sut.AddColumn(columnOld);
            this.Sut.ReplaceColumn(columnOld, columnNew);

            Assert.AreEqual(1, this.Sut.Columns.Count);
            Assert.AreEqual(columnNew, this.Sut.Columns.Single());
        }

        [TestMethod]
        [UnitTest]
        public void ReplaceColumnThatDoesNotExistAdds()
        {
            var columnNotThere = new DataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            var columnNew = new DataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            this.Sut.ReplaceColumn(columnNotThere, columnNew);

            Assert.AreEqual(1, this.Sut.Columns.Count);
            Assert.AreEqual(columnNew, this.Sut.Columns.Single());
        }

        [TestMethod]
        [UnitTest]
        public void ReplaceColumnReturnsBuilder()
        {
            var columnOld = new DataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            var columnNew = new DataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            this.Sut.AddColumn(columnOld);
            var builder = this.Sut.ReplaceColumn(columnOld, columnNew);

            Assert.AreEqual(this.Sut, builder);
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommandInvalidArgumentsThrow()
        {
            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.AddTableCommand(null, _ => { }));
            Assert.ThrowsException<ArgumentException>(() => this.Sut.AddTableCommand(string.Empty, _ => { }));
            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.AddTableCommand("test", null));

            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.AddTableCommand2("test", null, (context) => { }));
            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.AddTableCommand2("test", (context) => true, null));
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommandDuplicateNameThrows()
        {
            this.Sut.AddTableCommand("test", _ => { });
            Assert.ThrowsException<InvalidOperationException>(() => this.Sut.AddTableCommand("test", _ => { }));
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommandDuplicateNameThrowsIrrespectiveOfCase()
        {
            this.Sut.AddTableCommand("test", _ => { });
            Assert.ThrowsException<InvalidOperationException>(() => this.Sut.AddTableCommand("tEsT", _ => { }));
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommandDuplicateNameThrowsIrrespectiveOfPadding()
        {
            this.Sut.AddTableCommand("test", _ => { });
            Assert.ThrowsException<InvalidOperationException>(() => this.Sut.AddTableCommand(" test\t  ", _ => { }));
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommandAdds()
        {
            var name = "test";
            IReadOnlyList<int> capturedRows = null;
            void callback(IReadOnlyList<int> selectedRows)
            {
                capturedRows = selectedRows;
            }

            this.Sut.AddTableCommand(name, callback);

            Assert.AreEqual(1, this.Sut.Commands.Count);
            var command = this.Sut.Commands.Single();
            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(TableCommandCallbackAdapter));
            Assert.AreEqual(name, command.CommandName);

            var testList = new List<int> { 1, 2, 3, }.AsReadOnly();
            ((TableCommandCallbackAdapter)command).Execute(new TableCommandContext(null, null, testList));

            Assert.IsNotNull(capturedRows);
            Assert.AreEqual(testList, capturedRows);
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommandAddsWithTrimmedName()
        {
            var name = "   test\r \t\n  ";
            var expectedName = name.Trim();
            IReadOnlyList<int> capturedRows = null;
            void callback(IReadOnlyList<int> selectedRows)
            {
                capturedRows = selectedRows;
            }

            this.Sut.AddTableCommand(name, callback);

            Assert.AreEqual(1, this.Sut.Commands.Count);
            var command = this.Sut.Commands.Single();
            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(TableCommandCallbackAdapter));
            Assert.AreEqual(expectedName, command.CommandName);

            var testList = new List<int> { 1, 2, 3, }.AsReadOnly();
            ((TableCommandCallbackAdapter)command).Execute(new TableCommandContext(null, null, testList));

            Assert.IsNotNull(capturedRows);
            Assert.AreEqual(testList, capturedRows);
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommandReturnsInstanceOfBuilder()
        {
            var ret = this.Sut.AddTableCommand("test", _ => { });

            Assert.AreEqual(this.Sut, ret);
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommandMultipleCommandsCanBeAdded()
        {
            var name1 = "test1";
            var name2 = "test2";
            var name3 = "test3";

            IReadOnlyList<int> captured1 = null;
            IReadOnlyList<int> captured2 = null;
            IReadOnlyList<int> captured3 = null;

            this.Sut.AddTableCommand(name1, x => captured1 = x);
            this.Sut.AddTableCommand(name2, x => captured2 = x);
            this.Sut.AddTableCommand(name3, x => captured3 = x);

            Assert.AreEqual(3, this.Sut.Commands.Count);

            var command1 = this.Sut.Commands.SingleOrDefault(x => x.CommandName == name1);
            Assert.IsNotNull(command1);
            Assert.AreEqual(name1, command1.CommandName);
            var test1 = new List<int> { 1, 2, 3, }.AsReadOnly();
            ((TableCommandCallbackAdapter)command1).Execute(new TableCommandContext(null, null, test1));

            Assert.AreEqual(captured1, test1);

            var command2 = this.Sut.Commands.SingleOrDefault(x => x.CommandName == name2);
            Assert.IsNotNull(command2);
            Assert.AreEqual(name2, command2.CommandName);
            var test2 = new List<int> { 1, 2, 3, }.AsReadOnly();
            ((TableCommandCallbackAdapter)command2).Execute(new TableCommandContext(null, null, test2));
            Assert.AreEqual(captured2, test2);

            var command3 = this.Sut.Commands.SingleOrDefault(x => x.CommandName == name3);
            Assert.IsNotNull(command3);
            Assert.AreEqual(name3, command3.CommandName);
            var test3 = new List<int> { 1, 2, 3, }.AsReadOnly();
            ((TableCommandCallbackAdapter)command3).Execute(new TableCommandContext(null, null, test3));
            Assert.AreEqual(captured3, test3);
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommand2WrapsInTableCommand2Adapter()
        {
            var name = "test";
            TableCommandContext capturedCanContext = null;
            TableCommandContext capturedOnContext = null;

            this.Sut.AddTableCommand2(
                name,
                ctx => { capturedCanContext = ctx; return true; },
                ctx => { capturedOnContext = ctx; });

            Assert.AreEqual(1, this.Sut.Commands.Count);
            var command = this.Sut.Commands.Single();
            Assert.IsInstanceOfType(command, typeof(TableCommand2Adapter));
            Assert.AreEqual(name, command.CommandName);

            var adapter = (TableCommand2Adapter)command;
            var ctxIn = new TableCommandContext(null, null, new List<int> { 5 });

            Assert.IsTrue(adapter.CanExecute(ctxIn));
            Assert.AreSame(ctxIn, capturedCanContext);

            var result = adapter.Execute(ctxIn);
            Assert.AreSame(ctxIn, capturedOnContext);
            Assert.AreEqual(VoidTableCommandResult.Default, result);
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommand3AddsCommand()
        {
            var cmd = new TestTableCommand("cmd1");

            var ret = this.Sut.AddTableCommand3(cmd);

            Assert.AreSame(this.Sut, ret);
            Assert.AreEqual(1, this.Sut.Commands.Count);
            Assert.AreSame(cmd, this.Sut.Commands.Single());
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommand3NullThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.AddTableCommand3(null));
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommand3DuplicateNameThrows()
        {
            this.Sut.AddTableCommand3(new TestTableCommand("dup"));
            Assert.ThrowsException<InvalidOperationException>(
                () => this.Sut.AddTableCommand3(new TestTableCommand("DUP")));
        }

        [TestMethod]
        [UnitTest]
        public void AddTableCommand3DuplicateAgainstLegacyThrows()
        {
            this.Sut.AddTableCommand("legacy", _ => { });
            Assert.ThrowsException<InvalidOperationException>(
                () => this.Sut.AddTableCommand3(new TestTableCommand("LEGACY")));
        }

        [TestMethod]
        [UnitTest]
        public void CommandsPreservesInsertionOrder()
        {
            this.Sut.AddTableCommand("a", _ => { });
            this.Sut.AddTableCommand2("b", _ => true, _ => { });
            this.Sut.AddTableCommand3(new TestTableCommand("c"));

            var names = this.Sut.Commands.Select(x => x.CommandName).ToList();
            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, names);
        }

        private sealed class TestTableCommand : TableCommand3<TableCommandContext, int>
        {
            public TestTableCommand(string name) : base(name) { }
            public override bool CanExecute(TableCommandContext context) => true;
            public override int Execute(TableCommandContext context) => 42;
        }
    }
}
