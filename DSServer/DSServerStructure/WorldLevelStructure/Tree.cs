using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class Tree : GeneralWorldLevel
    {
        public Tree(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Tree;
            base.source = source;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
