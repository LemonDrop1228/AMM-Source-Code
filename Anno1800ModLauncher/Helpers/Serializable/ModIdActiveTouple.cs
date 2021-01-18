using System;
using System.Collections.Generic;
using System.Text;

namespace SerializableModinfo
{
    public class ModIdActiveTouple
    {
        public ModIdActiveTouple() { }
        public String ModID { get; set; }
        public bool Active { get; set; }
        public String Version { get; set; }
    }
}
