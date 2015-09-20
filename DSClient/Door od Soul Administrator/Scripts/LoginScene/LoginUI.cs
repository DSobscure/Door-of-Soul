using UnityEngine;
using System.Collections;
using System;
using DSSerializable.CharacterStructure;
using DSSerializable.WorldLevelStructure;
using System.Linq;

public class LoginUI : MonoBehaviour
{
    private string administratorUniqueID = "";
    private string sceneUniqueID = "";
    private string controlTheSceneResult = "";

    IEnumerator Start()
    {
        PhotonGlobal.PS.ControlTheSceneEvent += ControlTheSceneEventAction;
        PhotonGlobal.PS.ProjectContainerToSceneEvent += ProjectContainerToSceneEventAction;
        PhotonGlobal.PS.DisconnectEvent += DisconnectEventAction;
        PhotonGlobal.PS.MoveContainerToTargetPositionEvent += MoveContainerToTargetPositionEventAction;
        yield return null;
    }

    // Update is called once per frame
    void OnGUI()
    {
        try
        {
            GUI.Label(new Rect(30, 10, 100, 20), "AK - ");

            if (PhotonGlobal.PS.ServerConnected)
            {
                GUI.Label(new Rect(130, 10, 100, 20), "Connecting . . .");

                if (!SceneGlobal.ControlTheSceneStatus)
                {
                    GUI.Label(new Rect(30, 40, 200, 20), "Please Select Scene");

                    GUI.Label(new Rect(30, 70, 80, 20), "Administrator UniqueID:");
                    administratorUniqueID = GUI.TextField(new Rect(110, 70, 100, 20), administratorUniqueID, 17);

                    GUI.Label(new Rect(30, 100, 80, 20), "Scene UniqueID:");
                    sceneUniqueID = GUI.TextField(new Rect(110, 100, 100, 20), sceneUniqueID, 17);

                    if (GUI.Button(new Rect(30, 130, 100, 24), "確定"))
                    {
                        SceneGlobal.AdministratorUniqueID = int.Parse(administratorUniqueID);
                        SceneGlobal.SceneUniqueID = int.Parse(sceneUniqueID);
                        PhotonGlobal.PS.ControlTheScene(SceneGlobal.AdministratorUniqueID, SceneGlobal.SceneUniqueID);
                        Application.LoadLevel("testScene");
                    }
                    GUI.Label(new Rect(30, 160, 600, 20), controlTheSceneResult);
                }
            }
            else
            {
                GUI.Label(new Rect(130, 10, 200, 20), "Disconnect");
            }
        }
        catch (Exception EX)
        {
            Debug.Log(EX.Message);
        }
    }

    private void ControlTheSceneEventAction(bool controlTheSceneStatus, string debugMessage, SerializableScene scene, SerializableContainer[] containers)
    {
        if (controlTheSceneStatus)
        {
            SceneGlobal.Scene = new Scene(scene);
            foreach(SerializableContainer container in containers)
            {
                Container targetContainer = new Container(container);
                targetContainer.GameObject = Instantiate(SceneGlobal.containerPrefab, new Vector3(targetContainer.PositionX, targetContainer.PositionY, targetContainer.PositionZ), Quaternion.identity) as GameObject;
                SceneGlobal.Scene.ContainerDictionary.Add(container.UniqueID, targetContainer);
            }
            SceneGlobal.ControlTheSceneStatus = true;
        }
        else
        {
            SceneGlobal.ControlTheSceneStatus = false;
            controlTheSceneResult = debugMessage;
        }
    }

    private void ProjectContainerToSceneEventAction(int sceneUniqueID, SerializableContainer container)
    {
        if (SceneGlobal.Scene != null && SceneGlobal.Scene.UniqueID == sceneUniqueID)
        {
            Container targetContainer = new Container(container);
            SceneGlobal.Scene.ContainerDictionary.Add(container.UniqueID, targetContainer);
            targetContainer.GameObject = Instantiate(SceneGlobal.containerPrefab, new Vector3(targetContainer.PositionX, targetContainer.PositionY, targetContainer.PositionZ), Quaternion.identity) as GameObject;
        }
    }

    private void DisconnectEventAction(int[] soulUniqueIDList, int[] sceneUniqueIDList, int[] containerUniqueIDList)
    {
        if (sceneUniqueIDList.Contains(SceneGlobal.Scene.UniqueID))
        {
            foreach (int containerUniqueID in containerUniqueIDList)
            {
                if (SceneGlobal.Scene.ContainerDictionary.ContainsKey(containerUniqueID))
                {
                    Destroy(SceneGlobal.Scene.ContainerDictionary[containerUniqueID].GameObject);
                    SceneGlobal.Scene.ContainerDictionary.Remove(containerUniqueID);
                }
            }
        }
    }

    private void MoveContainerToTargetPositionEventAction(int sceneUniqueID, int containerUniqueID, float positionX, float positionY, float positionZ)
    {
        Scene scene = SceneGlobal.Scene;
        if (scene.UniqueID == sceneUniqueID && scene.ContainerDictionary.ContainsKey(containerUniqueID))
        {
            scene.ContainerDictionary[containerUniqueID].TargetPostion = new Vector3(positionX, positionY, positionZ);
            scene.ContainerDictionary[containerUniqueID].Moving = true;
        }
    }
}
