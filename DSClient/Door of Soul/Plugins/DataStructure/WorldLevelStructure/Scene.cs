using System.Collections.Generic;
using DSSerializable.WorldLevelStructure;

public class Scene
{
    public int UniqueID { get; protected set; }
    public string Name { get; set; }

    public Dictionary<int, Container> ContainerDictionary { get; set; }

    public Scene(int uniqueID, string name)
    {
        UniqueID = uniqueID;
        Name = name;
        ContainerDictionary = new Dictionary<int, Container>();
    }

    public Scene(SerializableScene scene)
    {
        UniqueID = scene.UniqueID;
        Name = scene.Name;
        ContainerDictionary = new Dictionary<int, Container>();
    }
}
