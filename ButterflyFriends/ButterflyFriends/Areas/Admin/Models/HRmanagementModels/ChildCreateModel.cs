using ButterflyFriends.Models;

namespace ButterflyFriends.Areas.Admin.Models.HRmanagementModels
{
    public class ChildCreateModel
    {
        public DbTables.Child Child { get; set; }
        public string SponsorName { get; set; }
    }
}