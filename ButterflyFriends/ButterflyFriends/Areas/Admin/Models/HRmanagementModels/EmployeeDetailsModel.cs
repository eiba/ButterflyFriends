using ButterflyFriends.Models;

namespace ButterflyFriends.Areas.Admin.Models.HRmanagementModels
{
    public class EmployeeDetailsModel
    {
        public ApplicationUser User { get; set; }
        public FileModel File { get; set; }
    }
}