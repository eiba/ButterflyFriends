using System.Collections.Generic;
using ButterflyFriends.Models;
using PagedList;

namespace ButterflyFriends.Areas.Admin.Models
{
    public class DonationModel
    {
        public IPagedList<DbTables.Donations> Donations { get; set; }
        public IPagedList<DbTables.Subscriptions> Plans { get; set; }
        public IList<Subscription> Subscriptions { get; set; }
    }

    public class Subscription
    {
        public string SubId { get; set; }
        public string PlanId { get; set; }
        public string CustomerId { get; set; }
        public string PlanName { get; set; }
        public int Amount { get; set; }
    }
}