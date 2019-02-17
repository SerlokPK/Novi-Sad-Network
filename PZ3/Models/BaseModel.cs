using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opet.Models
{
    public class BaseModel
    {
        public List<Substation> Substations { get; set; }
        public List<Node> Nodes { get; set; }
        public List<Switch> Switches { get; set; }
        public List<Line> Lines { get; set; }
    }
}
