using System.Collections.Generic;
using DSSerializable.CharacterStructure;
using UnityEngine;

public class Container
{
    public int UniqueID { get; protected set; }
    public string Name { get; set; }
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

    public Vector3 TargetPostion { get; set; }
    public bool Moving { get; set; }

    public Container(int uniqueID, string name, int locationUniqueID, float postionX, float positionY, float positionZ)
    {
        UniqueID = uniqueID;
        Name = name;
        LocationUniqueID = locationUniqueID;
        PositionX = postionX;
        PositionY = positionY;
        PositionZ = positionZ;
    }

    public Container(SerializableContainer container)
    {
        UniqueID = container.UniqueID;
        Name = container.Name;
        LocationUniqueID = container.LocationUniqueID;
        PositionX = container.PositionX;
        PositionY = container.PositionY;
        PositionZ = container.PositionZ;
    }
}
