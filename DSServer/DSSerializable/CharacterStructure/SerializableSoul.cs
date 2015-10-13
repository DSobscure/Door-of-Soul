using System;
using Newtonsoft.Json;

namespace DSSerializable.CharacterStructure
{
    public class SerializableSoul
    {
        [JsonProperty("UniqueID")]
        public int UniqueID { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}
