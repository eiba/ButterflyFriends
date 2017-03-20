using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ButterflyFriends.Models;

namespace ButterflyFriends.Areas.Admin.Models.HRmanagementModels
{
    public class EmployeeDetailsModel
    {
        public ApplicationUser User { get; set; }
        public FileModel File { get; set; }
    }
}