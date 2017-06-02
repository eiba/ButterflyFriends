using System.Collections.Generic;

namespace ButterflyFriends.Models
{
    public class MyImagesModel
    {
        public IList<DbTables.File> Images { get; set; }
        public int StartId { get; set; }
    }
}