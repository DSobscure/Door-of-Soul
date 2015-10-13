using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class Region : GeneralWorldLevel
    {
        public Region(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Region;
            base.source = source;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
