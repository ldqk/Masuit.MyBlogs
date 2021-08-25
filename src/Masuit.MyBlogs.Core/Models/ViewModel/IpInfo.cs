using MaxMind.GeoIP2.Responses;

namespace Masuit.MyBlogs.Core.Models.ViewModel
{
    public class IpInfo
    {
        public string Address { get; set; }
        public CityResponse CityInfo { get; set; }
        public AsnResponse Asn { get; set; }
        public bool IsProxy { get; set; }
        public string TimeZone { get; set; }
    }
}