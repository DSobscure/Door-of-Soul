using UnityEngine;
using System.Collections;

public class ContainerPositionUpdate : MonoBehaviour 
{
	void Start () 
    {
        InvokeRepeating("UpdateContainerPosition", 0f, 5f);
	}

    void UpdateContainerPosition()
    {
        if (SceneGlobal.Scene is Scene)
        {
            foreach (Container container in SceneGlobal.Scene.ContainerDictionary.Values)
            {
                PhotonGlobal.PS.ContainerPositionUpdate(SceneGlobal.SceneUniqueID, container.UniqueID, container.PositionX, container.PositionY, container.PositionZ);
            }
        }
    }
}
