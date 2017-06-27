using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Chapter2ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //QueryContacts();
            //QueryContactsLinq();
            //QueryContactsLambda();
            //EagerLoading();
            //Edit();
            //CreateEntity();
            //reateParentChildEntity();
            //RemoveEnitiy();
            //VariousMethods();
            //MergeOptionExample();
            //ExecuteOption();
            ExecuteOption();
        }

        private static void QueryContacts()
        {
            using (var context = new SampleEntities())
            {
                var contacts = context.Contacts;
                foreach (var contact in contacts)
                {
                    Console.WriteLine($"{contact.FirstName.Trim()} {contact.LastName}");
                }
                Console.WriteLine("Perss Enter...");
                Console.ReadLine();
            }
        }
        private static void QueryContactsLinq()
        {
            using (var context = new SampleEntities())
            {
                var contacts = from c in context.Contacts
                               where c.FirstName == "Robert"
                               select c;
                foreach (var contact in contacts)
                {
                    Console.WriteLine($"{contact.FirstName.Trim()} {contact.LastName}");
                }
                Console.WriteLine("Perss Enter...");
                Console.ReadLine();
            }
        }

        private static void QueryContactsLambda()
        {
            using (var context = new SampleEntities())
            {
                var contacts = context.Contacts.Where(c => c.FirstName == "Robert").OrderBy(foo => foo.LastName);
                foreach (var contact in contacts)
                {
                    Console.WriteLine($"{contact.FirstName.Trim()} {contact.LastName}");
                }
                Console.WriteLine("Perss Enter...");
                Console.ReadLine();
            }
        }

        private static void LinqProjections()
        {
            using (var context = new SampleEntities())
            {
                var contacts = context.Contacts.Where(c => c.FirstName == "Robert").OrderBy(foo => foo.LastName).Select(c => new {c.Title, c.FirstName, c.LastName});
                foreach (var contact in contacts)
                {
                    Console.WriteLine($"{contact.FirstName.Trim()} {contact.LastName}");
                }
                Console.WriteLine("Perss Enter...");
                Console.ReadLine();
            }
        }

        private static void LetProjectionWithLinq()
        {

            using (var context = new SampleEntities())
            {
                var contacts = from c in context.Contacts
                    where c.FirstName == "Robert"
                    let contactName = new {c.Title, c.LastName, c.FirstName}
                    select contactName;

                foreach (var contact in contacts)
                {
                    Console.WriteLine($"{contact.FirstName.Trim()} {contact.LastName}");
                }

                var contacts1 =
                    from c in context.Contacts
                    where c.FirstName == "Robert"
                    let foo = new
                    {
                        ContactName = new { c.Title, c.LastName, c.FirstName },
                        c.Addresses //this participates in change tracking and DB updates
                    }
                    orderby foo.ContactName.LastName
                    select foo;

                foreach (var contact in contacts1)
                {
                    var name = contact.ContactName;
                    Console.WriteLine(
                        $"{name.Title.Trim()} {name.FirstName.Trim()} {name.LastName.Trim()}: # Addresses {contact.Addresses.Count}");
                }

                var addresses = context.Addresses.Where(a => a.CountryRegion == "UK").Select(a => new
                {
                    a,
                    a.Contact.FirstName,
                    a.Contact.LastName
                });
                
                foreach (var address in addresses)
                {
                    Console.WriteLine($"{address.FirstName} {address.LastName} {address.a.Street1} {address.a.City}");
                }

                var shapedResults = from c in context.Contacts
                    select new
                    {
                        c.FirstName,
                        c.LastName,
                        StreetsCities = from a in c.Addresses
                            select new {a.Street1, a.City}
                    };

                var shapedResultsLinq = context.Contacts.Select(c => new
                {
                    c.FirstName,
                    c.LastName,
                    StreetsCities = c.Addresses.Select(a => new {a.Street1, a.City})
                });

                var flattenedResults =
                    from a in context.Addresses
                    orderby a.Contact.LastName
                    select new {a.Contact.LastName, a.Contact.FirstName, a.Street1, a.City};

                Console.WriteLine("Perss Enter...");
                Console.ReadLine();
            }
        }

        private static void Joins()
        {
            using (var context = new SampleEntities())
            {
                var joinQuery = from c in context.Contacts
                    join oa in context.vOfficeAddresses on c.ContactID equals oa.ContactID
                    select new
                    {
                        oa.FirstName,
                        oa.LastName,
                        c.Title,
                        oa.Street1,
                        oa.City,
                        oa.StateProvince
                    };

                var nestedQuery = context.vOfficeAddresses.Select(oa => new
                {
                    oa.FirstName,
                    oa.LastName,
                    Title = context.Contacts.Where(c => c.ContactID == oa.ContactID).Select(c => c.Title).FirstOrDefault(),
                    oa.Street1,
                    oa.City,
                    oa.StateProvince
                });
            }
        }

        private static void Grouping()
        {
            using (var context = new SampleEntities())
            {
                var groupQuery = from c in context.Contacts
                    group c by c.Title
                    into myGroup
                    orderby myGroup.Key
                    select new {MyTitle = myGroup.Key, MyGroup = myGroup};
            }
        }
        private static void ChainingAggregates()
        {
            using (var context = new SampleEntities())
            {
                var chainingAggregatesQuery =
                    context.Contacts.GroupBy(c => c.Title).OrderBy(myGroup => myGroup.Key).Select(myGroup => new
                    {
                        MyTitle = myGroup.Key,
                        MyGroup = myGroup,
                        Max = myGroup.Max(c => c.AddDate),
                        Count = myGroup.Count()
                    });
            }
        }
        private static void EagerLoading()
        {
            using (var context = new SampleEntities())
            {
                var contacts = context.Contacts.Include(a => a.Addresses).Select(c => c).ToList();
                foreach (var contact in contacts)
                {
                    Console.WriteLine($"{contact.FirstName.Trim()} {contact.LastName} {contact.Addresses.Count}");
                }
                Console.WriteLine("Perss Enter from eager...");
                Console.ReadLine();
            }
        }

        private static void Edit()
        {
            using (var context = new SampleEntities())
            {
                var contact = context.Contacts.First();
                contact.FirstName = "Julia";
                contact.ModifiedDate = DateTime.Now;
                context.SaveChanges();
            }
        }

        private static void CreateEntity()
        {
            using (var context = new SampleEntities())
            {
                var contact = context.Contacts.First(c => c.FirstName == "Robert");
                var address = new Address
                {
                    Street1 = "One Main Street",
                    City = "Burlington",
                    StateProvince = "VT",
                    AddressType = "Business",
                    ModifiedDate = DateTime.Now,
                    Contact = contact
                };
                contact.Addresses.Add(address);
                
                //join the new address to the contact
                context.SaveChanges();
            }
        }
        private static void CreateParentChildEntity()
        {
            using (var context = new SampleEntities())
            {
                var contact = new Contact
                {
                    FirstName = "Camey",
                    LastName = "Combs",
                    AddDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    Addresses = new List<Address>()
                };
                var address = new Address
                {
                    Street1 = "One Main Street",
                    City = "Olympia",
                    StateProvince = "WA",
                    AddressType = "Business",
                    ModifiedDate = DateTime.Now,
                    Contact = contact
                };

                var contact1 = new Contact
                {
                    FirstName = "Camey",
                    LastName = "Combs",
                    AddDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    Addresses = new List<Address> { new Address
                    {
                        Street1 = "One Main Street",
                        City = "Olympia",
                        StateProvince = "WA",
                        AddressType = "Business",
                        ModifiedDate = DateTime.Now
                    }}
                };
                context.Contacts.Add(contact1);

                contact.Addresses.Add(address);
                //join the new address to the contact
                //add the new graph to the context
                context.Contacts.Add(contact);
                context.SaveChanges();
            }
        }

        private static void RemoveEnitiy()
        {
            using (var context = new SampleEntities())
            {
                var contact = context.Contacts.Find(438);

                if (contact != null)
                    context.Contacts.Remove(contact);

                context.SaveChanges();
            }
        }

        private static void VariousMethods()
        {
            using (var context = new SampleEntities())
            {
                var contacts = context.Contacts.Where(c => c.FirstName == "Robert");
                
                var list = contacts.ToList();
                Console.WriteLine(contacts.ToString());
                Console.ReadLine();
            }
        }
        private static void MergeOptionExample()
        {
            using (var context = new SampleEntities())
            {
                var ctx = ((IObjectContextAdapter) context).ObjectContext.CreateObjectSet<Contact>();
                
                var contactsQuery = ctx.Select(m => m);
                ((ObjectQuery) contactsQuery).MergeOption = MergeOption.NoTracking;
                var list = contactsQuery.ToList();

                foreach (var address in list)
                {
                    Console.WriteLine(address.FirstName);
                }

                Console.ReadLine();
            }
        }

        private static void MergeOptionExampleObjectSet()
        {
            using (var context = new SampleEntities())
            {
                var contactsQuery = context.ContactsObjectSet.Where(c => c.FirstName == "Robert");
                ((ObjectQuery)contactsQuery).MergeOption = MergeOption.PreserveChanges;
                var results = contactsQuery.ToList();

                foreach (var address in results)
                {
                    Console.WriteLine(address.FirstName);
                }

                Console.ReadLine();
            }
        }

        private static void ExecuteOption()
        {
            using (var context = new SampleEntities())
            {
                var list = context.ContactsObjectSet.Execute(MergeOption.NoTracking).ToList();
                
                foreach (var address in list)
                {
                    Console.WriteLine(address.FirstName);
                }
                Console.ReadLine();
            }
        }
    }
}
