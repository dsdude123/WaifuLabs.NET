using System;
using System.Collections.Generic;
using System.Text;

namespace WaifuLabs.NET.Model
{
    public class Waifu
    {
        public List<List<double>> seeds { get; set; }
        public String image { get; set; } // base64
    }
}
