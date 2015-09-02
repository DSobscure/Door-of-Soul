using UnityEngine;
using System.Collections;
using System;

public class ContainerMoveController : MonoBehaviour {
	
	void Update () 
    {
        if (SceneGlobal.Scene is Scene)
        {
            foreach (Container container in SceneGlobal.Scene.ContainerDictionary.Values)
            {
                if (container.Moving)
                {
                    Rigidbody rigidbody = container.GameObject.GetComponent<Rigidbody>();
                    Vector3 velocity = new Vector3(x: Convert.ToSingle(Math.Round(container.TargetPostion.x - rigidbody.position.x, 1)),
                                               y: 0,
                                               z: Convert.ToSingle(Math.Round(container.TargetPostion.z - rigidbody.position.z, 1)));
                    rigidbody.velocity = velocity.normalized * 5;
                    container.Moving = (velocity.magnitude == 0) ? false : true;
                }
            }
        }
	}
}
