using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions;
using UnitTests.NetStandard.Database;
using UnitTests.NetStandard.Helpers;
using UnitTests.NetStandard.Model;
using Xunit;

namespace UnitTests.NetStandard.BulkInsertTests
{
    public class NetStandard_CustomSchemaBulkInsert : IDisposable
    {
        private readonly TestDatabase _context;
        private readonly IList<CustomSchemaEntity> _collection;

        public NetStandard_CustomSchemaBulkInsert()
        {
            _context = new TestDatabase();
            ClearTable();
            _collection = new List<CustomSchemaEntity>();
            for (var i = 0; i < 10; i++)
            {
                _collection.Add(new CustomSchemaEntity
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
            var simpleModels = _context.CustomSchemaEntities.ToList();
            Assert.Equal(simpleModels.Count, _collection.Count);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly()
        {
            _context.BulkInsert(_collection);
            var simpleModels = _context.CustomSchemaEntities
                .OrderBy(model => model.Id)
                .ToList();

            Assert.Equal(simpleModels.Count, _collection.Count);

            for (var i = 0; i < _collection.Count; i++)
            {
                var entity = _collection[i];
                var saved = simpleModels[i];
                Assert.Equal(entity.Name, saved.Name);
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
        public void TestInsertedEntitiesIdentities_OutputIdentity()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity);
            Assert.True(_collection.All(model => model.Id != 0));
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputIdentity()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity);
            var savedModel = _context.CustomSchemaEntities
                .OrderBy(model => model.Id)
                .ToList();

            Assert.Equal(savedModel.Count, _collection.Count);

            for (var i = 0; i < _collection.Count; i++)
            {
                var entity = _collection[i];
                var saved = savedModel[i];
                Assert.Equal(entity.Id, saved.Id);
                Assert.Equal(entity.Name, saved.Name);
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
        public void TestInsertedEntitiesIdentities_OutputComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputComputed);
            Assert.True(_collection.All(model => model.Id == 0));
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputComputed);
            var savedModel = _context.CustomSchemaEntities
                .OrderBy(model => model.Id)
                .ToList();

            Assert.Equal(savedModel.Count, _collection.Count);

            for (var i = 0; i < _collection.Count; i++)
            {
                var entity = _collection[i];
                var saved = savedModel[i];
                Assert.NotEqual(entity.Id, saved.Id);
                Assert.Equal(entity.Name, saved.Name);
            }
        }

        #endregion
        
        #region Output Identity & Computed Set

        [Fact]
        public void TestAffectedRowsCount_OutputIdentityComputed()
        {
            var rowsCount = _context.BulkInsert(_collection, InsertOptions.OutputIdentity  | InsertOptions.OutputComputed);
            Assert.Equal(rowsCount, _collection.Count);
        }

        [Fact]
        public void TestInsertedEntitiesIdentities_OutputIdentityComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity  | InsertOptions.OutputComputed);
            Assert.True(_collection.All(model => model.Id != 0));
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputIdentityComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity | InsertOptions.OutputComputed);
            var savedModel = _context.CustomSchemaEntities
                .OrderBy(model => model.Id)
                .ToList();

            Assert.Equal(savedModel.Count, _collection.Count);

            for (var i = 0; i < _collection.Count; i++)
            {
                var entity = _collection[i];
                var saved = savedModel[i];
                Assert.Equal(entity.Id, saved.Id);
                Assert.Equal(entity.Name, saved.Name);
            }
        }

        #endregion

        public void Dispose()
        {
            ClearTable();
        }

        private void ClearTable()
        {
            _context.CustomSchemaEntities.RemoveRange(_context.CustomSchemaEntities.ToList());
            _context.SaveChanges();
        }
    }
}