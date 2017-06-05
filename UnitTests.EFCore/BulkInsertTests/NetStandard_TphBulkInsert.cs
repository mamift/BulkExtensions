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
    public class NetStandard_TphBulkInsert : IDisposable
    {
        private readonly TestDatabase _context;
        private readonly IList<Person> _collection;

        public NetStandard_TphBulkInsert()
        {
            _context = new TestDatabase();
            ClearTable();
            _collection = new List<Person>();
            for (var i = 0; i < 10; i++)
            {
                _collection.Add(new Person
                {
                    Name = Helper.RandomString(10),
                    Birthday = DateTime.Today
                });
                _collection.Add(new Employee
                {
                    Name = Helper.RandomString(10),
                    Birthday = DateTime.Today,
                    JobTitle = Helper.RandomString(10)
                });
                _collection.Add(new Client
                {
                    Name = Helper.RandomString(10),
                    Birthday = DateTime.Today,
                    Category = Helper.RandomString(10)
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
            var simpleModels = _context.People.ToList();
            Assert.Equal(simpleModels.Count, _collection.Count);
        }

        [Fact]
        public void TestInsertedDerivedTypes()
        {
            _context.BulkInsert(_collection);
            var tphModel = _context.People.ToList();
            Assert.Equal(tphModel.OfType<Employee>().Count(), 10);
            Assert.Equal(tphModel.OfType<Client>().Count(), 10);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly()
        {
            _context.BulkInsert(_collection);
            var tphModel = _context.People
                .OrderBy(model => model.Id)
                .ToList();

            Assert.Equal(tphModel.Count, _collection.Count);
            for (var i = 0; i < _collection.Count; i++)
            {
                var entity = _collection[i];
                var saved = tphModel[i];
                Assert.Equal(entity.Name, saved.Name);
                Assert.Equal(entity.Birthday, saved.Birthday);
                var employee = entity as Employee;
                if (employee != null)
                {
                    var savedEmployee = saved as Employee;
                    Assert.NotNull(savedEmployee);
                    Assert.Equal(employee.JobTitle, savedEmployee.JobTitle);
                    continue;
                }
                var client = entity as Client;
                if (client != null)
                {
                    var savedClient = saved as Client;
                    Assert.NotNull(savedClient);
                    Assert.Equal(client.Category, savedClient.Category);
                }
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
        public void TestInsertedDerivedTypes_OutputIdentity()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity);
            var tphModel = _context.People.ToList();
            Assert.Equal(tphModel.OfType<Employee>().Count(), 10);
            Assert.Equal(tphModel.OfType<Client>().Count(), 10);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputIdentity()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity);
            var tphModel = _context.People
                .OrderBy(model => model.Id)
                .ToList();

            Assert.Equal(tphModel.Count, _collection.Count);
            for (var i = 0; i < _collection.Count; i++)
            {
                var entity = _collection[i];
                var saved = tphModel[i];

                Assert.Equal(entity.Id, saved.Id);
                Assert.Equal(entity.Name, saved.Name);
                Assert.Equal(entity.Birthday, saved.Birthday);

                var employee = entity as Employee;
                if (employee != null)
                {
                    var savedEmployee = saved as Employee;
                    Assert.NotNull(savedEmployee);
                    Assert.Equal(employee.JobTitle, savedEmployee.JobTitle);
                    continue;
                }
                var client = entity as Client;
                if (client != null)
                {
                    var savedClient = saved as Client;
                    Assert.NotNull(savedClient);
                    Assert.Equal(client.Category, savedClient.Category);
                }
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
        public void TestInsertedDerivedTypes_OutputComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputComputed);
            var tphModel = _context.People.ToList();
            Assert.Equal(tphModel.OfType<Employee>().Count(), 10);
            Assert.Equal(tphModel.OfType<Client>().Count(), 10);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputComputed);
            var tphModel = _context.People
                .OrderBy(model => model.Id)
                .ToList();

            Assert.Equal(tphModel.Count, _collection.Count);
            for (var i = 0; i < _collection.Count; i++)
            {
                var entity = _collection[i];
                var saved = tphModel[i];

                Assert.NotEqual(entity.Id, saved.Id);
                Assert.Equal(entity.Name, saved.Name);
                Assert.Equal(entity.Birthday, saved.Birthday);

                var employee = entity as Employee;
                if (employee != null)
                {
                    var savedEmployee = saved as Employee;
                    Assert.NotNull(savedEmployee);
                    Assert.Equal(employee.JobTitle, savedEmployee.JobTitle);
                    continue;
                }
                var client = entity as Client;
                if (client != null)
                {
                    var savedClient = saved as Client;
                    Assert.NotNull(savedClient);
                    Assert.Equal(client.Category, savedClient.Category);
                }
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
        public void TestInsertedEntitiesIdentities_OutputIdentityComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity | InsertOptions.OutputComputed);
            Assert.True(_collection.All(model => model.Id != 0));
        }

        [Fact]
        public void TestInsertedDerivedTypes_OutputIdentityComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity | InsertOptions.OutputComputed);
            var tphModel = _context.People.ToList();
            Assert.Equal(tphModel.OfType<Employee>().Count(), 10);
            Assert.Equal(tphModel.OfType<Client>().Count(), 10);
        }

        [Fact]
        public void TestInsertValuesSavedCorrectly_OutputIdentityComputed()
        {
            _context.BulkInsert(_collection, InsertOptions.OutputIdentity | InsertOptions.OutputComputed);
            var tphModel = _context.People
                .OrderBy(model => model.Id)
                .ToList();

            Assert.Equal(tphModel.Count, _collection.Count);
            for (var i = 0; i < _collection.Count; i++)
            {
                var entity = _collection[i];
                var saved = tphModel[i];

                Assert.Equal(entity.Id, saved.Id);
                Assert.Equal(entity.Name, saved.Name);
                Assert.Equal(entity.Birthday, saved.Birthday);

                var employee = entity as Employee;
                if (employee != null)
                {
                    var savedEmployee = saved as Employee;
                    Assert.NotNull(savedEmployee);
                    Assert.Equal(employee.JobTitle, savedEmployee.JobTitle);
                    continue;
                }
                var client = entity as Client;
                if (client != null)
                {
                    var savedClient = saved as Client;
                    Assert.NotNull(savedClient);
                    Assert.Equal(client.Category, savedClient.Category);
                }
            }
        }

        #endregion

        public void Dispose()
        {
            ClearTable();
        }

        private void ClearTable()
        {
            _context.People.RemoveRange(_context.People.ToList());
            _context.SaveChanges();
        }
    }
}