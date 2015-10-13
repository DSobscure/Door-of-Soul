using System;
using Newtonsoft.Json;
using DSObjectStructure;

namespace DSSerializable.CharacterStructure
{
    public class SerializableContainer
    {
        [JsonProperty("UniqueID")]
        public int UniqueID { get; set; }
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

        [JsonProperty("Inventory")]
        public Inventory Inventory { get; set; }
    }
}
