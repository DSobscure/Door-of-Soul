using UnityEngine;
using System.Collections;
using DSSerializable.CharacterStructure;

public class EnterScene : MonoBehaviour {

    public GameObject ContainerPrefab;

	// Use this for initialization
	IEnumerator Start()
    {
        AnswerGlobal.containerPrefab = ContainerPrefab;
        PhotonGlobal.PS.GetSceneData(AnswerGlobal.MainContainer.LocationUniqueID);
        PhotonGlobal.PS.ProjectContainerToSceneEvent += ProjectContainerToSceneEventAction;

        yield return null;
    }

    private void ProjectContainerToSceneEventAction(int sceneUniqueID, SerializableContainer container)
    {
        Debug.Log("I know");
        if (AnswerGlobal.Scene != null && AnswerGlobal.Scene.UniqueID == sceneUniqueID)
        {
            Container targetContainer = new Container(container);
            AnswerGlobal.Scene.ContainerDictionary.Add(container.UniqueID, targetContainer);
            targetContainer.GameObject = Instantiate(AnswerGlobal.containerPrefab, new Vector3(targetContainer.PositionX, targetContainer.PositionY, targetContainer.PositionZ), Quaternion.identity) as GameObject;
        }
    }
}
