using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ButterflyFriends.Models;

namespace ButterflyFriends.Areas.Admin.Models
{
    public class VariousModel
    {
        public DbTables.GoogleCaptchaAPI GoogleCaptchaAPI { get; set; }
        public DbTables.SendGridAPI SendGridAPI { get; set; }

        public DbTables.File File { get; set; }
        public DbTables.Carousel Carousel { get; set; }


    }
    
}