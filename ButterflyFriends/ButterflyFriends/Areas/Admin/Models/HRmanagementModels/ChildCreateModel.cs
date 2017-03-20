using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ButterflyFriends.Models;

namespace ButterflyFriends.Areas.Admin.Models.HRmanagementModels
{
    public class ChildCreateModel
    {
        public DbTables.Child Child { get; set; }
        public string SponsorName { get; set; }
    }
}