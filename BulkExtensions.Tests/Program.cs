using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using BulkExtensions.Tests.Database;
using BulkExtensions.Tests.Model;
using EntityFramework.BulkExtensions.Operations;

namespace BulkExtensions.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var db = new TestDatabase())
                {
                    db.Database.Initialize(true);
                    Console.WriteLine("Database created successfully!");

                    var people = new List<Person>
                    {
                        new Person
                        {
                            Name = "Nobody",
                            Birthday = DateTime.Now
                        },
                        new Employee
                        {
                            Name = "John",
                            Birthday = DateTime.Now,
                            JobTitle = "Developer"
                        },
                        new Client
                        {
                            Name = "Doe",
                            Birthday = DateTime.Now,
                            Category = "New"
                        }
                    };

                    db.BulkInsert(people, Options.OutputIdentity);
                    Console.WriteLine("People added successfully!");

                    foreach (var person in people)
                    {
                        person.Name += "UPDATED";
                    }

                    db.BulkUpdate(people);
                    Console.WriteLine("People updated successfully!");
                    using (var db2 = new TestDatabase())
                    {
                        var person = db2.Persons.Single(p => p.Id == 1);
                        person.Name = "context2";
                        db2.SaveChanges();
                    }
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
