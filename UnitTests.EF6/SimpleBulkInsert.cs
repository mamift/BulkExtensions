using System;
using System.Collections.Generic;
using EntityFramework.BulkExtensions;
using UnitTests.EF6.Database;
using UnitTests.EF6.Helpers;
using UnitTests.EF6.Model;
using Xunit;

namespace UnitTests.EF6
{
    public class SimpleBulkInsert
    {
        private readonly TestDatabase _context;
        private readonly IList<SimpleModel> _collection;

        public SimpleBulkInsert()
        {
            _context = new TestDatabase();
            _context.Database.Initialize(true);
            _collection = new List<SimpleModel>();
            for (var i = 0; i < 10; i++)
            {
                _collection.Add(new SimpleModel
                {
                    StringProperty = Helper.RandomString(10),
                    IntValue = Helper.RandomInt(),
                    DateTime = DateTime.Now,
                    Type = Helper.RandomEnum(),
                    DoubleValue = Helper.RandomDouble(1, 10) 
                });
            }
        }
        
        [Fact]
        public void CreateTest()
        {
            var rowsCount = _context.BulkInsert(_collection);
            Assert.True(rowsCount == _collection.Count);
        }
    }
}