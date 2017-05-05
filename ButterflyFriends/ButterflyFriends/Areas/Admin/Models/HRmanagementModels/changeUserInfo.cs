using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ButterflyFriends.Areas.Admin.Models
{
    public class changeUserInfo
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Tlf")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Gateadresse")]
        public string StreetAdress { get; set; }

        [Required]
        [Display(Name = "Postkode")]
        public int PostCode { get; set; }

        [Required]
        [Display(Name = "Kommune")]
        public string City { get; set; }

        [Required]
        [Display(Name = "Fylke")]
        public string State { get; set; }

        [Required]
        [Display(Name = "Fornavn")]
        public string Fname { get; set; }

        [Required]
        [Display(Name = "Etternavn")]
        public string Lname { get; set; }

        [StringLength(100, ErrorMessage = "{0} Må være minst {2} karakterer langt.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Passord")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Bekreft passord")]
        [Compare("Password", ErrorMessage = "Passordene må stemme overens!")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Id { get; set; }

        [Display(Name = "Stilling")]
        public string Position { get; set; }
        [Display(Name = "Bankonto")]
        public int? AccountNumber { get; set; }

        public int? RoleNr { get; set; }
    }
}