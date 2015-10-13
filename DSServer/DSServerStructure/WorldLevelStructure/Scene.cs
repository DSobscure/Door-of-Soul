using System.Collections.Generic;
using DSSerializable.WorldLevelStructure;
using DSServer;

namespace DSServerStructure.WorldLevelStructure
{
    public class Scene : GeneralWorldLevel
    {
        public Dictionary<int, Container> containerDictionary { get; protected set; }
        public int administratorUniqueID { get; protected set; }
        public DSPeer administratorPeer { get; protected set; }

        public Scene(int uniqueID, string name, GeneralWorldLevel source)
        {
            base.uniqueID = uniqueID;
            base.name = name;
            level = WorldLevelEnum.Scene;
            base.source = source;
            subWorldLevelDictionary = null;
            containerDictionary = new Dictionary<int, Container>();
        }

        public SerializableScene Serialize()
        {
            return new SerializableScene() { UniqueID = uniqueID, Name = name };
        }

        public void SetAdministrator(int uniqueID,DSPeer peer)
        {
            administratorUniqueID = uniqueID;
            administratorPeer = peer;
        }
    }
}
