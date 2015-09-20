using UnityEngine;
using System.Collections;
using System;

public class ContainerMoveController : MonoBehaviour {
	
	void FixedUpdate() 
    {
        if (SceneGlobal.ControlTheSceneStatus)
        {
            foreach (Container container in SceneGlobal.Scene.ContainerDictionary.Values)
            {
                container.PositionX = container.GameObject.transform.position.x;
                container.PositionY = container.GameObject.transform.position.y;
                container.PositionZ = container.GameObject.transform.position.z;
                container.EulerAngleY = container.GameObject.transform.rotation.y;
                if (container.Moving)
                {
                    Vector3 velocity = new Vector3(x: Convert.ToSingle(Math.Round(container.TargetPostion.x - container.PositionX, 1)),
                                           y: 0,
                                           z: Convert.ToSingle(Math.Round(container.TargetPostion.z - container.PositionZ, 1)));
                    container.GameObject.GetComponent<Rigidbody>().velocity = velocity.normalized * 5;
                    if (velocity.magnitude == 0)
                    {
                        container.Moving = false;
                        container.TargetPostion = container.GameObject.transform.position;
                    }
                }
            }
        }
	}

}
