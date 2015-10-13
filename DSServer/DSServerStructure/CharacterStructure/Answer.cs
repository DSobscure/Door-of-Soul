using System.Collections.Generic;
using DSSerializable.CharacterStructure;
using DSServer;

namespace DSServerStructure
{
    public class Answer
    {
        public int uniqueID { get; protected set; }
        public string name { get; protected set; }
        public int soulLimit { get; protected set; }
        public Dictionary<int, Soul> soulDictionary { get; protected set; }
        public DSPeer peer { get; protected set; }

        public Answer(int uniqueID, string name, int soulLimit, DSPeer peer)
        {
            this.uniqueID = uniqueID;
            this.name = name;
            this.soulLimit = soulLimit;
            soulDictionary = new Dictionary<int, Soul>();
            this.peer = peer;
        }

        public Answer(SerializableAnswer answer, DSPeer peer)
        {
            uniqueID = answer.UniqueID;
            name = answer.Name;
            soulLimit = answer.SoulLimit;
            soulDictionary = new Dictionary<int, Soul>();
            this.peer = peer;
        }

        public SerializableAnswer Serialize()
        {
            return new SerializableAnswer() {UniqueID = uniqueID, Name = name, SoulLimit = soulLimit };
        }
    }
}
