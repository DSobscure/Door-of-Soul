using System;
using System.Collections.Generic;
using System.Linq;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using ExitGames.Logging;
using DSDataStructure;
using DSProtocol;
using DSSerializable.CharacterStructure;
using DSSerializable;
using DSDataStructure.WorldLevelStructure;

namespace DSServer
{
    public partial class DSPeer : PeerBase
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public Guid guid { get; set; }
        private DSServer server;
        public Answer Answer { get; set; }

        public DSPeer(IRpcProtocol rpcprotocol,IPhotonPeer nativePeer,DSServer serverApplication) : base(rpcprotocol,nativePeer)
        {
            guid = Guid.NewGuid();
            server = serverApplication;
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            if(!DisconnectAsWanderer())
            {
                if (!DisconnectAsPlayer())
                {
                    if(!DisconnectAsSceneAdministrator())
                    {
                        DSServer.Log.Info("Disconnect Error because we don't know what is the target");
                    }
                }
            }
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            switch(operationRequest.OperationCode)
            {
                #region open DS
                case (byte)OperationType.OpenDS:
                    {
                        OpenDSTask(operationRequest);
                    }
                    break;
                #endregion

                #region get soul list
                case (byte)OperationType.GetSoulList:
                    {
                        GetSoulListTask(operationRequest);
                    }
                    break;
                #endregion

                #region get container list
                case (byte)OperationType.GetContainerList:
                    {
                        GetContainerListTask(operationRequest);
                    }
                    break;
                #endregion

                #region active soul
                case (byte)OperationType.ActiveSoul:
                    {
                        ActiveSoulTask(operationRequest);
                    }
                    break;
                #endregion

                #region project to scene
                case (byte)OperationType.ProjectToScene:
                    {
                        ProjectToSceneTask(operationRequest);
                    }
                    break;
                #endregion

                #region get scene data
                case (byte)OperationType.GetSceneData:
                    {
                        GetSceneDataTask(operationRequest);
                    }
                    break;
                #endregion              

                #region control the scene
                case (byte)OperationType.ControlTheScene:
                    {
                        ControlTheSceneTask(operationRequest);
                    }
                    break;
                #endregion

                #region send move target position
                case (byte)OperationType.SendMoveTargetPosition:
                    {
                        SendMoveTargetPositionTask(operationRequest);
                    }
                    break;
                #endregion

                #region container position update
                case (byte)OperationType.ContainerPositionUpdate:
                    {
                        ContainerPositionUpdateTask(operationRequest);
                    }
                    break;
                #endregion

                #region send message
                case (byte)OperationType.SendMessage:
                    {
                        SendMessageTask(operationRequest);
                    }
                    break;
                    #endregion
            }
        }
    }
}
