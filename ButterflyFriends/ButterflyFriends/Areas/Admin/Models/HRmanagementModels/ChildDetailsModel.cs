using ButterflyFriends.Models;

namespace ButterflyFriends.Areas.Admin.Models.HRmanagementModels
{
    public class ChildDetailsModel
    {
        public DbTables.Child Child { get; set; }
        public FileModel File { get; set; }
    }
}