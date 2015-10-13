using System;
using Newtonsoft.Json;

namespace DSSerializable.WorldLevelStructure
{
    public class SerializableScene
    {
        [JsonProperty("UniqueID")]
        public int UniqueID { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}
