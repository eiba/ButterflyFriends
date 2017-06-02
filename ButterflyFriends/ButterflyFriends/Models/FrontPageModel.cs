using System.Collections.Generic;

namespace ButterflyFriends.Models
{
    public class FrontPageModel
    {
        public IList<DbTables.Article> Articles { get; set; }
        public IList<CarouselObject> Carousel { get; set; }
        public DbTables.Info About { get; set; }
        public Donations Donations { get; set; }
    }

    public class CarouselObject
    {
        public string type { get; set; }
        public int id { get; set; }
    }

    public class Donations
    {
        public IList<DbTables.Subscriptions> Subscriptions { get; set; }
        public string DonationText { get; set; }
    }
}