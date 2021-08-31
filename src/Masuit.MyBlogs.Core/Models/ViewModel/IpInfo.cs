using MaxMind.GeoIP2.Model;

namespace Masuit.MyBlogs.Core.Models.ViewModel
{
    public class IpInfo
    {
        public string Address { get; set; }
        public Location Location { get; set; }
        public NetworkInfo Network { get; set; }
        public bool IsProxy { get; set; }
        public string TimeZone { get; set; }
        public string Domain { get; set; }
        public string Address2 { get; set; }
    }

    public class NetworkInfo
    {
        public string Router { get; set; }
        public long? Asn { get; set; }
        public string Organization { get; set; }
    }
}