using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class Planet : GeneralWorldLevel
    {
        public Planet(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Planet;
            base.source = source;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
