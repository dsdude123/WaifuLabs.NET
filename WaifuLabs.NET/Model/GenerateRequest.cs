using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WaifuLabs.NET.Model
{
    public class GenerateRequest
    {
        public int id { get; set; }
        [JsonProperty("params")]
        public WaifuParameters parameters { get; set; }
    }
}
