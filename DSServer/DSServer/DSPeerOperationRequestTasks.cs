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
                                            {(byte)OpenDSResponseItem.AnswerDataString,SerializeFunction.SerializeObject(Answer.Serialize())}
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
                if (answerUniqueID != Answer.UniqueID)
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
                    string soulListDataString = RegisterSoulData(answerUniqueID);
                    Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)GetSoulListResponseItem.SoulListDataString,soulListDataString}
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
        private void GetContainerListTask(OperationRequest operationRequest)
        {
            if (operationRequest.Parameters.Count != 1)
            {
                OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                {
                    ReturnCode = (short)ErrorType.InvalidParameter,
                    DebugMessage = "GetContainerList Parameter Error"
                };
                this.SendOperationResponse(response, new SendParameters());
            }
            else
            {
                int soulUniqueID = (int)operationRequest.Parameters[(byte)GetContainerListParameterItem.SoulUniqueID];
                if (!Answer.SoulDictionary.ContainsKey(soulUniqueID))
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (short)ErrorType.PermissionDeny,
                        DebugMessage = "The soul is not yours"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
                else
                {
                    string containerListDataString = RegisterContainerData(soulUniqueID);
                    Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)GetContainerListResponseItem.ContainerListDataString,containerListDataString}
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
            if (operationRequest.Parameters.Count != 1)
            {
                OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                {
                    ReturnCode = (short)ErrorType.InvalidParameter,
                    DebugMessage = "ActiveSoul Parameter Error"
                };
                this.SendOperationResponse(response, new SendParameters());
            }
            else
            {
                int soulUniqueID = (int)operationRequest.Parameters[(byte)ActiveSoulParameterItem.SoulUniqueID];
                if (ActiveSoul_and_Broadcast(soulUniqueID, operationRequest.Parameters))
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
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
                        DebugMessage = "靈魂已顯現"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
            }
        }
        private void ProjectToSceneTask(OperationRequest operationRequest)
        {
            if (operationRequest.Parameters.Count != 2)
            {
                OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                {
                    ReturnCode = (short)ErrorType.InvalidParameter,
                    DebugMessage = "ProjectToScene Parameter Error"
                };
                this.SendOperationResponse(response, new SendParameters());
            }
            else
            {
                int containerUniqueID = (int)operationRequest.Parameters[(byte)ProjectToSceneParameterItem.ContainerUniqueID];
                int sceneUniqueID = (int)operationRequest.Parameters[(byte)ProjectToSceneParameterItem.SceneUniqueID];

                if (!server.SceneDictionary.ContainsKey(sceneUniqueID))
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (short)ErrorType.NotExist,
                        DebugMessage = "Scene Not Exist"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
                else
                {
                    if (ProjectToScene(sceneUniqueID, containerUniqueID))
                    {
                        OperationResponse response = new OperationResponse(operationRequest.OperationCode)
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
                            DebugMessage = "場景已存在該容器"
                        };
                        SendOperationResponse(response, new SendParameters());
                    }
                }
            }
        }
        private void GetSceneDataTask(OperationRequest operationRequest)
        {
            if (operationRequest.Parameters.Count != 1)
            {
                OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                {
                    ReturnCode = (short)ErrorType.InvalidParameter,
                    DebugMessage = "GetSceneData Parameter Error"
                };
                this.SendOperationResponse(response, new SendParameters());
            }
            else
            {
                int sceneUniqueID = (int)operationRequest.Parameters[(byte)GetSceneDataParameterItem.SceneUniqueID];
                string sceneDataString, containersDataString;
                if (GetSceneData(sceneUniqueID, out sceneDataString, out containersDataString))
                {
                    Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)GetSceneDataResponseItem.SceneDataString,sceneDataString},
                                            {(byte)GetSceneDataResponseItem.ContainersDataString,containersDataString}
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
                        ReturnCode = (short)ErrorType.NotExist,
                        DebugMessage = "Scene not exist"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
            }
        }
        private void ControlTheSceneTask(OperationRequest operationRequest)
        {
            if (operationRequest.Parameters.Count != 2)
            {
                OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                {
                    ReturnCode = (short)ErrorType.InvalidParameter,
                    DebugMessage = "ControlTheScene Parameter Error"
                };
                this.SendOperationResponse(response, new SendParameters());
            }
            else
            {
                int administratorUniqueID = (int)operationRequest.Parameters[(byte)ControlTheSceneParameterItem.AdministratorUniqueID];
                int sceneUniqueID = (int)operationRequest.Parameters[(byte)ControlTheSceneParameterItem.SceneUniqueID];
                string sceneDataString, containersDataString;
                if (ControlTheScene(administratorUniqueID, sceneUniqueID, out sceneDataString, out containersDataString))
                {
                    Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)ControlTheSceneResponseItem.SceneDataString,sceneDataString},
                                            {(byte)ControlTheSceneResponseItem.ContainersDataString,containersDataString}
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
                        ReturnCode = (short)ErrorType.NotExist,
                        DebugMessage = "Scene not exist"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
            }
        }
        private void SendMoveTargetPositionTask(OperationRequest operationRequest)
        {
            if (operationRequest.Parameters.Count != 5)
            {
                OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                {
                    ReturnCode = (short)ErrorType.InvalidParameter,
                    DebugMessage = "SendMoveTargetPosition Parameter Error"
                };
                this.SendOperationResponse(response, new SendParameters());
            }
            else
            {
                int sceneUniqueID = (int)operationRequest.Parameters[(byte)SendMoveTargetPositionParameterItem.SceneUniqueID];
                int containerUniqueID = (int)operationRequest.Parameters[(byte)SendMoveTargetPositionParameterItem.ContainerUniqueID];
                float positionX = (float)operationRequest.Parameters[(byte)SendMoveTargetPositionParameterItem.PositionX];
                float positionY = (float)operationRequest.Parameters[(byte)SendMoveTargetPositionParameterItem.PositionY];
                float positionZ = (float)operationRequest.Parameters[(byte)SendMoveTargetPositionParameterItem.PositionZ];

                if (MoveTargetPosition_and_Broadcast(sceneUniqueID, containerUniqueID, positionX, positionY, positionZ))
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
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
                        ReturnCode = (short)ErrorType.NotExist,
                        DebugMessage = "Scene or Container not exist"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
            }
        }
        private void ContainerPositionUpdateTask(OperationRequest operationRequest)
        {
            if (operationRequest.Parameters.Count != 6)
            {
                OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                {
                    ReturnCode = (short)ErrorType.InvalidParameter,
                    DebugMessage = "ContainerPositionUpdate Parameter Error"
                };
                this.SendOperationResponse(response, new SendParameters());
            }
            else
            {
                int sceneUniqueID = (int)operationRequest.Parameters[(byte)ContainerPositionUpdateParameterItem.SceneUniqueID];
                int containerUniqueID = (int)operationRequest.Parameters[(byte)ContainerPositionUpdateParameterItem.ContainerUniqueID];
                float positionX = (float)operationRequest.Parameters[(byte)ContainerPositionUpdateParameterItem.PositionX];
                float positionY = (float)operationRequest.Parameters[(byte)ContainerPositionUpdateParameterItem.PositionY];
                float positionZ = (float)operationRequest.Parameters[(byte)ContainerPositionUpdateParameterItem.PositionZ];
                float eulerAngleY = (float)operationRequest.Parameters[(byte)ContainerPositionUpdateParameterItem.EulerAngleY];

                if (UpdateContainerPosition_and_Broadcast(sceneUniqueID, containerUniqueID, positionX, positionY, positionZ, eulerAngleY))
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
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
                        ReturnCode = (short)ErrorType.NotExist,
                        DebugMessage = "Scene or Container not exist"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
            }
        }
        private void SendMessageTask(OperationRequest operationRequest)
        {
            if (operationRequest.Parameters.Count != 3)
            {
                OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                {
                    ReturnCode = (short)ErrorType.InvalidParameter,
                    DebugMessage = "SendMessageTask Parameter Error"
                };
                this.SendOperationResponse(response, new SendParameters());
            }
            else
            {
                int containerUniqueID = (int)operationRequest.Parameters[(byte)SendMessageParameterItem.ContainerUniqueID];
                MessageLevel level = (MessageLevel)operationRequest.Parameters[(byte)SendMessageParameterItem.MessageLevel];
                string message = (string)operationRequest.Parameters[(byte)SendMessageParameterItem.Message];

                if (SendMessage_and_Broadcast(containerUniqueID, level, message))
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
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
                        ReturnCode = (short)ErrorType.NotExist,
                        DebugMessage = "Send target not exist"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
            }
        }
    }
}
