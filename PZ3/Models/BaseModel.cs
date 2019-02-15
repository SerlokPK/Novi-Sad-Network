using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opet.Models
{
    public class BaseModel
    {
        public List<Substation> Substation { get; set; }
        public List<Node> Node { get; set; }
        public List<Switch> Switch { get; set; }
        public List<Line> Line { get; set; }
    }
}
