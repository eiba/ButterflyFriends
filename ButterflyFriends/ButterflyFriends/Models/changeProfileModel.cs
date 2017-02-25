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

        [Required]
        public string Id { get; set; }
    }
}