﻿using UnityEngine;
using System.Collections;
using DSSerializable.CharacterStructure;

public class EnterScene : MonoBehaviour {

    public GameObject ContainerPrefab;

	// Use this for initialization
	IEnumerator Start()
    {
        AnswerGlobal.containerPrefab = ContainerPrefab;
        PhotonGlobal.PS.GetSceneData(AnswerGlobal.MainContainer.LocationUniqueID);
        Physics.IgnoreLayerCollision(8,8,true);
        yield return null;
    }
}
