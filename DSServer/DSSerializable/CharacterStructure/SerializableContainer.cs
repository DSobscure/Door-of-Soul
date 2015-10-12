using System;
using Newtonsoft.Json;

namespace DSSerializable.CharacterStructure
{
    public class SerializableContainer
    {
        [JsonProperty("UniqueID")]
        public int UniqueID { get; protected set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("LocationUniqueID")]
        public int LocationUniqueID { get; set; }

        [JsonProperty("PositionX")]
        public float PositionX { get; set; }
        [JsonProperty("PositionY")]
        public float PositionY { get; set; }
        [JsonProperty("PositionZ")]
        public float PositionZ { get; set; }
        [JsonProperty("EulerAngleY")]
        public float EulerAngleY { get; set; }


        public SerializableContainer(int uniqueID, string name, int locationUniqueID, float postionX, float positionY, float positionZ, float eulerAngleY)
        {
            UniqueID = uniqueID;
            Name = name;
            LocationUniqueID = locationUniqueID;
            PositionX = postionX;
            PositionY = positionY;
            PositionZ = positionZ;
            EulerAngleY = eulerAngleY;
        }
    }
}
