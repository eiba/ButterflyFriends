using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using ButterflyFriends.Areas.Admin.Controllers;

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

            [Required]
            [Display(Name = "Fornavn")]
            public string Fname { get; set; }
            [Required]
            [Display(Name = "Etternavn")]
            public string Lname { get; set; }
            [Required]
            [DataType(DataType.DateTime)]
            [Display(Name = "Fødselsdato")]
            public DateTime DoB { get; set; }
            [ForeignKey("User")]
            public string SponsorId { get; set; }
            public bool isActive { get; set; }

            public virtual ApplicationUser User { get; set; }
            public virtual IList<File> Pictures { get; set; }
            public virtual ThumbNail Thumbnail { get; set; }
        }
        
        [Table("Picture")]
        public class File
        {
            [Key]
            public int FileId { get; set; }
            [StringLength(255)]
            public string FileName { get; set; }
            [StringLength(100)]
            public string ContentType { get; set; }
            public byte[] Content { get; set; }
            public FileType FileType { get; set; }

            public virtual IList<TagBox> Tags { get; set; }

            public virtual IList<Child> Children { get; set; }
            public virtual IList<ApplicationUser> User { get; set; }
            public virtual ThumbNail ThumbNail { get; set; }
        }

        [Table("Thumbnail")]
        public class ThumbNail
        {
            [Key]
            public int ThumbNailId { get; set; }
            [StringLength(255)]
            public string ThumbNailName { get; set; }
            [StringLength(100)]
            public string ContentType { get; set; }
            public byte[] Content { get; set; }
            public FileType FileType { get; set; }

            [Required]
            public virtual File File { get; set; }

        }

        public class TagBox
        {
            [Key]
            public int TagId { get; set; }
            public string type { get; set; }
            public string Id { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string Name { get; set; }
            //public virtual File picture {get; set; }
        }
        public enum FileType
        {
            Picture = 1,Profile,Thumbnail
        }
    }
}