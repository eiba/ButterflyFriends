using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ButterflyFriends.Models
{
    public class MyImagesModel
    {
        public IList<DbTables.File> Images { get; set; }
        public int StartId { get; set; }
    }
}