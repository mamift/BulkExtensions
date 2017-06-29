using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions;
using UnitTests.EF6.Database;
using UnitTests.EF6.Helpers;
using UnitTests.EF6.Model;
using Xunit;

namespace UnitTests.EF6.BulkInsertTests
{
    public class EF6_IdentityOnlyBulkInsert : IDisposable
    {
        private readonly TestDatabase _context;
        private readonly IList<IdentityOnlyAutoEntity> _autoList;
        private readonly IList<IdentityOnlyEntity> _list;

        public EF6_IdentityOnlyBulkInsert()
        {
            _context = new TestDatabase();
            ClearTable();

            _autoList = new List<IdentityOnlyAutoEntity>
            {
                new IdentityOnlyAutoEntity(),
                new IdentityOnlyAutoEntity(),
                new IdentityOnlyAutoEntity(),
                new IdentityOnlyAutoEntity(),
                new IdentityOnlyAutoEntity()
            };

            _list = new List<IdentityOnlyEntity>
            {
                new IdentityOnlyEntity{Id = Helper.RandomString(10)},
                new IdentityOnlyEntity{Id = Helper.RandomString(10)},
                new IdentityOnlyEntity{Id = Helper.RandomString(10)},
                new IdentityOnlyEntity{Id = Helper.RandomString(10)},
                new IdentityOnlyEntity{Id = Helper.RandomString(10)}
            };
        }

        [Fact]
        public void TestAffectedRowsCount_Auto()
        {
            var count = _context.BulkInsert(_autoList);
            Assert.Equal(_autoList.Count, count);
        }

        [Fact]
        public void TestAffectedRowsCount_Auto_InsertOrUpdate()
        {
            var count = _context.BulkInsertOrUpdate(_autoList);
            Assert.Equal(_autoList.Count, count);
        }

        [Fact]
        public void TestOutputIdentity_Auto()
        {
            _context.BulkInsert(_autoList, InsertOptions.OutputIdentity);
            Assert.True(_autoList.All(entity => entity.Id != 0));
        }

        [Fact]
        public void TestOutputIdentity_Auto_InsertOrUpdate()
        {
            _context.BulkInsertOrUpdate(_autoList, InsertOptions.OutputIdentity);
            Assert.True(_autoList.All(entity => entity.Id != 0));
        }

        [Fact]
        public void TestAffectedRowsCount()
        {
            var count = _context.BulkInsert(_list);
            Assert.Equal(_list.Count, count);
        }

        [Fact]
        public void TestAffectedRowsCount_Auto_BulkUpdate()
        {
            _context.IdentityOnlyAutoEntity.AddRange(_autoList);
            _context.SaveChanges();

            var count = _context.BulkUpdate(_autoList);
            Assert.Equal(0, count);
        }

        public void Dispose()
        {
            ClearTable();
        }

        private void ClearTable()
        {
            _context.IdentityOnlyAutoEntity.RemoveRange(_context.IdentityOnlyAutoEntity.ToList());
            _context.IdentityOnlyEntity.RemoveRange(_context.IdentityOnlyEntity.ToList());
            _context.SaveChanges();
        }
    }
}