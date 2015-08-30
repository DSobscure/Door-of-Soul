using System.Collections.Generic;
using DSSerializable.CharacterStructure;

public class Answer
{
    public int UniqueID { get; protected set; }
    public string Name { get; set; }
    public int SoulLimit { get; protected set; }
    public Dictionary<int,Soul> SoulDictionary { get; set; }
    public int MainSoulUniqueID { get; set; }

    public Answer(int uniqueID, string name, int soulLimit, int mainSoulUniqueID)
    {
        UniqueID = uniqueID;
        Name = name;
        SoulLimit = soulLimit;
        MainSoulUniqueID = mainSoulUniqueID;
        SoulDictionary = new Dictionary<int, Soul>();
    }

    public Answer(SerializableAnswer answer)
    {
        UniqueID = answer.UniqueID;
        Name = answer.Name;
        MainSoulUniqueID = answer.MainSoulUniqueID;
        SoulLimit = answer.SoulLimit;
        SoulDictionary = new Dictionary<int, Soul>();
    }
}
