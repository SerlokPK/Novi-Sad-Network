using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opet.Models
{
    public class Line 
    {
        public double Id { get; set; }
        public string Name { get; set; }
        public bool IsUnderground { get; set; }
        public double R { get; set; }
        public string ConductorMaterial { get; set; }
        public string LineType { get; set; }
        public double ThermalConstantHeat { get; set; }
        public double FirstEnd { get; set; }
        public double SecondEnd { get; set; }
        public List<Point> Vertices { get; set; }
    }
}
