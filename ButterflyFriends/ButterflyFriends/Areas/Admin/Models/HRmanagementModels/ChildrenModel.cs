using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ButterflyFriends.Models;
using PagedList;

namespace ButterflyFriends.Areas.Admin.Models.HRmanagementModels
{
    public class ChildrenModel
    {
        public IPagedList<DbTables.Child> Children { get; set; }
    }
}