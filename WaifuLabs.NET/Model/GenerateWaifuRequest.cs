using System;
using System.Collections.Generic;
using System.Text;

namespace WaifuLabs.NET.Model
{
    public class GenerateWaifuRequest
    {
        public List<List<double>> currentGirl { get; set; }
        public int size { get; set; }
        public int step { get; set; }
    }
}
