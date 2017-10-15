using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace CCTV.Core
{
    public class Settings
    {
        public List<TV> TVs { get; set; }
        public int PollIntervalSeconds { get; set; } = 5;
        public int IdleTimeout { get; set; } = 60;

        public static Settings Load(string filename)
        {
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(filename));
        }
    }
}
