using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSDataStructure.WorldLevelStructure
{
    public class Area : GeneralWorldLevel
    {
        public Area(int uniqueID, string name, GeneralWorldLevel source)
        {
            UniqueID = uniqueID;
            Name = name;
            Level = WorldLevelEnum.Area;
            Source = source;
            SubWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
