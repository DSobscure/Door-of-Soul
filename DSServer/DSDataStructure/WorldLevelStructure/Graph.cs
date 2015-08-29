using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSDataStructure.WorldLevelStructure
{
    public class Graph : GeneralWorldLevel
    {
        public Graph(int uniqueID,string name)
        {
            UniqueID = uniqueID;
            Name = name;
            Level = WorldLevelEnum.Graph;
            SubWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
