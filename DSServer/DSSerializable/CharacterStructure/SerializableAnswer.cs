using System;
using Newtonsoft.Json;

namespace DSSerializable.CharacterStructure
{
    public class SerializableAnswer
    {
        [JsonProperty("UniqueID")]
        public int UniqueID { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("SoulLimit")]
        public int SoulLimit { get; set; }

        public SerializableAnswer(int uniqueID, string name, int soulLimit)
        {
            UniqueID = uniqueID;
            Name = name;
            SoulLimit = soulLimit;
        }
    }
}
