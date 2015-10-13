using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class Space : GeneralWorldLevel
    {
        public Space(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Space;
            base.source = source;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
