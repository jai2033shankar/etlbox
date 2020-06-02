using ETLBox.ConnectionManager;
using ETLBox.DataFlow;
using ETLBox.Helper;
using ETLBox.SqlServer;
using ETLBoxTests.Fixtures;
using System;
using System.Collections.Generic;
using Xunit;

namespace ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class CustomSourceTests
    {
        public SqlConnectionManager Connection => Config.SqlConnection.ConnectionManager("DataFlow");
        public CustomSourceTests(DataFlowDatabaseFixture dbFixture)
        {
        }

        public class MySimpleRow
        {
            public int Col1 { get; set; }
            public string Col2 { get; set; }
        }

        [Fact]
        public void SimpleFlow()
        {
            //Arrange
            TwoColumnsTableFixture dest2Columns = new TwoColumnsTableFixture("Destination4CustomSource");
            List<string> Data = new List<string>() { "Test1", "Test2", "Test3" };
            int _readIndex = 0;
            Func<MySimpleRow> ReadData = () =>
            {
                var result = new MySimpleRow()
                {
                    Col1 = _readIndex + 1,
                    Col2 = Data[_readIndex]
                };
                _readIndex++;
                return result;
            };

            Func<bool> EndOfData = () => _readIndex >= Data.Count;

            //Act
            CustomSource<MySimpleRow> source = new CustomSource<MySimpleRow>(ReadData, EndOfData);
            DbDestination<MySimpleRow> dest = new DbDestination<MySimpleRow>(Connection, "Destination4CustomSource");
            source.LinkTo(dest);
            source.Execute();
            dest.Wait();

            //Assert
            dest2Columns.AssertTestData();
        }
    }
}