using System;

namespace DSSerializable.CharacterStructure
{
    [Serializable]
    public class SerializableContainer
    {
        public int UniqueID { get; protected set; }
        public string Name { get; set; }
        public int LocationUniqueID { get; set; }

        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }

        public SerializableContainer(int uniqueID, string name, int locationUniqueID, float postionX, float positionY, float positionZ)
        {
            UniqueID = uniqueID;
            Name = name;
            LocationUniqueID = locationUniqueID;
            PositionX = postionX;
            PositionY = positionY;
            PositionZ = positionZ;
        }
    }
}
