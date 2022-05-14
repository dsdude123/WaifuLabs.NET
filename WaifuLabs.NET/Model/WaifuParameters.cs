using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WaifuLabs.NET.Model
{
    public class WaifuParameters
    {
        [JsonProperty("currentGirl", NullValueHandling = NullValueHandling.Ignore)]
        public string currentGirl { get; set; }
        public int step { get; set; }
    }
}
