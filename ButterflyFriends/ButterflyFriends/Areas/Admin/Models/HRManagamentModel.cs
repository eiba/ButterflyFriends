using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ButterflyFriends.Areas.Admin.Models.HRmanagementModels;

namespace ButterflyFriends.Areas.Admin.Models
{
    public class HRManagamentModel
    {
        public EmployeeModel EmployeeModel { get; set; }
        public ChildrenModel ChildrenModel { get; set; }
        public SponsorModel SponsorModel { get; set; }
    }
}