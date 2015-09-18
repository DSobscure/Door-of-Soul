using System.Collections.Generic;
using DSDataStructure.WorldLevelStructure;
using DSSerializable.CharacterStructure;
using DSServer;

namespace DSDataStructure
{
    public class Answer
    {
        public int UniqueID { get; protected set; }
        public string Name { get; set; }
        public int SoulLimit { get; protected set; }
        public Dictionary<int,Soul> SoulDictionary { get; set; }
        public DSPeer Peer;
        public int MainSoulUniqueID { get; set; }

        public Answer(int uniqueID, string name, int soulLimit, int mainSoulUniqueID, DSPeer peer)
        {
            UniqueID = uniqueID;
            Name = name;
            SoulLimit = soulLimit;
            MainSoulUniqueID = mainSoulUniqueID;
            SoulDictionary = new Dictionary<int, Soul>();
            Peer = peer;
        }

        public Answer(SerializableAnswer answer, DSPeer peer)
        {
            UniqueID = answer.UniqueID;
            Name = answer.Name;
            SoulLimit = answer.SoulLimit;
            MainSoulUniqueID = answer.MainSoulUniqueID;
            SoulDictionary = new Dictionary<int, Soul>();
            Peer = peer;
        }

        public SerializableAnswer Serialize()
        {
            return new SerializableAnswer(UniqueID, Name, SoulLimit, MainSoulUniqueID);
        }
    }

    public class Soul
    {
        public int UniqueID { get; protected set; }
        public string Name { get; set; }
        public Dictionary<int, Container> ContainerDictionary { get; set; }
        public Answer SourceAnswer { get; protected set; }
        public int MainContainerUniqueID { get; set; }
        public bool Active { get; set; }

        public Soul(int uniqueID, string name, int mainContainerUniqueID, Answer sourceAnswer)
        {
            UniqueID = uniqueID;
            Name = name;
            MainContainerUniqueID = mainContainerUniqueID;
            SourceAnswer = sourceAnswer;
            ContainerDictionary = new Dictionary<int,Container>();
        }

        public Soul(SerializableSoul soul, Answer sourceAnswer)
        {
            UniqueID = soul.UniqueID;
            Name = soul.Name;
            MainContainerUniqueID = soul.MainContainerUniqueID;
            SourceAnswer = sourceAnswer;
            ContainerDictionary = new Dictionary<int,Container>();
        }

        public SerializableSoul Serialize()
        {
            return new SerializableSoul(UniqueID, Name, MainContainerUniqueID);
        }
    }

    public class Container
    {
        public int UniqueID { get; protected set; }
        public string Name { get; set; }
        public Dictionary<int, Soul> SoulDictionary { get; set; }
        public Scene Location { get; set; }

        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float EulerAngleY { get; set; }

        public Container(int uniqueID, string name, Scene location, float postionX, float positionY, float positionZ,float eulerAngleY)
        {
            UniqueID = uniqueID;
            Name = name;
            SoulDictionary = new Dictionary<int,Soul>();
            Location = location;
            PositionX = postionX;
            PositionY = positionY;
            PositionZ = positionZ;
            EulerAngleY = eulerAngleY;
        }

        public Container(SerializableContainer container, Scene location)
        {
            UniqueID = container.UniqueID;
            Name = container.Name;
            SoulDictionary = new Dictionary<int,Soul>();
            Location = location;
            PositionX = container.PositionX;
            PositionY = container.PositionY;
            PositionZ = container.PositionZ;
            EulerAngleY = container.EulerAngleY;
        }

        public SerializableContainer Serialize()
        {
            return new SerializableContainer(UniqueID, Name, Location.UniqueID, PositionX, PositionY, PositionZ, EulerAngleY);
        }
    }
}
