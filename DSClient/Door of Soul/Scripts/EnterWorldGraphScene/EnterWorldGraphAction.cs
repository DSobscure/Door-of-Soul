using UnityEngine;
using System.Collections;
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
        //PhotonGlobal.PS.ProjectContainerToSceneEvent += ProjectContainerToSceneEventAction;
        PhotonGlobal.PS.GetSceneDataEvent += GetSceneDataEventAction;
        PhotonGlobal.PS.ProjectToSceneEvent += ProjectToSceneEventAction;

        PhotonGlobal.PS.GetSoulList(AnswerGlobal.Answer);
        yield return null;
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

    private void ProjectContainerToSceneEventAction(bool projectContainerToSceneStatus, string debugMessage, int sceneUniqueID, SerializableContainer container)
    {
        if (projectContainerToSceneStatus)
        {
            if(AnswerGlobal.Scene != null && AnswerGlobal.Scene.UniqueID == sceneUniqueID)
            {
                Container targetContainer = new Container(container);
                AnswerGlobal.Scene.ContainerDictionary.Add(container.UniqueID, targetContainer);
                targetContainer.GameObject = Instantiate(AnswerGlobal.containerPrefab, new Vector3(targetContainer.PositionX, targetContainer.PositionY, targetContainer.PositionZ), Quaternion.identity) as GameObject;
            }
        }
        else
        {
            projectToSceneResult = debugMessage;
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
}
