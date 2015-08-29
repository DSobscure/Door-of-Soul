using System.Collections.Generic;
using DSSerializable.CharacterStructure;

public class Soul
{
    public int UniqueID { get; protected set; }
    public string Name { get; set; }
    public List<Container> ContainerList { get; set; }
    public Answer SourecAnswer { get; protected set; }

    public Soul(int uniqueID, string name, Answer answer)
    {
        UniqueID = uniqueID;
        Name = name;
        SourecAnswer = answer;
        ContainerList = new List<Container>();
    }

    public Soul(SerializableSoul soul,Answer sourceAnswer)
    {
        UniqueID = soul.UniqueID;
        Name = soul.Name;
        SourecAnswer = sourceAnswer;
        ContainerList = new List<Container>();
    }
}
