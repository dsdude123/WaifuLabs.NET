using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WaifuLabs.NET.Model
{
    public class ResponseData
    {
        [JsonProperty("newGirls", TypeNameHandling = TypeNameHandling.Arrays)]
        public List<Waifu> newGirls { get; set; }

        public string girl { get; set; } // Final girl in base64
    }
}
