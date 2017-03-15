using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ButterflyFriends.Models
{
    /// <summary>
    /// Application user table, for all users in the database, employee does in addition
    /// have an employee table.
    /// </summary>
    [Table("ApplicationUser")]
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Fornavn")]
        public string Fname { get; set; }
        [Display(Name = "Etternavn")]
        public string Lname { get; set; }
        [ForeignKey("Adress")]
        public int AdressId { get; set; }
        [Display(Name = "Tlf")]
        public string Phone { get; set; }
        public string AccessLvL { get; set; }
        public bool IsEnabeled { get; set; }
        public int RoleNr { get; set; }

        public virtual DbTables.Adresses Adress { get; set; }
        public virtual IList<DbTables.Child> Children { get; set; }
        public virtual DbTables.Employees Employee { get; set; }
        public virtual IList<DbTables.File> Pictures { get; set; }
        //public virtual IList<DbTables.TagBox> TagBoxs { get; set; }

        /// <summary>
        /// Method for asynchronously generating unique and complex ids for the user accounts.
        /// </summary>
        /// <param name="manager">The manager gets and handles everything about the users in the database.</param>
        /// <remarks>Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType</remarks>
        /// <returns>Returns the the id for the user</returns>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    /// <summary>
    /// The database class, containing all the database tables
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        //The database tables.
        public DbSet<DbTables.Adresses> Adresses { get; set; }
        public DbSet<DbTables.Employees> Employees { get; set; }
        public DbSet<DbTables.Child> Children { get; set; }
        public DbSet<DbTables.File> Files { get; set; }

        public DbSet<DbTables.TagBox> TagBoxs { get; set; }
        /// <summary>
        /// Creates the database
        /// </summary>
        /// <returns>Returns a new database object</returns>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}