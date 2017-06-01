using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ButterflyFriends.Models;
using PagedList;

namespace ButterflyFriends.Areas.Admin.Models
{
    public class DonationModel
    {
        public IPagedList<DbTables.Donations> Donations { get; set; }
        public IPagedList<DbTables.Subscriptions> Plans { get; set; }
        public IPagedList<Subscriber> Subscribers { get; set; }

    }

    public class Subscriber
    {
    
    }
}