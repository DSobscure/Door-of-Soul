using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSDataStructure.WorldLevelStructure
{
    public class Block : GeneralWorldLevel
    {
        public Block(int uniqueID, string name, GeneralWorldLevel source)
        {
            UniqueID = uniqueID;
            Name = name;
            Level = WorldLevelEnum.Block;
            Source = source;
            SubWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
