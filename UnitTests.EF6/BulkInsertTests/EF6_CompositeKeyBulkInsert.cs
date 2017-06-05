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
    public class EF6_CompositeKeyBulkInsert : IDisposable
    {
        private readonly TestDatabase _context;
        private readonly IList<CompositeKeyEntity> _collection;
        private readonly FirstKeyEntity _firstKeyEntity1;
        private readonly FirstKeyEntity _firstKeyEntity2;
        private readonly SecondKeyEntity _secondKeyEntity1;
        private readonly SecondKeyEntity _secondKeyEntity2;

        public EF6_CompositeKeyBulkInsert()
        {
            _context = new TestDatabase();
            ClearTable();
            _firstKeyEntity1 = new FirstKeyEntity
            {
                Name = Helper.RandomString(10)
            };
            _firstKeyEntity2 = new FirstKeyEntity
            {
                Name = Helper.RandomString(10)
            };
            _secondKeyEntity1 = new SecondKeyEntity
            {
                Name = Helper.RandomString(10)
            };
            _secondKeyEntity2 = new SecondKeyEntity
            {
                Name = Helper.RandomString(10)
            };
            _context.FirstKeyEntity.AddRange(new[] { _firstKeyEntity1, _firstKeyEntity2 });
            _context.SecondKeyEntity.AddRange(new[] { _secondKeyEntity1, _secondKeyEntity2 });
            _context.SaveChanges();
            _collection = new List<CompositeKeyEntity>
            {
                new CompositeKeyEntity
                {
                    FirstKeyEntityId = _firstKeyEntity1.Id,
                    SecondKeyEntityId = _secondKeyEntity1.Id,
                    Name = Helper.RandomString(10)
                },
                new CompositeKeyEntity
                {
                    FirstKeyEntityId = _firstKeyEntity1.Id,
                    SecondKeyEntityId = _secondKeyEntity2.Id,
                    Name = Helper.RandomString(10)
                },
                new CompositeKeyEntity
                {
                    FirstKeyEntityId = _firstKeyEntity2.Id,
                    SecondKeyEntityId = _secondKeyEntity1.Id,
                    Name = Helper.RandomString(10)
                },
                new CompositeKeyEntity
                {
                    FirstKeyEntityId = _firstKeyEntity2.Id,
                    SecondKeyEntityId = _secondKeyEntity2.Id,
                    Name = Helper.RandomString(10)
                }
            };
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
            var savedEntities = _context.CompositeKeyEntity.ToList();
            Assert.Equal(savedEntities.Count, _collection.Count);
        }

        [Fact]
        public void TestInsertedEntitiesFirstKeysCount()
        {
            _context.BulkInsert(_collection);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            var firstKeyCount1 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_firstKeyEntity1.Id));
            Assert.Equal(firstKeyCount1.Count(), 2);

            var firstKeyCount2 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_firstKeyEntity2.Id));
            Assert.Equal(firstKeyCount2.Count(), 2);
        }

        [Fact]
        public void TestInsertedEntitiesSecondKeysCount()
        {
            _context.BulkInsert(_collection);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            var secondKeyCount1 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_secondKeyEntity1.Id));
            Assert.Equal(secondKeyCount1.Count(), 2);

            var secondKeyCount2 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_secondKeyEntity2.Id));
            Assert.Equal(secondKeyCount2.Count(), 2);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly()
        {
            _context.BulkInsert(_collection);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            foreach (var entity in _collection)
            {
                var savedEntity = savedEntities
                    .Where(keyEntity => keyEntity.FirstKeyEntityId == entity.FirstKeyEntityId)
                    .SingleOrDefault(keyEntity => keyEntity.SecondKeyEntityId == entity.SecondKeyEntityId);

                Assert.NotNull(savedEntity);
                Assert.Equal(savedEntity.Name, entity.Name);
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
        public void TestInsertedEntitiesKeysCount_OutputIdentity()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            var firstKeyCount1 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_firstKeyEntity1.Id));
            Assert.Equal(firstKeyCount1.Count(), 2);

            var firstKeyCount2 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_firstKeyEntity2.Id));
            Assert.Equal(firstKeyCount2.Count(), 2);
        }

        [Fact]
        public void TestInsertedEntitiesSecondKeysCount_OutputIdentity()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            var secondKeyCount1 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_secondKeyEntity1.Id));
            Assert.Equal(secondKeyCount1.Count(), 2);

            var secondKeyCount2 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_secondKeyEntity2.Id));
            Assert.Equal(secondKeyCount2.Count(), 2);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputIdentity()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            foreach (var entity in _collection)
            {
                var savedEntity = savedEntities
                    .Where(keyEntity => keyEntity.FirstKeyEntityId == entity.FirstKeyEntityId)
                    .SingleOrDefault(keyEntity => keyEntity.SecondKeyEntityId == entity.SecondKeyEntityId);

                Assert.NotNull(savedEntity);
                Assert.Equal(savedEntity.Name, entity.Name);
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
        public void TestInsertedEntitiesKeysCount_OutputComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputComputed);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            var firstKeyCount1 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_firstKeyEntity1.Id));
            Assert.Equal(firstKeyCount1.Count(), 2);

            var firstKeyCount2 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_firstKeyEntity2.Id));
            Assert.Equal(firstKeyCount2.Count(), 2);
        }

        [Fact]
        public void TestInsertedEntitiesSecondKeysCount_OutputComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputComputed);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            var secondKeyCount1 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_secondKeyEntity1.Id));
            Assert.Equal(secondKeyCount1.Count(), 2);

            var secondKeyCount2 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_secondKeyEntity2.Id));
            Assert.Equal(secondKeyCount2.Count(), 2);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputComputed);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            foreach (var entity in _collection)
            {
                var savedEntity = savedEntities
                    .Where(keyEntity => keyEntity.FirstKeyEntityId == entity.FirstKeyEntityId)
                    .SingleOrDefault(keyEntity => keyEntity.SecondKeyEntityId == entity.SecondKeyEntityId);

                Assert.NotNull(savedEntity);
                Assert.Equal(savedEntity.Name, entity.Name);
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
        public void TestInsertedEntitiesKeysCount_OutputIdentityComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity | InsertOptions.OutputComputed);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            var firstKeyCount1 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_firstKeyEntity1.Id));
            Assert.Equal(firstKeyCount1.Count(), 2);

            var firstKeyCount2 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_firstKeyEntity2.Id));
            Assert.Equal(firstKeyCount2.Count(), 2);
        }

        [Fact]
        public void TestInsertedEntitiesSecondKeysCount_OutputIdentityComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity | InsertOptions.OutputComputed);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            var secondKeyCount1 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_secondKeyEntity1.Id));
            Assert.Equal(secondKeyCount1.Count(), 2);

            var secondKeyCount2 = savedEntities
                .Where(saved => saved.FirstKeyEntityId.Equals(_secondKeyEntity2.Id));
            Assert.Equal(secondKeyCount2.Count(), 2);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputIdentityComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity | InsertOptions.OutputComputed);
            var savedEntities = _context.CompositeKeyEntity
                .ToList();

            foreach (var entity in _collection)
            {
                var savedEntity = savedEntities
                    .Where(keyEntity => keyEntity.FirstKeyEntityId == entity.FirstKeyEntityId)
                    .SingleOrDefault(keyEntity => keyEntity.SecondKeyEntityId == entity.SecondKeyEntityId);

                Assert.NotNull(savedEntity);
                Assert.Equal(savedEntity.Name, entity.Name);
            }
        }

        #endregion

        public void Dispose()
        {
            ClearTable();
        }

        private void ClearTable()
        {
            _context.CompositeKeyEntity.RemoveRange(_context.CompositeKeyEntity.ToList());
            _context.FirstKeyEntity.RemoveRange(_context.FirstKeyEntity.ToList());
            _context.SecondKeyEntity.RemoveRange(_context.SecondKeyEntity.ToList());
            _context.SaveChanges();
        }
    }
}