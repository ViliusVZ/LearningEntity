using NinjaDomain.Classes;
using NinjaDomain.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    static class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer(new NullDatabaseInitializer<NinjaContext>());
            //InsertNinja();
            //InsertMultipleNinjas();
            //SimpleNinjaQueries();
            //QueryAndUpdateNinja();
            //QueryAndUpdateNinjaDisconnected();
            //RetrieveDataWithFind();
            //RetrieveDataWithStoredProc();
            //DeleteNinja();
            //InsertNinjaWithEquipment();
            //SimpleNinjaGraphQuery();
            ProjectionQuery();
        }

        private static void InsertNinja()
        {
            var ninja = new Ninja
            {
                Name = "SampsonSan",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(2008, 1, 28),
                ClanId = 1
            };
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                context.Ninjas.Add(ninja);
                context.SaveChanges();
            }
        }

        private static void InsertMultipleNinjas()
        {
            var ninja1 = new Ninja
            {
                Name = "Leonardo",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(1984, 1, 1),
                ClanId = 1
            };
            var ninja2 = new Ninja
            {
                Name = "Raphael",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(1985, 1, 1),
                ClanId = 1
            };
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                context.Ninjas.AddRange(new List<Ninja> { ninja1, ninja2 });
                context.SaveChanges();
            }
        }

        private static void SimpleNinjaQueries()
        {
            using (var context = new NinjaContext())
            {
                //Could use ToList() here, but it's IQueryable, so I can enumerate
                var ninjas = context.Ninjas.Where(n => n.DateOfBirth >= new DateTime(1984, 1, 1))
                    .FirstOrDefault();

                // foreach (var ninja in ninjas)
                // {
                Console.WriteLine(ninjas.Name);
                // }

                //var query = context.Ninjas;
                // Good example
                // foreach (var ninja in query)
                // {
                //     Console.WriteLine(ninja.Name);
                // }

                // Bad example
                // foreach (var ninja in context.Ninjas)
                // {
                //     Console.WriteLine(ninja.Name);
                // }
                // Why is it bad?
                // Because for the whole iteration SQL connection
                // remains open. "Performance issues"

            }
        }

        private static void QueryAndUpdateNinja()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninja = context.Ninjas.FirstOrDefault();
                ninja.ServedInOniwaban = (!ninja.ServedInOniwaban);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Shows how to deal with updating DB while being disconnected.
        /// </summary>
        private static void QueryAndUpdateNinjaDisconnected()
        {
            Ninja ninja;
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                ninja = context.Ninjas.FirstOrDefault();
            }

            ninja.ServedInOniwaban = (!ninja.ServedInOniwaban);

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                // This
                //context.Ninjas.Add(ninja);
                // Or
                context.Ninjas.Attach(ninja);
                context.Entry(ninja).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Trick here is, that when we first call Find()
        /// it looks in the memory and sees, that we don't
        /// have this object in memory yet. But on the second
        /// "iteration" it sees that we already have this in
        /// memory and doesn't send a query, just returns 
        /// that object from memory.
        /// </summary>
        private static void RetrieveDataWithFind()
        {
            var keyval = 4;
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninja = context.Ninjas.Find(keyval);
                Console.WriteLine($"After find #1: {ninja.Name}");

                var someNinja = context.Ninjas.Find(keyval);
                Console.WriteLine($"After find #2: {ninja.Name}");
                ninja = null;
            }
        }

        /// <summary>
        /// Allows you to directly send SQL queries.
        /// Not recommended, only use it to execute stored procedures.
        /// </summary>
        private static void RetrieveDataWithStoredProc()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                //Consider ToList()
                var ninjas = context.Ninjas.SqlQuery("exec GetOldNinjas");
                foreach (var ninja in ninjas)
                {
                    Console.WriteLine(ninja.Name);
                }
            }
        }

        /// <summary>
        /// Allows you to delete object from DB.
        /// Problem here is, that to delete an object from DB it's still
        /// two trips. First to get the id, second to delete an object
        /// based on that Id. A better idea would be to execute a 
        /// stored procedure that would do it in one go.
        /// </summary>
        private static void DeleteNinja()
        {
            Ninja ninja;

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                ninja = context.Ninjas.FirstOrDefault();
            }

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                //context.Ninjas.Attach(ninja);
                //context.Ninjas.Remove(ninja);
                context.Entry(ninja).State = EntityState.Deleted;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Inserts data into the DB, specifies relationships between objects.
        /// </summary>
        private static void InsertNinjaWithEquipment()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                var ninja = new Ninja
                {
                    Name = "Kacy Catanzaro",
                    ServedInOniwaban = false,
                    DateOfBirth = new DateTime(1990, 1, 14),
                    ClanId = 1
                };

                var muscles = new NinjaEquipment
                {
                    Name = "Muscles",
                    Type = EquipmentType.Tool
                };

                var spunk = new NinjaEquipment
                {
                    Name = "Spunk",
                    Type = EquipmentType.Weapon
                };

                context.Ninjas.Add(ninja);
                // Note this
                ninja.EquipmentOwned.Add(muscles);
                ninja.EquipmentOwned.Add(spunk);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Lazy loading: a mere mention of particular object
        /// will cause EF6 to return that entire project. But to
        /// do this the object must be defined as virtual.
        /// </summary>
        private static void SimpleNinjaGraphQuery()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                //var ninja = context.Ninjas.Include(n => n.EquipmentOwned)
                //    .FirstOrDefault(n => n.Name.StartsWith("Kacy"));

                var ninja = context.Ninjas
                    .FirstOrDefault(n => n.Name.StartsWith("Kacy"));
                Console.WriteLine("Ninja Retrieved: " + ninja.Name);
                //context.Entry(ninja).Collection(n => n.EquipmentOwned).Load();

                // With making the list virtual, the result would've been actual count
                Console.WriteLine("Ninja Equipment Count: " + ninja.EquipmentOwned.Count());
            }
        }

        /// <summary>
        /// Only returns specified properties about the object.
        /// </summary>
        private static void ProjectionQuery()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninjas = context.Ninjas
                    .Select(n => new { n.Name, n.DateOfBirth, n.EquipmentOwned })
                    .ToList();

                foreach (var ninja in ninjas)
                {
                    Console.WriteLine(ninja.EquipmentOwned.Count);
                }
            }
        }
    }
}
