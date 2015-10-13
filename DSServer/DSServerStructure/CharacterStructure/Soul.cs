using System.Collections.Generic;
using DSSerializable.CharacterStructure;

namespace DSServerStructure
{
    public class Soul
    {
        public int uniqueID { get; protected set; }
        public string name { get; protected set; }
        public Dictionary<int, Container> containerDictionary { get; protected set; }
        public Answer sourceAnswer { get; protected set; }
        public bool Active { get; set; }

        public Soul(int uniqueID, string name, Answer sourceAnswer)
        {
            this.uniqueID = uniqueID;
            this.name = name;
            this.sourceAnswer = sourceAnswer;
            containerDictionary = new Dictionary<int, Container>();
        }

        public Soul(SerializableSoul soul, Answer sourceAnswer)
        {
            uniqueID = soul.UniqueID;
            name = soul.Name;
            this.sourceAnswer = sourceAnswer;
            containerDictionary = new Dictionary<int, Container>();
        }

        public SerializableSoul Serialize()
        {
            return new SerializableSoul() { UniqueID = uniqueID, Name = name };
        }
    }
}
