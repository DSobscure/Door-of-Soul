using UnityEngine;
using System.Collections;
using System.Linq;
using DSSerializable.CharacterStructure;
using System;
using DSSerializable.WorldLevelStructure;
using DSProtocol;
using System.Collections.Generic;

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
        PhotonGlobal.PS.SendMessageEvent += SendMessageEventAction;

        PhotonGlobal.PS.GetSoulList(AnswerGlobal.Answer);
        yield return null;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(30, 10, 300, 20),getSoulListResult);
        GUI.Label(new Rect(30, 30, 300, 20), getSceneDataResult);
        GUI.Label(new Rect(30, 50, 300, 20), projectToSceneResult);
    }

    private void GetSoulListEventAction(bool getSoulListStatus, string debugMessage, List<SerializableSoul> soulList)
    {
        if (getSoulListStatus)
        {
            foreach (SerializableSoul soul in soulList)
            {
                Soul targetSoul = new Soul(soul, AnswerGlobal.Answer);
                AnswerGlobal.Answer.SoulDictionary.Add(soul.UniqueID,targetSoul);
            }
            AnswerGlobal.Answer.MainSoulUniqueID = soulList[0].UniqueID;
            AnswerGlobal.MainSoul = AnswerGlobal.Answer.SoulDictionary[soulList[0].UniqueID];
            PhotonGlobal.PS.GetContainerList(AnswerGlobal.MainSoul);
            PhotonGlobal.PS.ActiveSoul(AnswerGlobal.MainSoul);
        }
        else
        {
            getSoulListResult = debugMessage;
        }
    }
    private void GetContainerListEventAction(bool getContainerListStatus, string debugMessage, List<SerializableContainer> containerList)
    {
        if (getContainerListStatus)
        {
            Soul mainSoul = AnswerGlobal.Answer.SoulDictionary[AnswerGlobal.Answer.MainSoulUniqueID];
            foreach (SerializableContainer container in containerList)
            {
                Container targetContainer = new Container(container);
                mainSoul.ContainerDictionary.Add(container.UniqueID, targetContainer);
            }
            mainSoul.MainContainerUniqueID = containerList[0].UniqueID;
            AnswerGlobal.MainContainer = mainSoul.ContainerDictionary[containerList[0].UniqueID];
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
            targetContainer.GameObject.GetComponent<CharacterName>().Name = targetContainer.Name;
        }
    }
    private void GetSceneDataEventAction(bool getSceneDataStatus, string debugMessage, SerializableScene scene, List<SerializableContainer> containers)
    {
        if (getSceneDataStatus)
        {
            AnswerGlobal.Scene = new Scene(scene);
            foreach(SerializableContainer container in containers)
            {
                Container targetContainer = new Container(container);
                targetContainer.GameObject = Instantiate(AnswerGlobal.containerPrefab, new Vector3(targetContainer.PositionX, targetContainer.PositionY, targetContainer.PositionZ), Quaternion.identity) as GameObject;
                targetContainer.GameObject.GetComponent<CharacterName>().Name = targetContainer.Name;
                AnswerGlobal.Scene.ContainerDictionary.Add(container.UniqueID, targetContainer);
                if(AnswerGlobal.MainContainer.UniqueID == container.UniqueID)
                {
                    Camera.main.GetComponent<CameraFollow>().target = targetContainer.GameObject.transform;
                    AnswerGlobal.MainContainer = targetContainer;
                    targetContainer.GameObject.tag = "Self";
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
    private void UpdateContainerPositionEventAction(int sceneUniqueID, int containerUniqueID, float positionX, float positionY, float positionZ, float eulerAngleY)
    {
        Scene scene = AnswerGlobal.Scene;
        if (scene != null && scene.UniqueID == sceneUniqueID && scene.ContainerDictionary.ContainsKey(containerUniqueID))
        {
            Container container = scene.ContainerDictionary[containerUniqueID];
            container.PositionX = positionX;
            container.PositionY = positionY;
            container.PositionZ = positionZ;
            container.SyncVector = (new Vector3(positionX, positionY, positionZ))- container.GameObject.transform.position;
        }
    }
    private void MoveTargetPositionEventAction(int sceneUniqueID, int containerUniqueID, float positionX, float positionY, float positionZ)
    {
        Scene scene = AnswerGlobal.Scene;
        if (scene.UniqueID == sceneUniqueID && scene.ContainerDictionary.ContainsKey(containerUniqueID))
        {
            Container container = scene.ContainerDictionary[containerUniqueID];
            container.TargetPostion = new Vector3(positionX, positionY, positionZ);
            container.Moving = true;
            container.GameObject.GetComponent<ContainerMoveController>().targetPosition = container.TargetPostion;
            container.GameObject.GetComponent<ContainerMoveController>().moving = container.Moving;
        }
    }
    private void SendMessageEventAction(int containerUniqueID, string containerName, MessageLevel level, string message)
    {
        MessagePanelController messageController = GameObject.FindGameObjectWithTag("MessagePanel").GetComponent<MessagePanelController>();
        if(messageController != null)
        {
            switch(level)
            {
                case MessageLevel.Scene:
                    {
                        if(AnswerGlobal.Scene.ContainerDictionary.ContainsKey(containerUniqueID))
                        {
                            messageController.AppendMessage("[場景] "+containerName+": "+message);
                            messageController.UpdateMessageBox();
                        }
                    }
                    break;
            }
        }
    }
}
