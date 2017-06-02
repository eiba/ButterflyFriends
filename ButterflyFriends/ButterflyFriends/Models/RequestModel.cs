namespace ButterflyFriends.Models
{
    public class RequestModel
    {
        public DbTables.MembershipRequest MembershipRequest { get; set; }
        public string SiteKey { get; set; }
        public int TermsID { get; set; }
    }
}