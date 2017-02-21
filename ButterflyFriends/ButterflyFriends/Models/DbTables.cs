using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ButterflyFriends.Models
{
    // This class contains all the database tabels except the user table. See the class "IdentityModels.cs" for the ApplicationUser table.

    /// <summary>
    /// Database table for adresses.
    /// </summary>
    public class DbTables
    {

        /// <summary>
        /// Database table for adresses.
        /// </summary>
        [Table("Adresses")]
        public class Adresses
        {
            [Key]
            public int AdressId { get; set; }

            [Required]
            [Display(Name = "By")]
            public string City { get; set; }
            [Required]
            [Display(Name = "Postkode")]
            public int PostCode { get; set; }
            [Required]
            [Display(Name = "Fylke")]
            public string County { get; set; }
            [Required]
            [Display(Name = "Gateadresse")]
            public string StreetAdress { get; set; }

            public virtual IList<ApplicationUser> User { get; set; }

        }

        /// <summary>
        /// Database tables for employees
        /// </summary>
        [Table("Employees")]
        public class Employees
        {
            [Key, ForeignKey("User")]
            public string EmployeeId { get; set; }

            [Display(Name="Bankonto")]
            public string BankAccount { get; set; }
            [Display(Name = "Stilling")]
            public string Position { get; set; }

            public virtual ApplicationUser User { get; set; }
        }

        /// <summary>
        /// Database table for the children
        /// </summary>
        [Table("Child")]
        public class Child
        {
            [Key]
            public int Id { get; set; }
            [Display(Name = "Fornavn")]
            public string Fname { get; set; }
            [Display(Name = "Etternavn")]
            public string Lname { get; set; }
            [Display(Name = "Fødselsdato")]
            public string DoB { get; set; }

            public virtual ApplicationUser User { get; set; }
            public virtual IList<File> Pictures { get; set; }
        }
        
        [Table("Picture")]
        public class File
        {
            public int FileId { get; set; }
            [StringLength(255)]
            public string FileName { get; set; }
            [StringLength(100)]
            public string ContentType { get; set; }
            public byte[] Content { get; set; }
            public FileType FileType { get; set; }
            public virtual IList<Child> Children { get; set; }
            public virtual IList<ApplicationUser> User { get; set; }
        }

        public enum FileType
        {
            Picture = 1
        }
    }
}