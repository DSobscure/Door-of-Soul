using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class Area : GeneralWorldLevel
    {
        public Area(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Area;
            base.source = source;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
