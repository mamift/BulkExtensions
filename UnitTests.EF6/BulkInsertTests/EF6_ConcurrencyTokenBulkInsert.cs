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
    public class EF6_ConcurrencyTokenBulkInsert : IDisposable
    {
        private readonly TestDatabase _context;
        private readonly IList<ConcurrencyTokenEntity> _collection;

        public EF6_ConcurrencyTokenBulkInsert()
        {
            _context = new TestDatabase();
            ClearTable();
            _collection = new List<ConcurrencyTokenEntity>();
            for (var i = 0; i < 10; i++)
            {
                _collection.Add(new ConcurrencyTokenEntity
                {
                    Name = Helper.RandomString(10)
                });
            }
        }

        #region No Options Set

        [Fact]
        public void TestAffectedRowsCount()
        {
            var rowsCount = _context.BulkInsert(_collection);
            Assert.Equal(rowsCount, _collection.Count);
        }

        [Fact]
        public void TestInsertedEntitiesCount()
        {
            _context.BulkInsert(_collection);
            var savedEntities = _context.ConcurrencyTokenEntity.ToList();
            Assert.Equal(savedEntities.Count, _collection.Count);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly()
        {
            _context.BulkInsert(_collection);
            var savedEntities = _context.ConcurrencyTokenEntity
                .ToList();

            Assert.Equal(savedEntities.Count, _collection.Count);

            for (var i = 0; i < _collection.Count; i++)
            {
                var entity = _collection[i];
                var savedEntity = savedEntities[i];
                Assert.NotEqual(entity.Id, savedEntity.Id);
                Assert.Equal(entity.Name, savedEntity.Name);
                Assert.NotEqual(savedEntity.RowVersion, entity.RowVersion);
            }
        }

        #endregion

        #region Output Identity Set

        [Fact]
        public void TestAffectedRowsCount_OutputIdentity()
        {
            var rowsCount = _context.BulkInsert(_collection, InsertOptions.OutputIdentity);
            Assert.Equal(rowsCount, _collection.Count);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputIdentity()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity);
            var savedEntities = _context.ConcurrencyTokenEntity
                .ToList();

            Assert.Equal(savedEntities.Count, _collection.Count);

            foreach (var entity in _collection)
            {
                var savedEntity = savedEntities.SingleOrDefault(saved => saved.Id.Equals(entity.Id));
                Assert.NotNull(savedEntity);
                Assert.Equal(entity.Name, savedEntity.Name);
                Assert.NotEqual(savedEntity.RowVersion, entity.RowVersion);
            }
        }

        #endregion

        #region Output Computed Set

        [Fact]
        public void TestAffectedRowsCount_OutputComputed()
        {
            var rowsCount = _context.BulkInsert(_collection, InsertOptions.OutputComputed);
            Assert.Equal(rowsCount, _collection.Count);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputComputed);
            var savedEntities = _context.ConcurrencyTokenEntity
                .ToList();

            Assert.Equal(savedEntities.Count, _collection.Count);

            for (var i = 0; i < _collection.Count; i++)
            {
                var entity = _collection[i];
                var savedEntity = savedEntities[i];
                Assert.NotEqual(entity.Id, savedEntity.Id);
                Assert.Equal(savedEntity.Name, entity.Name);
                Assert.NotEqual(savedEntity.RowVersion, entity.RowVersion);
            }
        }

        #endregion

        #region Output Identity & Computed Set

        [Fact]
        public void TestAffectedRowsCount_OutputIdentityComputed()
        {
            var rowsCount = _context.BulkInsert(_collection,
                InsertOptions.OutputIdentity | InsertOptions.OutputComputed);
            Assert.Equal(rowsCount, _collection.Count);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputIdentityComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity | InsertOptions.OutputComputed);
            var savedEntities = _context.ConcurrencyTokenEntity
                .ToList();

            Assert.Equal(savedEntities.Count, _collection.Count);

            foreach (var entity in _collection)
            {
                var savedEntity = savedEntities.SingleOrDefault(saved => saved.Id.Equals(entity.Id));
                Assert.NotNull(savedEntity);
                Assert.Equal(entity.Name, savedEntity.Name);
                Assert.NotEqual(savedEntity.RowVersion, entity.RowVersion);
            }
        }

        #endregion

        public void Dispose()
        {
            ClearTable();
        }

        private void ClearTable()
        {
            _context.ConcurrencyTokenEntity.RemoveRange(_context.ConcurrencyTokenEntity.ToList());
            _context.SaveChanges();
        }
    }
}