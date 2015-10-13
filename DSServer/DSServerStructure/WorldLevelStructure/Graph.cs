using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public class Graph : GeneralWorldLevel
    {
        public Graph(int uniqueID,string name)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Graph;
            subWorldLevelDictionary = new Dictionary<int, GeneralWorldLevel>();
        }
    }
}
