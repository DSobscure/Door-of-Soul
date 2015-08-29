using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSDataStructure.WorldLevelStructure
{
    public class Star : GeneralWorldLevel
    {
        public Star(int uniqueID, string name, GeneralWorldLevel source)
        {
            UniqueID = uniqueID;
            Name = name;
            Level = WorldLevelEnum.Star;
            Source = source;
            SubWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
