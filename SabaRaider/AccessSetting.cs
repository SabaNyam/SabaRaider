using System;

namespace SabaRaider
{
    [Serializable()]
    public class AccessSettings
    {
        public long UserId { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSec { get; set; }
        public string AccessKey { get; set; }
        public string AccessSec { get; set; }
    }
}
