using UnityEngine;
using System.Collections;
using DSSerializable.CharacterStructure;

public class EnterScene : MonoBehaviour {

    public GameObject ContainerPrefab;

	// Use this for initialization
	IEnumerator Start()
    {
        SceneGlobal.containerPrefab = ContainerPrefab;
        Physics.IgnoreLayerCollision(8,8,true);
        yield return null;
    }
}
