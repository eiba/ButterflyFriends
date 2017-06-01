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
        public SubReciept SubReciept { get; set; }

    }

    public class SubReciept
    {
        public string Id { get; set; }
        public string referenceId { get; set; }
        public int Amount { get; set; }
    }
}