using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class Galaxy : GeneralWorldLevel
    {
        public Galaxy(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Galaxy;
            base.source = source;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
