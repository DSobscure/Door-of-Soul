using UnityEngine;
using System.Collections;
using System.Linq;
using DSSerializable.CharacterStructure;
using System;
using DSSerializable.WorldLevelStructure;

public class EnterWorldGraphAction : MonoBehaviour {

    private string getSoulListResult = "";
    private string projectToSceneResult = "";
    private string getSceneDataResult = "";

    IEnumerator Start()
    {
        PhotonGlobal.PS.GetSoulListEvent += GetSoulListEventAction;
        PhotonGlobal.PS.GetContainerListEvent += GetContainerListEventAction;
        PhotonGlobal.PS.ProjectContainerToSceneEvent += ProjectContainerToSceneEventAction;
        PhotonGlobal.PS.GetSceneDataEvent += GetSceneDataEventAction;
        PhotonGlobal.PS.ProjectToSceneEvent += ProjectToSceneEventAction;
        PhotonGlobal.PS.DisconnectEvent += DisconnectEventAction;
        PhotonGlobal.PS.UpdateContainerPositionEvent += UpdateContainerPositionEventAction;
        PhotonGlobal.PS.MoveTargetPositionEvent += MoveTargetPositionEventAction;

        PhotonGlobal.PS.GetSoulList(AnswerGlobal.Answer);
        yield return null;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(30, 10, 300, 20),getSoulListResult);
        GUI.Label(new Rect(30, 30, 300, 20), getSceneDataResult);
        GUI.Label(new Rect(30, 50, 300, 20), projectToSceneResult);
    }

    private void GetSoulListEventAction(bool getSoulListStatus, string debugMessage, SerializableSoul[] soulList)
    {
        if (getSoulListStatus)
        {
            foreach (SerializableSoul soul in soulList)
            {
                Soul targetSoul = new Soul(soul, AnswerGlobal.Answer);
                AnswerGlobal.Answer.SoulDictionary.Add(soul.UniqueID,targetSoul);
                if(soul.UniqueID == AnswerGlobal.Answer.MainSoulUniqueID)
                {
                    AnswerGlobal.MainSoul = targetSoul;
                }
            }
            
            PhotonGlobal.PS.GetContainerList(AnswerGlobal.MainSoul);
            PhotonGlobal.PS.ActiveSoul(AnswerGlobal.MainSoul);
        }
        else
        {
            getSoulListResult = debugMessage;
        }
    }
    private void GetContainerListEventAction(bool getContainerListStatus, string debugMessage, SerializableContainer[] containerList)
    {
        if (getContainerListStatus)
        {
            Soul mainSoul = AnswerGlobal.Answer.SoulDictionary[AnswerGlobal.Answer.MainSoulUniqueID];
            foreach (SerializableContainer container in containerList)
            {
                Container targetContainer = new Container(container);
                mainSoul.ContainerDictionary.Add(container.UniqueID, targetContainer);
                if(container.UniqueID == AnswerGlobal.MainSoul.MainContainerUniqueID)
                {
                    AnswerGlobal.MainContainer = targetContainer;
                }
            }
            PhotonGlobal.PS.ProjectToScene(AnswerGlobal.MainContainer, AnswerGlobal.MainContainer.LocationUniqueID);
        }
        else
        {
            getSoulListResult = debugMessage;
        }
    }
    private void ProjectContainerToSceneEventAction(int sceneUniqueID, SerializableContainer container)
    {
        if (AnswerGlobal.Scene != null && AnswerGlobal.Scene.UniqueID == sceneUniqueID)
        {
            Container targetContainer = new Container(container);
            AnswerGlobal.Scene.ContainerDictionary.Add(container.UniqueID, targetContainer);
            targetContainer.GameObject = Instantiate(AnswerGlobal.containerPrefab, new Vector3(targetContainer.PositionX, targetContainer.PositionY, targetContainer.PositionZ), Quaternion.identity) as GameObject;
        }
    }
    private void GetSceneDataEventAction(bool getSceneDataStatus, string debugMessage, SerializableScene scene, SerializableContainer[] containers)
    {
        if (getSceneDataStatus)
        {
            AnswerGlobal.Scene = new Scene(scene);
            foreach(SerializableContainer container in containers)
            {
                Container targetContainer = new Container(container);
                AnswerGlobal.Scene.ContainerDictionary.Add(container.UniqueID, targetContainer);
                targetContainer.GameObject = Instantiate(AnswerGlobal.containerPrefab, new Vector3(targetContainer.PositionX,targetContainer.PositionY,targetContainer.PositionZ) ,Quaternion.identity) as GameObject;
                if(container.UniqueID == AnswerGlobal.MainContainer.UniqueID)
                {
                    Camera.main.transform.parent = targetContainer.GameObject.transform;
                }
            }
        }
        else
        {
            getSceneDataResult = debugMessage;
        }
    }
    private void ProjectToSceneEventAction(bool projectToSceneStatus, string debugMessage)
    {
        if (projectToSceneStatus)
        {
            Application.LoadLevel("testScene");
        }
        else
        {
            projectToSceneResult = debugMessage;
        }
    }
    private void DisconnectEventAction(int[] soulUniqueIDList, int[] sceneUniqueIDList, int[] containerUniqueIDList)
    {
        if(sceneUniqueIDList.Contains(AnswerGlobal.Scene.UniqueID))
        {
            foreach (int containerUniqueID in containerUniqueIDList)
            {
                if (AnswerGlobal.Scene.ContainerDictionary.ContainsKey(containerUniqueID))
                {
                    Destroy(AnswerGlobal.Scene.ContainerDictionary[containerUniqueID].GameObject);
                    AnswerGlobal.Scene.ContainerDictionary.Remove(containerUniqueID);
                }
            }
        }
    }
    private void UpdateContainerPositionEventAction(int sceneUniqueID, int containerUniqueID, float positionX, float positionY, float positionZ)
    {
        Scene scene = AnswerGlobal.Scene;
        if (scene.UniqueID == sceneUniqueID && scene.ContainerDictionary.ContainsKey(containerUniqueID))
        {
            scene.ContainerDictionary[containerUniqueID].GameObject.GetComponent<Rigidbody>().MovePosition(new Vector3(positionX, positionY, positionZ));
            //scene.ContainerDictionary[containerUniqueID].TargetPostion = new Vector3(positionX, positionY, positionZ);
            //scene.ContainerDictionary[containerUniqueID].Moving = true;
        }
    }
    private void MoveTargetPositionEventAction(int sceneUniqueID, int containerUniqueID, float positionX, float positionY, float positionZ)
    {
        Scene scene = AnswerGlobal.Scene;
        if (scene.UniqueID == sceneUniqueID && scene.ContainerDictionary.ContainsKey(containerUniqueID))
        {
            scene.ContainerDictionary[containerUniqueID].TargetPostion = new Vector3(positionX, positionY, positionZ);
            scene.ContainerDictionary[containerUniqueID].Moving = true;
        }
    }
}
