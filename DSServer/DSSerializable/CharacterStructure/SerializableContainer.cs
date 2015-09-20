using System;

namespace DSSerializable.CharacterStructure
{
    [Serializable]
    public class SerializableContainer
    {
        public int UniqueID { get; protected set; }
        public string Name { get; set; }
        public int LocationUniqueID { get; set; }

        public float PositionX { get { return Convert.ToSingle(_positionX); } }
        public float PositionY { get { return Convert.ToSingle(_positionY); } }
        public float PositionZ { get { return Convert.ToSingle(_positionZ); } }
        public float EulerAngleY { get { return Convert.ToSingle(_eulerAngleY); } }

        private string _positionX;
        private string _positionY;
        private string _positionZ;
        private string _eulerAngleY;


        public SerializableContainer(int uniqueID, string name, int locationUniqueID, float postionX, float positionY, float positionZ, float eulerAngleY)
        {
            UniqueID = uniqueID;
            Name = name;
            LocationUniqueID = locationUniqueID;
            _positionX = postionX.ToString();
            _positionY = positionY.ToString();
            _positionZ = positionZ.ToString();
            _eulerAngleY = eulerAngleY.ToString();
        }
    }
}
