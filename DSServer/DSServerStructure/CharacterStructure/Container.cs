using System.Collections.Generic;
using DSServerStructure.WorldLevelStructure;
using DSSerializable.CharacterStructure;
using DSObjectStructure;

namespace DSServerStructure
{
    public class Container
    {
        public int uniqueID { get; protected set; }
        public string name { get; protected set; }
        public Dictionary<int, Soul> soulDictionary { get; protected set; }
        public Scene location { get; protected set; }
        public Inventory inventory { get; protected set; } 
        
        public float positionX { get; protected set; }
        public float positionY { get; protected set; }
        public float positionZ { get; protected set; }
        public float eulerAngleX { get; protected set; }
        public float eulerAngleY { get; protected set; }
        public float eulerAngleZ { get; protected set; }

        public Container(int uniqueID, string name, Scene location, float positionX, float positionY, float positionZ,float eulerAngleY,Inventory inventory)
        {
            this.uniqueID = uniqueID;
            this.name = name;
            soulDictionary = new Dictionary<int,Soul>();
            this.location = location;
            this.positionX = positionX;
            this.positionY = positionY;
            this.positionZ = positionZ;
            this.eulerAngleY = eulerAngleY;
            this.inventory = inventory;
        }

        public Container(SerializableContainer container, Scene location)
        {
            uniqueID = container.UniqueID;
            name = container.Name;
            soulDictionary = new Dictionary<int,Soul>();
            this.location = location;
            positionX = container.PositionX;
            positionY = container.PositionY;
            positionZ = container.PositionZ;
            eulerAngleY = container.EulerAngleY;
        }

        public SerializableContainer Serialize()
        {
            return new SerializableContainer() {
                UniqueID = uniqueID,
                Name = name,
                LocationUniqueID = location.uniqueID,
                PositionX = positionX,
                PositionY = positionY,
                PositionZ = positionZ,
                EulerAngleY = eulerAngleY,
                Inventory = inventory
            };
        }

        public void UpdatePosition(float x,float y,float z)
        {
            positionX = x;
            positionY = y;
            positionZ = z;
        }
        public void UpdateEulerAngle(float x,float y,float z)
        {
            eulerAngleX = x;
            eulerAngleY = y;
            eulerAngleZ = z;
        }
    }
}
