using System;

namespace DSSerializable.CharacterStructure
{
    [Serializable]
    public class SerializableSoul
    {
        public int UniqueID { get; protected set; }
        public string Name { get; set; }
        public int MainContainerUniqueID { get; set; }

        public SerializableSoul(int uniqueID, string name, int mainContainerUniqueID)
        {
            UniqueID = uniqueID;
            Name = name;
            MainContainerUniqueID = mainContainerUniqueID;
        }
    }
}
