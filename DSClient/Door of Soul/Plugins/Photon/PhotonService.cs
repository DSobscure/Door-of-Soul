using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System;
using DSProtocol;
using DSSerializable;
using DSSerializable.CharacterStructure;

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
        //switch (eventData.Code)
        //{
            
        //}
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        switch (operationResponse.OperationCode)
        {
            case (byte)OperationType.OpenDS:
                OpenDSTask(operationResponse);
                break;
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
            OpenDSEvent(true, "", SerializeFunction.DeserializeObject<SerializableAnswer>(Convert.ToString(operationResponse.Parameters[(byte)OpenDSResponseItem.AnswerDataString])));
        }
        else
        {
            DebugReturn(0, operationResponse.DebugMessage);
            OpenDSEvent(false, operationResponse.DebugMessage,null); // send error message to loginEvent
        }
    }

    //Event Task
    

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
}
