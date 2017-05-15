using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ButterflyFriends.Models
{
    public class FrontPageModel
    {
        public IList<DbTables.Article> Articles { get; set; }
        public IList<CarouselObject> Carousel { get; set; }
        public DbTables.Info About { get; set; }

    }

    public class CarouselObject
    {
        public string type { get; set; }
        public int id { get; set; }

    }
}