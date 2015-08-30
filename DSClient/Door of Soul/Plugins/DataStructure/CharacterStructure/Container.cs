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

    private float _positionX;
    private float _positionY;
    private float _positionZ;
    public float PositionX 
    {
        get
        {
            if (GameObject != null)
                return GameObject.transform.position.x;
            else
                return _positionX;
        } 
        set
        {
            _positionX = value;
        }
    }
    public float PositionY
    {
        get
        {
            if (GameObject != null)
                return GameObject.transform.position.y;
            else
                return _positionY;
        }
        set
        {
            _positionY = value;
        }
    }
    public float PositionZ
    {
        get
        {
            if (GameObject != null)
                return GameObject.transform.position.z;
            else
                return _positionZ;
        }
        set
        {
            _positionZ = value;
        }
    }

    public Container(int uniqueID, string name, int locationUniqueID, float postionX, float positionY, float positionZ)
    {
        UniqueID = uniqueID;
        Name = name;
        SoulDictionary = new Dictionary<int, Soul>();
        LocationUniqueID = locationUniqueID;
        PositionX = postionX;
        PositionY = positionY;
        PositionZ = positionZ;
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
    }
}
