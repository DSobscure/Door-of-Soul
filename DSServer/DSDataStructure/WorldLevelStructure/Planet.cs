using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSDataStructure.WorldLevelStructure
{
    public class Planet : GeneralWorldLevel
    {
        public Planet(int uniqueID, string name, GeneralWorldLevel source)
        {
            UniqueID = uniqueID;
            Name = name;
            Level = WorldLevelEnum.Planet;
            Source = source;
            SubWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
