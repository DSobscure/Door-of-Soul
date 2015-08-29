using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSDataStructure.WorldLevelStructure
{
    public abstract class GeneralWorldLevel
    {
        public int UniqueID { get; protected set; }
        public string Name { get; set; }
        public WorldLevelEnum Level { get; protected set; }
        public GeneralWorldLevel Source { get; protected set; }
        public Dictionary<int,GeneralWorldLevel> SubWorldLevelDictionary { get; protected set; }
        
    }
}
