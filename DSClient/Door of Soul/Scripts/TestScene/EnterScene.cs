using UnityEngine;
using System.Collections;
using DSSerializable.CharacterStructure;

public class EnterScene : MonoBehaviour {

    public GameObject ContainerPrefab;

	// Use this for initialization
	IEnumerator Start()
    {
        AnswerGlobal.containerPrefab = ContainerPrefab;
        Physics.IgnoreLayerCollision(8,8,true);
        PhotonGlobal.PS.GetSceneData(AnswerGlobal.MainContainer.LocationUniqueID);
        yield return null;
    }
}
