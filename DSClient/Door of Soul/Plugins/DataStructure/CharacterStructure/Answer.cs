using System.Collections.Generic;
using DSSerializable.CharacterStructure;

public class Answer
{
    public int UniqueID { get; protected set; }
    public string Name { get; set; }
    public int SoulLimit { get; protected set; }
    public List<Soul> SoulList { get; set; }

    public Answer(int uniqueID, string name, int soulLimit)
    {
        UniqueID = uniqueID;
        Name = name;
        SoulLimit = soulLimit;
        SoulList = new List<Soul>();
    }

    public Answer(SerializableAnswer answer)
    {
        UniqueID = answer.UniqueID;
        Name = answer.Name;
        SoulLimit = answer.SoulLimit;
        SoulList = new List<Soul>();
    }
}
