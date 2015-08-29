using System.Collections.Generic;
using DSSerializable.CharacterStructure;

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

    public Container(SerializableContainer container,Scene location)
    {
        UniqueID = container.UniqueID;
        Name = container.Name;
        SoulList = new List<Soul>();
        Location = location;
        PositionX = container.PositionX;
        PositionY = container.PositionY;
        PositionZ = container.PositionZ;
    }
}
