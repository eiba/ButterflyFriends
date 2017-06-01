using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ButterflyFriends.Models;

namespace ButterflyFriends.Areas.Admin.Models
{
    public class VariousModel
    {
        public DbTables.GoogleCaptchaAPI GoogleCaptchaAPI { get; set; }
        public DbTables.SendGridAPI SendGridAPI { get; set; }
        public DbTables.StripeAPI StripeAPI { get; set; }
        public DbTables.TermsOfUse Terms { get; set; }
        public DbTables.Carousel Carousel { get; set; }
        public DbTables.Info About { get; set; }
        public DbTables.Twitter Twitter { get; set; }
        public DbTables.Facebook Facebook { get; set; }
        public DbTables.BackgroundImage Background { get; set; }
        public DbTables.Disqus Disqus { get; set; }

    }

    public class AboutAdress
    {
        [Display(Name = "Gateadresse")]
        public string StreetAdress { get; set; }
        [Display(Name = "Postkode")]
        public int? PostCode { get; set; }
        [Display(Name = "Fylke")]
        public string County { get; set; }
        [Display(Name = "By")]
        public string City { get; set; }

    }
}