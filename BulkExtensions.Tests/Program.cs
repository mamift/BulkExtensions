using System;
using System.Collections.Generic;
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

                    db.BulkInsert(people, Identity.Output);
                    Console.WriteLine("People added successfully!");
                    Thread.Sleep(2000);

                    foreach (var person in people)
                    {
                        person.Name += "UPDATED";
                    }

                    db.BulkUpdate(people);
                    Console.WriteLine("People updated successfully!");
                    Thread.Sleep(2000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }            
        }
    }
}
