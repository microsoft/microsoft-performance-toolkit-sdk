// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var column = new BaseDataColumn<string>(
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
            var column = new BaseDataColumn<string>(
               new ColumnMetadata(Guid.NewGuid(), "name"),
               new UIHints { Width = 200, },
               Projection.CreateUsingFuncAdaptor(i => "test"));

            Assert.AreEqual(this.Sut, this.Sut.AddColumn(column));
        }

        [TestMethod]
        [UnitTest]
        public void ReplaceColumnOldNullThrows()
        {
            var column = new BaseDataColumn<string>(
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
            var column = new BaseDataColumn<string>(
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
            var column = new BaseDataColumn<string>(
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
            var columnOld = new BaseDataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            var columnNew = new BaseDataColumn<string>(
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
            var columnNotThere = new BaseDataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            var columnNew = new BaseDataColumn<string>(
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
            var columnOld = new BaseDataColumn<string>(
              new ColumnMetadata(Guid.NewGuid(), "name"),
              new UIHints { Width = 200, },
              Projection.CreateUsingFuncAdaptor(i => "test"));

            var columnNew = new BaseDataColumn<string>(
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
            Assert.AreEqual(name, command.MenuName);

            var testList = new List<int> { 1, 2, 3, }.AsReadOnly();
            command.Callback(testList);

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
            Assert.AreEqual(expectedName, command.MenuName);

            var testList = new List<int> { 1, 2, 3, }.AsReadOnly();
            command.Callback(testList);

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

            var command1 = this.Sut.Commands.SingleOrDefault(x => x.MenuName == name1);
            Assert.IsNotNull(command1);
            Assert.AreEqual(name1, command1.MenuName);
            var test1 = new List<int> { 1, 2, 3, }.AsReadOnly();
            command1.Callback(test1);
            Assert.AreEqual(captured1, test1);

            var command2 = this.Sut.Commands.SingleOrDefault(x => x.MenuName == name2);
            Assert.IsNotNull(command2);
            Assert.AreEqual(name2, command2.MenuName);
            var test2 = new List<int> { 1, 2, 3, }.AsReadOnly();
            command2.Callback(test2);
            Assert.AreEqual(captured2, test2);

            var command3 = this.Sut.Commands.SingleOrDefault(x => x.MenuName == name3);
            Assert.IsNotNull(command3);
            Assert.AreEqual(name3, command3.MenuName);
            var test3 = new List<int> { 1, 2, 3, }.AsReadOnly();
            command3.Callback(test3);
            Assert.AreEqual(captured3, test3);
        }
    }
}
