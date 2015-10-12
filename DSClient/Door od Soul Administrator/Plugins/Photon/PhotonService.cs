using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System;
using DSProtocol;
using DSSerializable;
using DSSerializable.CharacterStructure;
using DSSerializable.WorldLevelStructure;
using Newtonsoft.Json;

public partial class PhotonService : IPhotonPeerListener
{
    public PhotonPeer peer { get; protected set; }
    public bool ServerConnected { get; protected set; }
    public string DebugMessage { get; protected set; }

    public PhotonService()
    {
        peer = null;
        ServerConnected = false;
        DebugMessage = "";
    }

    public void Connect(string ip, int port, string serverNmae)
    {
        try
        {
            string serverAddress = ip + ":" + port.ToString();
            this.peer = new PhotonPeer(this, ConnectionProtocol.Udp);
            if (!this.peer.Connect(serverAddress, serverNmae))
            {
                ConnectEvent(false);
            }
        }
        catch (Exception EX)
        {
            ConnectEvent(false);
            throw EX;
        }
    }

    public void Disconnect()
    {
        try
        {
            if (peer != null)
                this.peer.Disconnect();
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }

    public void Service()
    {
        try
        {
            if (this.peer != null)
                this.peer.Service();
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }


    public void DebugReturn(DebugLevel level, string message)
    {
        this.DebugMessage = message;
    }

    public void OnEvent(EventData eventData)
    {
        switch (eventData.Code)
        {
            #region project container to scene
            case (byte)BroadcastType.ProjectContainer:
                {
                    ProjectContainerToSceneEventTask(eventData);
                }
                break;
            #endregion

            #region disconnect
            case (byte)BroadcastType.Disconnect:
                {
                    DisconnectEventTask(eventData);
                }
                break;
            #endregion

            #region send move target position
            case (byte)BroadcastType.SendMoveTargetPosition:
                {
                    SendMoveTargetPositionEventTask(eventData);
                }
                break;
            #endregion
        }
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        switch (operationResponse.OperationCode)
        {
            #region control the scene
            case (byte)OperationType.ControlTheScene:
                {
                    ControlTheSceneTask(operationResponse);
                }
                break;
            #endregion

            #region container position update
            case (byte)OperationType.ContainerPositionUpdate:
                {
                    ContainerPositionUpdateTask(operationResponse);
                }
                break;
            #endregion
        }
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.Connect:
                this.peer.EstablishEncryption();
                break;
            case StatusCode.Disconnect:
                this.peer = null;
                this.ServerConnected = false;
                ConnectEvent(false);
                break;
            case StatusCode.EncryptionEstablished:
                this.ServerConnected = true;
                ConnectEvent(true);
                break;
        }
    }

    //OperationResponse Task
    private void ControlTheSceneTask(OperationResponse operationResponse)
    {
        if (operationResponse.ReturnCode == (short)ErrorType.Correct)
        {

            ControlTheSceneEvent
                (
                    controlTheSceneStatus: true,
                    debugMessage: "",
                    scene: JsonConvert.DeserializeObject<SerializableScene>((string)operationResponse.Parameters[(byte)GetSceneDataResponseItem.SceneDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }),
                    containers: JsonConvert.DeserializeObject<List<SerializableContainer>>((string)operationResponse.Parameters[(byte)GetSceneDataResponseItem.ContainersDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })
                );
        }
        else
        {
            DebugReturn(0, operationResponse.DebugMessage);
            ControlTheSceneEvent(false, operationResponse.DebugMessage,null,null);
        }
    }
    private void ContainerPositionUpdateTask(OperationResponse operationResponse)
    {
        if (operationResponse.ReturnCode != (short)ErrorType.Correct)
        {

            DebugReturn(0, operationResponse.DebugMessage);
        }
    }

    //Event Task
    private void ProjectContainerToSceneEventTask(EventData eventData)
    {
        if(eventData.Parameters.Count == 2)
        {
            int sceneUniqueID = (int)eventData.Parameters[(byte)ProjectContainerBroadcastItem.SceneUniqueID];
            SerializableContainer container = JsonConvert.DeserializeObject<SerializableContainer>((string)eventData.Parameters[(byte)ProjectContainerBroadcastItem.ContainerDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            if (sceneUniqueID == SceneGlobal.Scene.UniqueID)
                ProjectContainerToSceneEvent(sceneUniqueID, container);
        }
        else
        {
            DebugReturn(0, "ProjectContainerToSceneEventTask parameter error");
        }
    }

    private void DisconnectEventTask(EventData eventData)
    {
        if(eventData.Parameters.Count == 3)
        {
            int[] soulUniqueIDList = JsonConvert.DeserializeObject<int[]>((string)eventData.Parameters[(byte)DisconnectBroadcastItem.SoulUniqueIDListDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            int[] sceneUniqueIDList = JsonConvert.DeserializeObject<int[]>((string)eventData.Parameters[(byte)DisconnectBroadcastItem.SceneUniqueIDListDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            int[] containerUniqueIDList = JsonConvert.DeserializeObject<int[]>((string)eventData.Parameters[(byte)DisconnectBroadcastItem.ContainerUniqueIDListDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            DisconnectEvent(soulUniqueIDList, sceneUniqueIDList, containerUniqueIDList);
        }
        else
        {
            DebugReturn(0, "DisconnectEventTask parametr error");
        }
    }

    private void SendMoveTargetPositionEventTask(EventData eventData)
    {
        if (eventData.Parameters.Count == 5)
        {
            int sceneUniqueID = (int)eventData.Parameters[(byte)SendMoveTargetPositionBroadcastItem.SceneUniqueID];
            int containerUniqueID = (int)eventData.Parameters[(byte)SendMoveTargetPositionBroadcastItem.ContainerUniqueID];
            float positionX = (float)eventData.Parameters[(byte)SendMoveTargetPositionBroadcastItem.PositionX];
            float positionY = (float)eventData.Parameters[(byte)SendMoveTargetPositionBroadcastItem.PositionY];
            float positionZ = (float)eventData.Parameters[(byte)SendMoveTargetPositionBroadcastItem.PositionZ];
            MoveContainerToTargetPositionEvent(sceneUniqueID, containerUniqueID, positionX, positionY, positionZ);
        }
        else
        {
            DebugReturn(0, "SendMoveTargetPositionEventTask parametr error");
        }
    }

    //內部函數區塊   主動行為

    public void ControlTheScene(int administratorUniqueID, int sceneUniqueID)
    {
        try
        {
            var parameter = new Dictionary<byte, object> { 
                             { (byte)ControlTheSceneParameterItem.AdministratorUniqueID,administratorUniqueID },
                             { (byte)ControlTheSceneParameterItem.SceneUniqueID,sceneUniqueID }   
                        };

            this.peer.OpCustom((byte)OperationType.ControlTheScene, parameter, true, 0, true);
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }
    public void ContainerPositionUpdate(int sceneUniqueID,int containerUniqueID,float positionX,float positionY,float positionZ,float eulerAngleY)
    {
        try
        {
            var parameter = new Dictionary<byte, object> { 
                             {(byte)ContainerPositionUpdateParameterItem.SceneUniqueID,sceneUniqueID},
                             {(byte)ContainerPositionUpdateParameterItem.ContainerUniqueID,containerUniqueID},
                             {(byte)ContainerPositionUpdateParameterItem.PositionX,positionX},
                             {(byte)ContainerPositionUpdateParameterItem.PositionY,positionY},
                             {(byte)ContainerPositionUpdateParameterItem.PositionZ,positionZ},
                             {(byte)ContainerPositionUpdateParameterItem.EulerAngleY,eulerAngleY}
                        };

            this.peer.OpCustom((byte)OperationType.ContainerPositionUpdate, parameter, true, 0, true);
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }
}
