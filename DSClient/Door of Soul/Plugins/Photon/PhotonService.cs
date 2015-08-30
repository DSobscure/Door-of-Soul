using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System;
using DSProtocol;
using DSSerializable;
using DSSerializable.CharacterStructure;
using DSSerializable.WorldLevelStructure;

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
            OpenDSEvent(true, "", SerializeFunction.DeserializeObject<SerializableAnswer>((string)operationResponse.Parameters[(byte)OpenDSResponseItem.AnswerDataString]));
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
            GetSoulListEvent(true, "", SerializeFunction.DeserializeObject<SerializableSoul[]>((string)operationResponse.Parameters[(byte)GetSoulListResponseItem.SoulListDataString]));
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
            GetContainerListEvent(true, "", SerializeFunction.DeserializeObject<SerializableContainer[]>((string)operationResponse.Parameters[(byte)GetContainerListResponseItem.ContainerListDataString]));
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
                    scene: SerializeFunction.DeserializeObject<SerializableScene>((string)operationResponse.Parameters[(byte)GetSceneDataResponseItem.SceneDataString]),
                    containers: SerializeFunction.DeserializeObject<SerializableContainer[]>((string)operationResponse.Parameters[(byte)GetSceneDataResponseItem.ContainersDataString])
                );
        }
        else
        {
            DebugReturn(0, operationResponse.DebugMessage);
            GetSceneDataEvent(false, operationResponse.DebugMessage, null,null);
        }
    }

    //Event Task
    public void ProjectContainerToSceneTask(EventData eventData)
    {
        int sceneUniqueID = (int)eventData.Parameters[(byte)ProjectContainerBroadcastItem.SceneUniqueID];
        SerializableContainer container =  SerializeFunction.DeserializeObject<SerializableContainer>((string)eventData.Parameters[(byte)ProjectContainerBroadcastItem.ContainerDataString]);
        if(container.UniqueID != AnswerGlobal.MainContainer.UniqueID)
            ProjectContainerToSceneEvent(sceneUniqueID, container);
    }
    private void ActiveSoulEventTask(EventData eventData)
    {
        //not implement
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
}
