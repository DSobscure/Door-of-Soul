using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class Star : GeneralWorldLevel
    {
        public Star(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Star;
            base.source = source;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
