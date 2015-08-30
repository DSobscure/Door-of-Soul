using System.Collections.Generic;
using DSSerializable.CharacterStructure;

public class Soul
{
    public int UniqueID { get; protected set; }
    public string Name { get; set; }
    public Dictionary<int, Container> ContainerDictionary { get; set; }
    public Answer SourecAnswer { get; protected set; }
    public int MainContainerUniqueID { get; set; }

    public Soul(int uniqueID, string name, int mainContainerUniqueID, Answer answer)
    {
        UniqueID = uniqueID;
        Name = name;
        MainContainerUniqueID = mainContainerUniqueID;
        SourecAnswer = answer;
        ContainerDictionary = new Dictionary<int,Container>();
    }

    public Soul(SerializableSoul soul,Answer sourceAnswer)
    {
        UniqueID = soul.UniqueID;
        Name = soul.Name;
        MainContainerUniqueID = soul.MainContainerUniqueID;
        SourecAnswer = sourceAnswer;
        ContainerDictionary = new Dictionary<int,Container>();
    }
}
