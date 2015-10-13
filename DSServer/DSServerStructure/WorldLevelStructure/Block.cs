using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class Block : GeneralWorldLevel
    {
        public Block(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Block;
            base.source = source;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
