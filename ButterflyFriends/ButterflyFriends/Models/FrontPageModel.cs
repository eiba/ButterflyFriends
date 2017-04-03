using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ButterflyFriends.Models
{
    public class FrontPageModel
    {
        public IList<DbTables.Article> Articles { get; set; }
    }
}