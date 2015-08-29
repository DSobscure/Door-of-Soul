using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public List<Soul> SoulList { get; set; }
        public DSPeer Peer;

        public Answer(int uniqueID, string name, int soulLimit,DSPeer peer)
        {
            UniqueID = uniqueID;
            Name = name;
            SoulLimit = soulLimit;
            SoulList = new List<Soul>();
            Peer = peer;
        }

        public Answer(SerializableAnswer answer, DSPeer peer)
        {
            UniqueID = answer.UniqueID;
            Name = answer.Name;
            SoulLimit = answer.SoulLimit;
            SoulList = new List<Soul>();
            Peer = peer;
        }

        public SerializableAnswer Serialize()
        {
            return new SerializableAnswer(UniqueID, Name, SoulLimit);
        }
    }

    public class Soul
    {
        public int UniqueID { get; protected set; }
        public string Name { get; set; }
        public List<Container> ContainerList { get; set; }
        public Answer SourceAnswer { get; protected set; }

        public Soul(int uniqueID, string name, Answer sourceAnswer)
        {
            UniqueID = uniqueID;
            Name = name;
            SourceAnswer = sourceAnswer;
            ContainerList = new List<Container>();
        }

        public Soul(SerializableSoul soul, Answer sourceAnswer)
        {
            UniqueID = soul.UniqueID;
            Name = soul.Name;
            SourceAnswer = sourceAnswer;
            ContainerList = new List<Container>();
        }

        public SerializableSoul Serialize()
        {
            return new SerializableSoul(UniqueID, Name);
        }
    }

    public class Container
    {
        public int UniqueID { get; protected set; }
        public string Name { get; set; }
        public List<Soul> SoulList { get; set; }
        public Scene Location { get; set; }

        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }

        public Container(int uniqueID, string name, Scene location, float postionX, float positionY, float positionZ)
        {
            UniqueID = uniqueID;
            Name = name;
            SoulList = new List<Soul>();
            Location = location;
            PositionX = postionX;
            PositionY = positionY;
            PositionZ = positionZ;
        }

        public Container(SerializableContainer container, Scene location)
        {
            UniqueID = container.UniqueID;
            Name = container.Name;
            SoulList = new List<Soul>();
            Location = location;
            PositionX = container.PositionX;
            PositionY = container.PositionY;
            PositionZ = container.PositionZ;
        }

        public SerializableContainer Serialize()
        {
            return new SerializableContainer(UniqueID, Name, PositionX, PositionY, PositionZ);
        }
    }
}
