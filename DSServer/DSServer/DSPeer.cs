using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using ExitGames.Logging;
using DSDataStructure;
using DSProtocol;
using DSSerializable.CharacterStructure;
using System.IO;
using DSSerializable;

namespace DSServer
{
    public class DSPeer : PeerBase
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public Guid guid { get; set; }
        private DSServer server;
        public Answer answer { get; set; }

        public DSPeer(IRpcProtocol rpcprotocol,IPhotonPeer nativePeer,DSServer serverApplication) : base(rpcprotocol,nativePeer)
        {
            guid = Guid.NewGuid();
            server = serverApplication;
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            if(server.WandererDictionary.ContainsKey(guid))
            {
                server.WandererDictionary.Remove(guid);
            }
            if(server.AnswerDictionary.ContainsKey(answer.UniqueID))
            {
                server.AnswerDictionary.Remove(answer.UniqueID);
            }
            foreach(Soul soul in answer.SoulList)
            {
                foreach(Container container in soul.ContainerList)
                {
                    if(server.ContainerDictionary.ContainsKey(container.UniqueID))
                    {
                        server.ContainerDictionary.Remove(container.UniqueID);
                    }
                    if(container.Location.ContainerDictionary.ContainsKey(container.UniqueID))
                    {
                        container.Location.ContainerDictionary.Remove(container.UniqueID);
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

                #region active soul
                case (byte)OperationType.ActiveSoul:
                    {
                        ActiveSoulTask(operationRequest);
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

                #region project to scene
                case (byte)OperationType.ProjectToScene:
                    {
                        ProjectToSceneTask(operationRequest);
                    }
                    break;
                #endregion
            }
        }

        //Tasks
        private void OpenDSTask(OperationRequest operationRequest)
        {
            if (operationRequest.Parameters.Count != 2)
            {
                OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                {
                    ReturnCode = (short)ErrorType.InvalidParameter,
                    DebugMessage = "OpenDS Parameter Error"
                };
                this.SendOperationResponse(response, new SendParameters());
            }
            else
            {
                string account = (string)operationRequest.Parameters[(byte)OpenDSParameterItem.Account];
                string password = (string)operationRequest.Parameters[(byte)OpenDSParameterItem.Password];

                int answerUniqueID;
                if (server.database.LoginCheck(account, password, out answerUniqueID))
                {
                    if (!server.AnswerDictionary.ContainsKey(answerUniqueID))
                    {
                        if (OpenDS(answerUniqueID))
                        {
                            Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)OpenDSResponseItem.AnswerDataString,SerializeFunction.SerializeObject(answer.Serialize())}
                                        };

                            OperationResponse response = new OperationResponse(operationRequest.OperationCode, parameter)
                            {
                                ReturnCode = (short)ErrorType.Correct,
                                DebugMessage = ""
                            };

                            SendOperationResponse(response, new SendParameters());
                        }
                        else
                        {
                            OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                            {
                                ReturnCode = (short)ErrorType.InvalidOperation,
                                DebugMessage = "登入失敗!"
                            };
                            SendOperationResponse(response, new SendParameters());
                        }
                    }
                    else
                    {
                        OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                        {
                            ReturnCode = (short)ErrorType.InvalidOperation,
                            DebugMessage = "此帳號已經登入!"
                        };
                        SendOperationResponse(response, new SendParameters());
                    }
                }
                else
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (short)ErrorType.InvalidOperation,
                        DebugMessage = "帳號密碼錯誤!"
                    };
                    SendOperationResponse(response, new SendParameters());
                }
            }
        }
        private void GetSoulListTask(OperationRequest operationRequest)
        {
            if (operationRequest.Parameters.Count != 1)
            {
                OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                {
                    ReturnCode = (short)ErrorType.InvalidParameter,
                    DebugMessage = "GetSoulList Parameter Error"
                };
                this.SendOperationResponse(response, new SendParameters());
            }
            else
            {
                int answerUniqueID = (int)operationRequest.Parameters[(byte)GetSoulListParameterItem.AnswerUniqueID];
                if(answerUniqueID != answer.UniqueID)
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (short)ErrorType.PermissionDeny,
                        DebugMessage = "The anwer is not yours"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
                else
                {
                    SerializableSoul[] soulList = server.database.GetSoulList(answerUniqueID);
                    FillSoulData(soulList);
                    Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)GetSoulListResponseItem.SoulLimit,answer.SoulLimit},
                                            {(byte)GetSoulListResponseItem.SoulListDataString,SerializeFunction.SerializeObject(soulList)}
                                        };

                    OperationResponse response = new OperationResponse(operationRequest.OperationCode, parameter)
                    {
                        ReturnCode = (short)ErrorType.Correct,
                        DebugMessage = ""
                    };

                    SendOperationResponse(response, new SendParameters());
                }
            }
        }
        private void ActiveSoulTask(OperationRequest operationRequest)
        {

        }
        private void GetSceneDataTask(OperationRequest operationRequest)
        {

        }
        private void ProjectToSceneTask(OperationRequest operationRequest)
        {

        }

        //Action
        public bool OpenDS(int answerUniqueID)
        {
            string[]  requestItem = new string[2];
            requestItem[0] = "Name";
            requestItem[1] = "SoulLimit";

            TypeCode[] requestType = new TypeCode[2];
            requestType[0] = TypeCode.String;
            requestType[1] = TypeCode.Int32;

            object[] returnData = server.database.GetDataByUniqueID(answerUniqueID, requestItem, requestType, "answer");

            answer = new Answer(answerUniqueID, (string)returnData[0], (int)returnData[1],this);

            if (server.WandererDictionary.ContainsKey(guid))
            {
                server.WandererDictionary.Remove(guid);
            }
            server.AnswerDictionary.Add(answerUniqueID,answer);
            
            return true;
        }
        public void FillSoulData(SerializableSoul[] soulList)
        {
            foreach(SerializableSoul soul in soulList)
            {
                answer.SoulList.Add(new Soul(soul, answer));
            }
        }
    }
}
