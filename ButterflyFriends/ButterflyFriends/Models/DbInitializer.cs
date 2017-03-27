using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ButterflyFriends.Models
{
    /// <summary>
    /// The database initializer. Here we seed the database with test data when we run the project.
    /// The database is current DropCreateDatabaseIfModelChanges, so it drops and reseeds the database if any of the models/database tables change
    /// </summary>
    public class DbInitializer: DropCreateDatabaseAlways<ApplicationDbContext>
    {

        /// <summary>
        /// The seed method for the database, running whenever the database is created.
        /// </summary>
        /// <param name="db"> The database in which we will add values</param>
        protected override void Seed(ApplicationDbContext db)
        {

            // These two managers handle storage in the given db context for us
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            roleManager.Create(new IdentityRole("Admin")); // rights to view admin 
            roleManager.Create(new IdentityRole("Employee")); // The employee role
            roleManager.Create(new IdentityRole("Sponsor")); //the sponsor role
            roleManager.Create(new IdentityRole("Owner"));// The owner of the website

            //Adding different test values
            var ownerAdress = new DbTables.Adresses
            {
                City = "Oslo",
                PostCode = 4744,
                County = "Akershus",
                StreetAdress = "Kongensvei 9"
            };

            db.Adresses.Add(ownerAdress);

            var owner = new ApplicationUser
            {
                UserName = "Owner@butterflyfriends.no",
                Email = "Owner@butterflyfriends.no",
                Fname = "Mr.",
                Lname = "Owner",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Owner",
                IsEnabeled = true,
                Phone = "96347584",
                Employee = new DbTables.Employees { Position = "Daglig Leder", AccountNumber = 53355335 }
            };
            userManager.Create(owner, "Password1.");
            userManager.AddToRole(owner.Id, "Owner");

            var user1 = new ApplicationUser
            {
                UserName = "some@butterflyfriends.no",
                Email = "some@butterflyfriends.no",
                Fname = "Frank",
                Lname = "Åsnes",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                Phone = "96347584"
            };
            userManager.Create(user1, "Password1.");
            userManager.AddToRole(user1.Id, "Sponsor");

            var user2 = new ApplicationUser
            {
                UserName = "some2@butterflyfriends.no",
                Email = "some2@butterflyfriends.no",
                Fname = "Janne",
                Lname = "Fiskeman",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                Phone = "96347584"
            };
            userManager.Create(user2, "Password1.");
            userManager.AddToRole(user2.Id, "Sponsor");

            var user3 = new ApplicationUser
            {
                UserName = "some3@butterflyfriends.no",
                Email = "some3@butterflyfriends.no",
                Fname = "Ole",
                Lname = "Ingeredsen",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                Phone = "96347584"
            };
            userManager.Create(user3, "Password1.");
            userManager.AddToRole(user3.Id, "Sponsor");

            var user4 = new ApplicationUser
            {
                UserName = "some4@butterflyfriends.no",
                Email = "some4@butterflyfriends.no",
                Fname = "Mari",
                Lname = "Flatland",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                Phone = "96347584"
            };
            userManager.Create(user4, "Password1.");
            userManager.AddToRole(user4.Id, "Sponsor");

            var user6 = new ApplicationUser
            {
                UserName = "som6@butterflyfriends.no",
                Email = "some6@butterflyfriends.no",
                Fname = "Jan",
                Lname = "Johansen",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                Phone = "96347584"
            };
            userManager.Create(user6, "Password1.");
            userManager.AddToRole(user6.Id, "Sponsor");

            var user7 = new ApplicationUser
            {
                UserName = "some7@butterflyfriends.no",
                Email = "some7@butterflyfriends.no",
                Fname = "Lisa",
                Lname = "Buadhl",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                Phone = "96347584"
            };
            userManager.Create(user7, "Password1.");
            userManager.AddToRole(user7.Id, "Sponsor");

            var user8 = new ApplicationUser
            {
                UserName = "some8@butterflyfriends.no",
                Email = "some8@butterflyfriends.no",
                Fname = "Tor",
                Lname = "Hammer",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                Phone = "96347584"
            };
            userManager.Create(user8, "Password1.");
            userManager.AddToRole(user8.Id, "Sponsor");

            var user9 = new ApplicationUser
            {
                UserName = "some9@butterflyfriends.no",
                Email = "some9@butterflyfriends.no",
                Fname = "Karoline",
                Lname = "Sørsvann",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                Phone = "96347584"
            };
            userManager.Create(user9, "Password1.");
            userManager.AddToRole(user9.Id, "Sponsor");

            var user10 = new ApplicationUser
            {
                UserName = "some10@butterflyfriends.no",
                Email = "some10@butterflyfriends.no",
                Fname = "Lars",
                Lname = "Møllestad",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                Phone = "96347584"
            };
            userManager.Create(user10, "Password1.");
            userManager.AddToRole(user10.Id, "Sponsor");

            var user11 = new ApplicationUser
            {
                UserName = "some11@butterflyfriends.no",
                Email = "some11@butterflyfriends.no",
                Fname = "Camilla",
                Lname = "Bam",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                Phone = "96347584"
            };
            userManager.Create(user11, "Password1.");
            userManager.AddToRole(user11.Id, "Sponsor");

            var user12 = new ApplicationUser
            {
                UserName = "some12@butterflyfriends.no",
                Email = "some12@butterflyfriends.no",
                Fname = "Asgeir",
                Lname = "Millen",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                Phone = "96347584"
            };
            userManager.Create(user12, "Password1.");
            userManager.AddToRole(user12.Id, "Sponsor");

            var employee1 = new ApplicationUser
            {
                UserName = "employee1@butterflyfriends.no",
                Email = "employee1@butterflyfriends.no",
                Fname = "Åse",
                Lname = "Klevland",
                AdressId = ownerAdress.AdressId,
                AccessLvL = "Employee",
                IsEnabeled = true,
                Phone = "96347584",
                Employee = new DbTables.Employees{Position = "Regnskapsfører",AccountNumber = 23452345}
            };
            userManager.Create(employee1, "Password1.");
            userManager.AddToRole(employee1.Id, "Employee");

            db.Children.Add(new DbTables.Child
            {
                Fname = "Mohammed",
                Lname = "Bali",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            });
            db.Children.Add(new DbTables.Child
            {
                Fname = "Sasha",
                Lname = "Dababa",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            });
            db.Children.Add(new DbTables.Child
            {
                Fname = "Moduba",
                Lname = "Badabi",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            }); db.Children.Add(new DbTables.Child
            {
                Fname = "Shika",
                Lname = "Shinshi",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            }); db.Children.Add(new DbTables.Child
            {
                Fname = "Abdu",
                Lname = "Bali",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            });

            db.Children.Add(new DbTables.Child
            {
                Fname = "Ekon",
                Lname = "Isabis",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            });
            db.Children.Add(new DbTables.Child
            {
                Fname = "Maha",
                Lname = "Bahati",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            });
            db.Children.Add(new DbTables.Child
            {
                Fname = "Kwame",
                Lname = "Jafari",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            });
            db.Children.Add(new DbTables.Child
            {
                Fname = "Uma",
                Lname = "Maha",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            });
            db.Children.Add(new DbTables.Child
            {
                Fname = "Lulu",
                Lname = "Kalifa",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            });
            db.Children.Add(new DbTables.Child
            {
                Fname = "Amare",
                Lname = "Ode",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            });
            db.Children.Add(new DbTables.Child
            {
                Fname = "Asha",
                Lname = "haji",
                DoB = new DateTime(2005, 6, 19),
                isActive = true,
            });
            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Arendal",
                Description = "Please I wanna join",
                Email = "Bojackman@gmail.com",
                Fname = "Hans",
                Lname = "Johansen",
                Phone = "34343434",
                PostCode = 3434,
                State = "Aust-Agder",
                StreetAdress = "Neptunveien 9"
            });
            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Kristiansand",
                Description = "Hi I wanna help",
                Email = "Nobro@gmail.com",
                Fname = "Egil",
                Lname = "André",
                Phone = "34343434",
                PostCode = 4554,
                State = "Vest-Agder",
                StreetAdress = "Åsveien 34"
            });
            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Ålesund",
                Description = "Hi I wanna help",
                Email = "Ayay@gmail.com",
                Fname = "Mari",
                Lname = "Almdahl",
                Phone = "34343434",
                PostCode = 3434,
                State = "Rogaland",
                StreetAdress = "Brabra 34"
            });
            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Oslo",
                Description = "Hi I wanna help",
                Email = "kari@gmail.com",
                Fname = "Kari",
                Lname = "Hisland",
                Phone = "34343434",
                PostCode = 4554,
                State = "Akershus",
                StreetAdress = "Kongsvei 34"
            });
            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Kristiansand",
                Description = "Hi I wanna help",
                Email = "kuman@gmail.com",
                Fname = "Lisa",
                Lname = "Kuman",
                Phone = "34343434",
                PostCode = 4554,
                State = "Vest-Agder",
                StreetAdress = "Kvadraturen 56"
            });
            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Hammerfest",
                Description = "Hi I wanna help",
                Email = "ali@gmail.com",
                Fname = "Mohammed",
                Lname = "Ali",
                Phone = "34343434",
                PostCode = 6566,
                State = "Finmark",
                StreetAdress = "Snøland 67"
            });
            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Lillehammer",
                Description = "Hi I wanna help",
                Email = "hal@gmail.com",
                Fname = "Halgeir",
                Lname = "Helgeir",
                Phone = "34343434",
                PostCode = 4554,
                State = "Something",
                StreetAdress = "Hammervei 6"
            });
            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Chicago",
                Description = "Hi I wanna help",
                Email = "mike@gmail.com",
                Fname = "Mike",
                Lname = "Hefner",
                Phone = "34343434",
                PostCode = 4554,
                State = "Virginia",
                StreetAdress = "Queenstreet 7"
            });
            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Bergen",
                Description = "Hi I wanna help",
                Email = "sunn@gmail.com",
                Fname = "Sunniva",
                Lname = "Olsen",
                Phone = "34343434",
                PostCode = 4554,
                State = "Hordaland",
                StreetAdress = "Nygård 56"
            });
            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Bergen",
                Description = "Hi I wanna help",
                Email = "Nobro@gmail.com",
                Fname = "Jan",
                Lname = "Olsen",
                Phone = "34343434",
                PostCode = 4554,
                State = "Hordaland",
                StreetAdress = "Nygård 56"
            });

            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Stavanger",
                Description = "Hi I wanna help",
                Email = "sta@gmail.com",
                Fname = "Jun",
                Lname = "Johansen",
                Phone = "34343434",
                PostCode = 4554,
                State = "Hordaland",
                StreetAdress = "Vangvei 45"
            });

            db.MembershipRequests.Add(new DbTables.MembershipRequest
            {
                City = "Stavanger",
                Description = "Hi I wanna help",
                Email = "sta@gmail.com",
                Fname = "Egge",
                Lname = "Johansen",
                Phone = "34343434",
                PostCode = 4554,
                State = "Hordaland",
                StreetAdress = "Vangvei 45"
            });
            db.SaveChanges();



            //This method is for when you want to make a database diagram, of the database.
            //Uncomment and change DropCreateDatabaseIfModelChanges to DropCreateDatabaseAlways
            //to run this method upon database creation.
            
            
            using (var ctx = new ApplicationDbContext())
            {
                using (var writer = new XmlTextWriter(@"d:\Documents\Model.edmx", Encoding.Default))
                {
                    EdmxWriter.WriteEdmx(ctx, writer);
                }
            }
        }
    }
}