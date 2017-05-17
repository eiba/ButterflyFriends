using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ButterflyFriends.Models
{
    public class changeProfileModel
    {
        [Required]
        [StringLength(12, ErrorMessage = "Telefonummer må være mellom 8 og 12 karakterer", MinimumLength = 8)]
        [RegularExpression(@"^[0-9+]+$", ErrorMessage = "Telefonummer kan kun vare karakterer mellom 0 og 9 og +")]
        [Display(Name = "Tlf")]
        public string Phone { get; set; }

        [StringLength(11, ErrorMessage = "Fødselsnummer må være 11 siffer", MinimumLength = 11)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Fødselsnummer kan kun vare karakterer mellom 0 og 9")]
        [Display(Name = "Fødselsnr.")]
        public string BirthNumber { get; set; }

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

        [Required]
        public string Id { get; set; }

        public DbTables.File File { get; set; }
    }
}