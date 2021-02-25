using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream
{
    public class Image
    {

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }


        [JsonConstructor]
        internal Image()
        {
        }
    }
}
