using System.Collections.Generic;

namespace DSServerStructure.WorldLevelStructure
{
    public abstract class GeneralWorldLevel
    {
        public int uniqueID { get; protected set; }
        public string name { get; protected set; }
        public WorldLevelEnum level { get; protected set; }
        public GeneralWorldLevel source { get; protected set; }
        public Dictionary<int,GeneralWorldLevel> subWorldLevelDictionary { get; protected set; }
        
    }
}
