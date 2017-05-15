using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ButterflyFriends.Models
{
    public class AboutModel
    {
        public DbTables.Info About { get; set; }
        public DbTables.Facebook Facebook { get; set; }
        public DbTables.Twitter Twitter { get; set; }
    }
}