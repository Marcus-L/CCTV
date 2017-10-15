using System.Net;

namespace CCTV.Core
{
    public class TV
    {
        public string Name { get; set; }
        public string OffUrl { get; set; }
        public long SecondsIdle { get; set; } = 0;
        public float DefaultVolume { get; set; } = 0.5f;
        public IPEndPoint IP { get; set; }
    }
}
