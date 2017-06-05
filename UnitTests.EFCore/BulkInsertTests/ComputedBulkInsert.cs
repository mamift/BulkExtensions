using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions;
using UnitTests.EFCore.Database;
using UnitTests.EFCore.Helpers;
using UnitTests.EFCore.Model;
using Xunit;

namespace UnitTests.EFCore.BulkInsertTests
{
    public class ComputedBulkInsert : IDisposable
    {
        private readonly TestDatabase _context;
        private readonly IList<ComputedEntity> _collection;

        public ComputedBulkInsert()
        {
            _context = new TestDatabase();
            ClearTable();
            _collection = new List<ComputedEntity>();
            for (var i = 0; i < 10; i++)
            {
                _collection.Add(new ComputedEntity
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
            var savedEntities = _context.NotIncrementIdEntity.ToList();
            Assert.Equal(savedEntities.Count, _collection.Count);
        }

//        [Fact]
//        public void TestInsertValuesSavedCorrectly()
//        {
//            _context.BulkInsert(_collection);
//            var savedEntities = _context.NotIncrementIdEntity
//                .ToList();
//
//            Assert.Equal(savedEntities.Count, _collection.Count);
//
//            foreach (var entity in _collection)
//            {
//                Assert.NotNull(savedEntity);
//                Assert.Equal(savedEntity.Name, entity.Name);
//            }
//        }

        #endregion

//        #region Output Identity Set
//
//        [Fact]
//        public void TestAffectedRowsCount_OutputIdentity()
//        {
//            var rowsCount = _context.BulkInsert(_collection, InsertOptions.OutputIdentity);
//            Assert.Equal(rowsCount, _collection.Count);
//        }
//
//        [Fact]
//        public void TestInsertValuesSavedCorrectly_OutputIdentity()
//        {
//            _context.BulkInsert(_collection, InsertOptions.OutputIdentity);
//            var savedEntities = _context.NotIncrementIdEntity
//                .ToList();
//
//            Assert.Equal(savedEntities.Count, _collection.Count);
//
//            foreach (var entity in _collection)
//            {
//                var savedEntity = savedEntities.SingleOrDefault(saved => saved.Id.Equals(entity.Id));
//                Assert.NotNull(savedEntity);
//            }
//        }
//
//        #endregion
//
//        #region Output Computed Set
//
//        [Fact]
//        public void TestAffectedRowsCount_OutputComputed()
//        {
//            var rowsCount = _context.BulkInsert(_collection, InsertOptions.OutputComputed);
//            Assert.Equal(rowsCount, _collection.Count);
//        }
//
//        [Fact]
//        public void TestInsertValuesSavedCorrectly_OutputComputed()
//        {
//            _context.BulkInsert(_collection, InsertOptions.OutputComputed);
//            var savedEntities = _context.NotIncrementIdEntity
//                .ToList();
//
//            Assert.Equal(savedEntities.Count, _collection.Count);
//
//            foreach (var entity in _collection)
//            {
//                var savedEntity = savedEntities.SingleOrDefault(saved => saved.Id.Equals(entity.Id));
//                Assert.NotNull(savedEntity);
//                Assert.Equal(savedEntity.Name, entity.Name);
//            }
//        }
//
//        #endregion
//
//        #region Output Identity & Computed Set
//
//        [Fact]
//        public void TestAffectedRowsCount_OutputIdentityComputed()
//        {
//            var rowsCount = _context.BulkInsert(_collection,
//                InsertOptions.OutputIdentity | InsertOptions.OutputComputed);
//            Assert.Equal(rowsCount, _collection.Count);
//        }
//
//        [Fact]
//        public void TestInsertValuesSavedCorrectly_OutputIdentityComputed()
//        {
//            _context.BulkInsert(_collection, InsertOptions.OutputIdentity | InsertOptions.OutputComputed);
//            var savedEntities = _context.NotIncrementIdEntity
//                .ToList();
//
//            Assert.Equal(savedEntities.Count, _collection.Count);
//
//            foreach (var entity in _collection)
//            {
//                var savedEntity = savedEntities.SingleOrDefault(saved => saved.Id.Equals(entity.Id));
//                Assert.NotNull(savedEntity);
//                Assert.Equal(savedEntity.Name, entity.Name);
//            }
//        }
//
//        #endregion

        public void Dispose()
        {
            ClearTable();
        }

        private void ClearTable()
        {
            _context.ComputedEntity.RemoveRange(_context.ComputedEntity.ToList());
            _context.SaveChanges();
        }
    }
}