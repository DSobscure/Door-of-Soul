using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSDataStructure.WorldLevelStructure
{
    public class Continent : GeneralWorldLevel
    {
        public Continent(int uniqueID, string name, GeneralWorldLevel source)
        {
            UniqueID = uniqueID;
            Name = name;
            Level = WorldLevelEnum.Continent;
            Source = source;
            SubWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
