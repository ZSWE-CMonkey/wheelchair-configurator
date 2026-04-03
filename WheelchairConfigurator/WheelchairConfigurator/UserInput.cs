using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WheelchairConfigurator
{
    class UserInput
    {
        public double bodyHeight {  get; set; }
        public double pelvisWidth { get; set; }
        public double thighLength { get; set; }
        public double weight { get; set;}
        public string bodyStability {  get; set; }

        public bool headStability { get; set; }
        public string bedsoreRisk { get; set; }
        public bool handControl { get; set; }

        public string enviroment { get; set;}

        public bool legs {  get; set; }

        public bool pain { get; set;}

    }
}
