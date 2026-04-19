using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WheelchairConfigurator
{
    public class UserInput
    {
        public string patientIdentificator { get; set; } = String.Empty;
        public double BodyHeight { get; set; } = 0;
        public double PelvisWidth { get; set; } = 0;
        public double ThighLength { get; set; } = 0;
        public double Weight { get; set; } = 0;
        public string BodyStability { get; set; } = String.Empty;
        public bool HeadStability { get; set; } = true;
        public string BedsoreRisk { get; set; } = String.Empty;
        public string Control { get; set; } = String.Empty;
        public string Environment { get; set; } = String.Empty;
        public bool Legs { get; set; } = true;
        public string Pain { get; set; } = String.Empty;
        public DateTime Date { get; set; } = DateTime.Today;
    }
}
