using System;

namespace DSSerializable.CharacterStructure
{
    [Serializable]
    public class SerializableAnswer
    {
        public int UniqueID { get; protected set; }
        public string Name { get; set; }
        public int SoulLimit { get; protected set; }

        public SerializableAnswer(int uniqueID, string name, int soulLimit)
        {
            UniqueID = uniqueID;
            Name = name;
            SoulLimit = soulLimit;
        }
    }
}
