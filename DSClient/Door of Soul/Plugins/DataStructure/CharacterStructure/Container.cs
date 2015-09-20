using System.Collections.Generic;
using DSSerializable.CharacterStructure;
using UnityEngine;

public class Container
{
    public int UniqueID { get; protected set; }
    public string Name { get; set; }
    public Dictionary<int, Soul> SoulDictionary { get; set; }
    public int LocationUniqueID { get; set; }
    public GameObject GameObject { get; set; }

    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float EulerAngleY;

    public Vector3 TargetPostion { get; set; }
    public bool Moving { get; set; }

    public Container(int uniqueID, string name, int locationUniqueID, float postionX, float positionY, float positionZ, float eulerAngleY)
    {
        UniqueID = uniqueID;
        Name = name;
        SoulDictionary = new Dictionary<int, Soul>();
        LocationUniqueID = locationUniqueID;
        PositionX = postionX;
        PositionY = positionY;
        PositionZ = positionZ;
        EulerAngleY = eulerAngleY;
    }

    public Container(SerializableContainer container)
    {
        UniqueID = container.UniqueID;
        Name = container.Name;
        SoulDictionary = new Dictionary<int,Soul>();
        LocationUniqueID = container.LocationUniqueID;
        PositionX = container.PositionX;
        PositionY = container.PositionY;
        PositionZ = container.PositionZ;
        EulerAngleY = container.EulerAngleY;
    }
}
