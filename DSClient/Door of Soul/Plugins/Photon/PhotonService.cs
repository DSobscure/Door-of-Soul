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
                    ProjectContainerToSceneTask(eventData);
                }
                break;
            #endregion

            #region active soul
            case (byte)BroadcastType.ActiveSoul:
                {
                    ActiveSoulEventTask(eventData);
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

            #region container position update
            case (byte)BroadcastType.ContainerPositionUpdate:
                {
                    ContainerPositionUpdateEventTask(eventData);
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

            #region send message
            case (byte)BroadcastType.SendMessage:
                {
                    SendMessageEventTask(eventData);
                }
                break;
            #endregion
        }
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        switch (operationResponse.OperationCode)
        {
            #region open DS
            case (byte)OperationType.OpenDS:
                {
                    OpenDSTask(operationResponse);
                }
                break;
            #endregion

            #region get soul list
            case (byte)OperationType.GetSoulList:
                {
                    GetSoulListTask(operationResponse);
                }
                break;
            #endregion

            #region get container list
            case (byte)OperationType.GetContainerList:
                {
                    GetContainerListTask(operationResponse);
                }
                break;
            #endregion

            #region active soul
            case (byte)OperationType.ActiveSoul:
                {
                    ActiveSoulTask(operationResponse);
                }
                break;
            #endregion

            #region project to scene
            case (byte)OperationType.ProjectToScene:
                {
                    ProjectToSceneTask(operationResponse);
                }
                break;
            #endregion

            #region get scene data
            case (byte)OperationType.GetSceneData:
                {
                    GetSceneDataTask(operationResponse);
                }
                break;
            #endregion

            #region send move target position
            case (byte)OperationType.SendMoveTargetPosition:
                {
                    SendMoveTargetPositionTask(operationResponse);
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
    private void OpenDSTask(OperationResponse operationResponse)
    {
        if (operationResponse.ReturnCode == (short)ErrorType.Correct)
        {
            OpenDSEvent(true, "", JsonConvert.DeserializeObject<SerializableAnswer>((string)operationResponse.Parameters[(byte)OpenDSResponseItem.AnswerDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }));
        }
        else
        {
            DebugReturn(0, operationResponse.DebugMessage);
            OpenDSEvent(false, operationResponse.DebugMessage,null);
        }
    }
    private void GetSoulListTask(OperationResponse operationResponse)
    {
        if (operationResponse.ReturnCode == (short)ErrorType.Correct)
        {
            GetSoulListEvent(true, "", JsonConvert.DeserializeObject<List<SerializableSoul>>((string)operationResponse.Parameters[(byte)GetSoulListResponseItem.SoulListDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }));
        }
        else
        {
            DebugReturn(0, operationResponse.DebugMessage);
            GetSoulListEvent(false, operationResponse.DebugMessage, null);
        }
    }
    private void GetContainerListTask(OperationResponse operationResponse)
    {
        if (operationResponse.ReturnCode == (short)ErrorType.Correct)
        {
            GetContainerListEvent(true, "", JsonConvert.DeserializeObject<List<SerializableContainer>>((string)operationResponse.Parameters[(byte)GetContainerListResponseItem.ContainerListDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }));
        }
        else
        {
            DebugReturn(0, operationResponse.DebugMessage);
            GetContainerListEvent(false, operationResponse.DebugMessage, null);
        }
    }
    private void ActiveSoulTask(OperationResponse operationResponse)
    {
        if (operationResponse.ReturnCode != (short)ErrorType.Correct)
        {
            DebugReturn(0, operationResponse.DebugMessage);
        }
    }
    private void ProjectToSceneTask(OperationResponse operationResponse)
    {
        if (operationResponse.ReturnCode == (short)ErrorType.Correct)
        {
            ProjectToSceneEvent(true, "");
        }
        else
        {
            DebugReturn(0, operationResponse.DebugMessage);
            ProjectToSceneEvent(false, operationResponse.DebugMessage);
        }
    }
    private void GetSceneDataTask(OperationResponse operationResponse)
    {
        if (operationResponse.ReturnCode == (short)ErrorType.Correct)
        {
            GetSceneDataEvent
                (
                    getSceneDataStatus: true,
                    debugMessage: "",
                    scene: JsonConvert.DeserializeObject<SerializableScene>((string)operationResponse.Parameters[(byte)GetSceneDataResponseItem.SceneDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }),
                    containers: JsonConvert.DeserializeObject<List<SerializableContainer>>((string)operationResponse.Parameters[(byte)GetSceneDataResponseItem.ContainersDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })
                );
        }
        else
        {
            DebugReturn(0, operationResponse.DebugMessage);
            GetSceneDataEvent(false, operationResponse.DebugMessage, null,null);
        }
    }
    private void SendMoveTargetPositionTask(OperationResponse operationResponse)
    {
        if (operationResponse.ReturnCode != (short)ErrorType.Correct)
        {
            DebugReturn(0, operationResponse.DebugMessage);
        }
    }

    //Event Task
    private void ProjectContainerToSceneTask(EventData eventData)
    {
        if(eventData.Parameters.Count == 2)
        {
            int sceneUniqueID = (int)eventData.Parameters[(byte)ProjectContainerBroadcastItem.SceneUniqueID];
            SerializableContainer container = JsonConvert.DeserializeObject<SerializableContainer>((string)eventData.Parameters[(byte)ProjectContainerBroadcastItem.ContainerDataString], new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            if (container.UniqueID != AnswerGlobal.MainContainer.UniqueID)
                ProjectContainerToSceneEvent(sceneUniqueID, container);
        }
        else
        {
            Debug.Log("ProjectContainerToSceneTask parameter error");
        }
    }
    private void ActiveSoulEventTask(EventData eventData)
    {
        if(eventData.Parameters.Count == 1)
        {
            //int soulUniqueID = (int)eventData.Parameters[(byte)ActiveSoulBroadcastItem.SoulUniqueID];
            //ActiveSoulEvent(soulUniqueID);
        }
        else
        {
            Debug.Log("ActiveSoulEventTask parameter error");
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
            Debug.Log("DisconnectEventTask parameter error");
        }
    }
    private void ContainerPositionUpdateEventTask(EventData eventData)
    {
        if(eventData.Parameters.Count == 6)
        {
            int sceneUniqueID = (int)eventData.Parameters[(byte)ContainerPositionUpdateBroadcastItem.SceneUniqueID];
            int containerUniqueID = (int)eventData.Parameters[(byte)ContainerPositionUpdateBroadcastItem.ContainerUniqueID];
            float positionX = (float)eventData.Parameters[(byte)ContainerPositionUpdateBroadcastItem.PositionX];
            float positionY = (float)eventData.Parameters[(byte)ContainerPositionUpdateBroadcastItem.PositionY];
            float positionZ = (float)eventData.Parameters[(byte)ContainerPositionUpdateBroadcastItem.PositionZ];
            float eulerAngleY = (float)eventData.Parameters[(byte)ContainerPositionUpdateBroadcastItem.EulerAngleY];
            UpdateContainerPositionEvent(sceneUniqueID, containerUniqueID, positionX, positionY, positionZ, eulerAngleY);
        }
        else
        {
            Debug.Log("ContainerPositionUpdateEventTask parameter error");
        }
    }
    private void SendMoveTargetPositionEventTask(EventData eventData)
    {
        if(eventData.Parameters.Count == 5)
        {
            int sceneUniqueID = (int)eventData.Parameters[(byte)SendMoveTargetPositionBroadcastItem.SceneUniqueID];
            int containerUniqueID = (int)eventData.Parameters[(byte)SendMoveTargetPositionBroadcastItem.ContainerUniqueID];
            float positionX = (float)eventData.Parameters[(byte)SendMoveTargetPositionBroadcastItem.PositionX];
            float positionY = (float)eventData.Parameters[(byte)SendMoveTargetPositionBroadcastItem.PositionY];
            float positionZ = (float)eventData.Parameters[(byte)SendMoveTargetPositionBroadcastItem.PositionZ];
            MoveTargetPositionEvent(sceneUniqueID, containerUniqueID, positionX, positionY, positionZ);
        }
        else
        {
            Debug.Log("SendMoveTargetPositionEventTask parameter error");
        }
    }
    private void SendMessageEventTask(EventData eventData)
    {
        if (eventData.Parameters.Count == 4)
        {
            int containerUniqueID = (int)eventData.Parameters[(byte)SendMessageBroadcastItem.ContainerUniqueID];
            string containerName = (string)eventData.Parameters[(byte)SendMessageBroadcastItem.ContainerName];
            MessageLevel level = (MessageLevel)eventData.Parameters[(byte)SendMessageBroadcastItem.MessageLevel];
            string message = (string)eventData.Parameters[(byte)SendMessageBroadcastItem.Message];

            SendMessageEvent(containerUniqueID, containerName, level, message);
        }
        else
        {
            Debug.Log("SendMessageEventTask parameter error");
        }
    }

    //內部函數區塊   主動行為
    public void OpenDS(string account, string password)
    {
        try
        {
            var parameter = new Dictionary<byte, object> { 
                             { (byte)OpenDSParameterItem.Account, account },   
                             { (byte)OpenDSParameterItem.Password, password }
                        };

            this.peer.OpCustom((byte)OperationType.OpenDS, parameter, true, 0, true);
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }
    public void GetSoulList(Answer answer)
    {
        try
        {
            var parameter = new Dictionary<byte, object> { 
                             { (byte)GetSoulListParameterItem.AnswerUniqueID, answer.UniqueID },   
                        };

            this.peer.OpCustom((byte)OperationType.GetSoulList, parameter, true, 0, true);
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }
    public void GetContainerList(Soul soul)
    {
        try
        {
            var parameter = new Dictionary<byte, object> { 
                             { (byte)GetContainerListParameterItem.SoulUniqueID, soul.UniqueID },   
                        };

            this.peer.OpCustom((byte)OperationType.GetContainerList, parameter, true, 0, true);
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }
    public void ActiveSoul(Soul soul)
    {
        try
        {
            var parameter = new Dictionary<byte, object> { 
                             {(byte)ActiveSoulParameterItem.SoulUniqueID,soul.UniqueID},
                        };

            this.peer.OpCustom((byte)OperationType.ActiveSoul, parameter, true, 0, true);
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }
    public void ProjectToScene(Container container, int sceneUniqueID)
    {
        try
        {
            var parameter = new Dictionary<byte, object> { 
                             { (byte)ProjectToSceneParameterItem.ContainerUniqueID, container.UniqueID },
                             { (byte)ProjectToSceneParameterItem.SceneUniqueID, sceneUniqueID}
                        };

            this.peer.OpCustom((byte)OperationType.ProjectToScene, parameter, true, 0, true);
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }
    public void GetSceneData(int sceneUniqueID)
    {
        try
        {
            var parameter = new Dictionary<byte, object> { 
                             { (byte)GetSceneDataParameterItem.SceneUniqueID,sceneUniqueID },   
                        };

            this.peer.OpCustom((byte)OperationType.GetSceneData, parameter, true, 0, true);
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }
    public void SendMoveTargetPosition(int sceneUniqueID,int containerUniqueID,Vector3 position)
    {
        try
        {
            var parameter = new Dictionary<byte, object> { 
                             {(byte)SendMoveTargetPositionParameterItem.SceneUniqueID,sceneUniqueID},
                             {(byte)SendMoveTargetPositionParameterItem.ContainerUniqueID,containerUniqueID},
                             {(byte)SendMoveTargetPositionParameterItem.PositionX,position.x},
                             {(byte)SendMoveTargetPositionParameterItem.PositionY,position.y},
                             {(byte)SendMoveTargetPositionParameterItem.PositionZ,position.z}
                        };

            this.peer.OpCustom((byte)OperationType.SendMoveTargetPosition, parameter, true, 0, true);
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }
    public void SendMessage(int containerUniqueID, MessageLevel level,string message)
    {
        try
        {
            var parameter = new Dictionary<byte, object> {
                             {(byte)SendMessageParameterItem.ContainerUniqueID,containerUniqueID},
                             {(byte)SendMessageParameterItem.MessageLevel,level},
                             {(byte)SendMessageParameterItem.Message,message},
                        };

            this.peer.OpCustom((byte)OperationType.SendMessage, parameter, true, 0, true);
        }
        catch (Exception EX)
        {
            throw EX;
        }
    }
}
