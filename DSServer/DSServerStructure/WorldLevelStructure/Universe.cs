using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class Universe : GeneralWorldLevel
    {
        public Universe(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Universe;
            base.source = source;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
