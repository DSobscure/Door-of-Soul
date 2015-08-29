using System;

namespace DSSerializable.WorldLevelStructure
{
    [Serializable]
    public class SerializableScene
    {
        public int UniqueID { get; protected set; }
        public string Name { get; set; }

        public SerializableScene(int uniqueID, string name)
        {
            UniqueID = uniqueID;
            Name = name;
        }
    }
}
