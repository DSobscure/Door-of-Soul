using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class World : GeneralWorldLevel
    {
        public World(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.World;
            base.source = source;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
