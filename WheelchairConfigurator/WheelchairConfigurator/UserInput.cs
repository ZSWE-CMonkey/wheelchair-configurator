using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WheelchairConfigurator
{
    public class UserInput
    {
        public double BodyHeight { get; set; }
        public double PelvisWidth { get; set; }
        public double ThighLength { get; set; }
        public double Weight { get; set; }
        public string BodyStability { get; set; } = "";
        public bool HeadStability { get; set; }
        public string BedsoreRisk { get; set; } = "";
        public bool HandControl { get; set; }
        public string Environment { get; set; } = "";
        public bool Legs { get; set; }
        public string Pain { get; set; } = "";
    }
}
