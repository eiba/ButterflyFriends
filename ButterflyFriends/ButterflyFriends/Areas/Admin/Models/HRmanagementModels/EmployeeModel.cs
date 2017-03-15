using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ButterflyFriends.Models;

namespace ButterflyFriends.Areas.Admin.Models.HRmanagementModels
{
    public class EmployeeModel
    {
        public IList<ApplicationUser> Employees { get; set; }
    }
}