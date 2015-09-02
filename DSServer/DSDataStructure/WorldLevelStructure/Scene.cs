using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSSerializable.WorldLevelStructure;
using DSServer;

namespace DSDataStructure.WorldLevelStructure
{
    public class Scene : GeneralWorldLevel
    {
        public Dictionary<int, Container> ContainerDictionary { get; set; }
        public int SceneAdministratorUniqueID { get; set; }
        public DSPeer AdministratorPeer { get; set; }

        public Scene(int uniqueID, string name, GeneralWorldLevel source)
        {
            UniqueID = uniqueID;
            Name = name;
            Level = WorldLevelEnum.Scene;
            Source = source;
            SubWorldLevelDictionary = null;
            ContainerDictionary = new Dictionary<int, Container>();
        }

        public SerializableScene Serialize()
        {
            return new SerializableScene(UniqueID, Name);
        }
    }
}
