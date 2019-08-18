using System.Collections.Generic;
using System.Net;

namespace CCTV.Core
{
    public class TV
    {
        public string Name { get; set; }
        public List<Command> OnCommands { get; set; }
        public List<Command> OffCommands { get; set; }
        public long SecondsIdle { get; set; } = 0;
        public float DefaultVolume { get; set; } = 0.5f;
        public IPEndPoint IP { get; set; }
    }
}
