using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class Continent : GeneralWorldLevel
    {
        public Continent(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Continent;
            base.source = source;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
