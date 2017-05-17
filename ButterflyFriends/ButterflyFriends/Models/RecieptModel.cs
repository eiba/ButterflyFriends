using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ButterflyFriends.Models
{
    public class RecieptModel
    {
        public DbTables.Donations Donation { get; set; }
        public DbTables.Facebook Facebook { get; set; }
        public DbTables.Twitter Twitter { get; set; }

    }
}