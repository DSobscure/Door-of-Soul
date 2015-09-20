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
    public class DSPeer : PeerBase
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
            if(server.WandererDictionary.ContainsKey(guid))
            {
                server.WandererDictionary.Remove(guid);
            }
            else
            {
                if (Answer != null &&server.AnswerDictionary.ContainsKey(Answer.UniqueID))
                {
                    server.AnswerDictionary.Remove(Answer.UniqueID);
                    HashSet<int> soulUniqueIDList = new HashSet<int>();
                    HashSet<int> sceneUniqueIDList = new HashSet<int>();
                    HashSet<int> containerUniqueIDList = new HashSet<int>();
                    foreach (Soul soul in Answer.SoulDictionary.Values)
                    {
                        soulUniqueIDList.Add(soul.UniqueID);
                        foreach (Container container in soul.ContainerDictionary.Values)
                        {
                            sceneUniqueIDList.Add(container.Location.UniqueID);
                            containerUniqueIDList.Add(container.UniqueID);
                        }
                    }
                    Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)DisconnectBroadcastItem.SoulUniqueIDListDataString,SerializeFunction.SerializeObject(soulUniqueIDList.ToArray())},
                                            {(byte)DisconnectBroadcastItem.SceneUniqueIDListDataString,SerializeFunction.SerializeObject(sceneUniqueIDList.ToArray())},
                                            {(byte)DisconnectBroadcastItem.ContainerUniqueIDListDataString,SerializeFunction.SerializeObject(containerUniqueIDList.ToArray())}
                                        };
                    HashSet<DSPeer> peers = new HashSet<DSPeer>();
                    foreach (Answer answer in server.AnswerDictionary.Values)
                    {
                        peers.Add(answer.Peer);
                    }
                    foreach(Scene scene in server.SceneAdministratorDictionary.Values)
                    {
                        peers.Add(scene.AdministratorPeer);
                    }
                    server.Broadcast(peers.ToArray(), BroadcastType.Disconnect, parameter);

                    foreach (Soul soul in Answer.SoulDictionary.Values)
                    {
                        foreach (Container container in soul.ContainerDictionary.Values)
                        {
                            if (server.ContainerDictionary.ContainsKey(container.UniqueID))
                            {
                                server.ContainerDictionary.Remove(container.UniqueID);
                            }
                            if (container.Location.ContainerDictionary.ContainsKey(container.UniqueID))
                            {
                                container.Location.ContainerDictionary.Remove(container.UniqueID);
                            }
                        }
                    }
                }
                else
                {
                    var searchResult = server.SceneAdministratorDictionary.FirstOrDefault(pair => pair.Value.AdministratorPeer == this);
                    int administratorUniqueID = (searchResult.Value is Scene)?searchResult.Key:-1;
                    if(server.SceneAdministratorDictionary.ContainsKey(administratorUniqueID))
                    {
                        server.SceneAdministratorDictionary[administratorUniqueID].SceneAdministratorUniqueID = 0;
                        server.SceneAdministratorDictionary[administratorUniqueID].AdministratorPeer = null;
                        server.SceneAdministratorDictionary.Remove(administratorUniqueID);
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
                if(answerUniqueID != Answer.UniqueID)
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
                if (ActiveSoul_and_Broadcast(soulUniqueID,operationRequest.Parameters))
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
                    if (ProjectToScene(sceneUniqueID,containerUniqueID))
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
                string sceneDataString,containersDataString;
                if (GetSceneData(sceneUniqueID,out sceneDataString,out containersDataString))
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
                if (ControlTheScene(administratorUniqueID,sceneUniqueID,out sceneDataString,out containersDataString))
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

                if (MoveTargetPosition_and_Broadcast(sceneUniqueID,containerUniqueID,positionX,positionY,positionZ))
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

        //Action
        private bool OpenDS(int answerUniqueID)
        {
            string[]  requestItem = new string[3];
            requestItem[0] = "Name";
            requestItem[1] = "SoulLimit";
            requestItem[2] = "MainSoulUniqueID";

            TypeCode[] requestType = new TypeCode[3];
            requestType[0] = TypeCode.String;
            requestType[1] = TypeCode.Int32;
            requestType[2] = TypeCode.Int32;

            object[] returnData = server.database.GetDataByUniqueID(answerUniqueID, requestItem, requestType, "answer");

            Answer = new Answer(answerUniqueID, (string)returnData[0], (int)returnData[1], (int)returnData[2],this);

            if (server.WandererDictionary.ContainsKey(guid))
            {
                server.WandererDictionary.Remove(guid);
            }
            server.AnswerDictionary.Add(answerUniqueID,Answer);
            
            return true;
        }
        private string RegisterSoulData(int answerUniqueID)
        {
            SerializableSoul[] soulList = server.database.GetSoulList(answerUniqueID);
            foreach (SerializableSoul soul in soulList)
            {
                Answer.SoulDictionary.Add(soul.UniqueID,new Soul(soul, Answer));
            }
            return SerializeFunction.SerializeObject(soulList);
        }
        private string RegisterContainerData(int soulUniqueID)
        {
            SerializableContainer[] containerList = server.database.GetContainerList(soulUniqueID);
            foreach (SerializableContainer container in containerList)
            {
                Container soulContainer = new Container(container, server.SceneDictionary[container.LocationUniqueID]);
                soulContainer.SoulDictionary.Add(soulUniqueID,Answer.SoulDictionary[soulUniqueID]);
                Answer.SoulDictionary[soulUniqueID].ContainerDictionary.Add(container.UniqueID, soulContainer);
                server.ContainerDictionary.Add(container.UniqueID, soulContainer);
            }
            return SerializeFunction.SerializeObject(containerList);
        }
        private bool ActiveSoul_and_Broadcast(int soulUniqueID, Dictionary<byte,object> parameters)
        {
            if(Answer.SoulDictionary[soulUniqueID].Active)
            {
                return false;
            }
            else
            {
                Answer.SoulDictionary[soulUniqueID].Active = true;
                HashSet<DSPeer> peers = new HashSet<DSPeer>();
                foreach (Answer answer in server.AnswerDictionary.Values)
                {
                    peers.Add(answer.Peer);
                }
                server.Broadcast(peers.ToArray(), BroadcastType.ActiveSoul, parameters);
                return true;
            }
        }
        private bool ProjectToScene(int sceneUniqueID, int containerUniqueID)
        {
            if(server.SceneDictionary[sceneUniqueID].ContainerDictionary.ContainsKey(containerUniqueID))
            {
                return false;
            }
            else
            {
                Container container = Answer.SoulDictionary.First(pair => pair.Value.ContainerDictionary.ContainsKey(containerUniqueID)).Value.ContainerDictionary[containerUniqueID];
                return ProjectContainerToScene_and_Broadcast(container, server.SceneDictionary[sceneUniqueID]);
            }
        }
        private bool ProjectContainerToScene_and_Broadcast(Container container, Scene scene)
        {
            try
            {
                scene.ContainerDictionary.Add(container.UniqueID, container);
                Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)ProjectContainerBroadcastItem.SceneUniqueID,scene.UniqueID},
                                            {(byte)ProjectContainerBroadcastItem.ContainerDataString,SerializeFunction.SerializeObject(container.Serialize())}
                                        };
                HashSet<DSPeer> peers = new HashSet<DSPeer>();
                foreach (Container targetContainer in scene.ContainerDictionary.Values)
                {
                    foreach (Soul targetSoul in targetContainer.SoulDictionary.Values)
                    {
                        peers.Add(targetSoul.SourceAnswer.Peer);
                    }
                }
                if (scene.AdministratorPeer != null)
                    peers.Add(scene.AdministratorPeer);
                server.Broadcast(peers.ToArray(), BroadcastType.ProjectContainer, parameter);
                return true;
            }
            catch(Exception ex)
            {
                DSServer.Log.Error(ex.Message);
                return false;
            }
        }
        private bool GetSceneData(int sceneUniqueID, out string sceneDataString,out string containersDataString)
        {
            if (server.SceneDictionary.ContainsKey(sceneUniqueID))
            {
                Scene scene = server.SceneDictionary[sceneUniqueID];
                List<SerializableContainer> containerList = new List<SerializableContainer>();
                foreach (Container container in scene.ContainerDictionary.Values)
                {
                    containerList.Add(container.Serialize());
                }
                sceneDataString = SerializeFunction.SerializeObject(scene.Serialize());
                containersDataString = SerializeFunction.SerializeObject(containerList.ToArray());
                return true;
            }
            else
            {
                sceneDataString = "";
                containersDataString = "";
                return false;
            }
        }
        private bool ControlTheScene(int administratorUniqueID, int sceneUniqueID, out string sceneDataString, out string containersDataString)
        {
            if(server.SceneDictionary.ContainsKey(sceneUniqueID))
            {
                server.SceneDictionary[sceneUniqueID].SceneAdministratorUniqueID = administratorUniqueID;
                server.SceneDictionary[sceneUniqueID].AdministratorPeer = this;
                server.SceneAdministratorDictionary.Add(administratorUniqueID, server.SceneDictionary[sceneUniqueID]);

                List<SerializableContainer> containerList = new List<SerializableContainer>();
                foreach (Container container in server.SceneDictionary[sceneUniqueID].ContainerDictionary.Values)
                {
                    containerList.Add(container.Serialize());
                }
                sceneDataString = SerializeFunction.SerializeObject(server.SceneDictionary[sceneUniqueID].Serialize());
                containersDataString = SerializeFunction.SerializeObject(containerList.ToArray());
                return true;
            }
            else
            {
                sceneDataString = "";
                containersDataString = "";
                return false;
            }
        }
        private bool MoveTargetPosition_and_Broadcast(int sceneUniqueID, int containerUniqueID, float positionX, float positionY, float positionZ)
        {
            if (server.SceneDictionary.ContainsKey(sceneUniqueID) && server.SceneDictionary[sceneUniqueID].ContainerDictionary.ContainsKey(containerUniqueID))
            {
                Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)SendMoveTargetPositionBroadcastItem.SceneUniqueID,sceneUniqueID},
                                            {(byte)SendMoveTargetPositionBroadcastItem.ContainerUniqueID,containerUniqueID},
                                            {(byte)SendMoveTargetPositionBroadcastItem.PositionX,positionX},
                                            {(byte)SendMoveTargetPositionBroadcastItem.PositionY,positionY},
                                            {(byte)SendMoveTargetPositionBroadcastItem.PositionZ,positionZ}
                                        };
                Scene scene = server.SceneDictionary[sceneUniqueID];
                List<DSPeer> peers = new List<DSPeer>();
                foreach (Container targetContainer in scene.ContainerDictionary.Values)
                {
                    foreach (Soul targetSoul in targetContainer.SoulDictionary.Values)
                    {
                        peers.Add(targetSoul.SourceAnswer.Peer);
                    }
                }
                if(scene.AdministratorPeer != null)
                    peers.Add(scene.AdministratorPeer);
                server.Broadcast(peers.ToArray(), BroadcastType.SendMoveTargetPosition, parameter);
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool UpdateContainerPosition_and_Broadcast(int sceneUniqueID, int containerUniqueID, float positionX, float positionY, float positionZ, float eulerAngleY)
        {
            if(server.SceneDictionary.ContainsKey(sceneUniqueID) && server.SceneDictionary[sceneUniqueID].ContainerDictionary.ContainsKey(containerUniqueID))
            {
                Scene scene = server.SceneDictionary[sceneUniqueID];
                Container container = scene.ContainerDictionary[containerUniqueID];
                container.PositionX = positionX;
                container.PositionY = positionY;
                container.PositionZ = positionZ;
                container.EulerAngleY = eulerAngleY;
                Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)ContainerPositionUpdateBroadcastItem.SceneUniqueID,scene.UniqueID},
                                            {(byte)ContainerPositionUpdateBroadcastItem.ContainerUniqueID,container.UniqueID},
                                            {(byte)ContainerPositionUpdateBroadcastItem.PositionX,positionX},
                                            {(byte)ContainerPositionUpdateBroadcastItem.PositionY,positionY},
                                            {(byte)ContainerPositionUpdateBroadcastItem.PositionZ,positionZ},
                                            {(byte)ContainerPositionUpdateBroadcastItem.EulerAngleY,eulerAngleY}
                                        };
                HashSet<DSPeer> peers = new HashSet<DSPeer>();
                foreach (Container targetContainer in scene.ContainerDictionary.Values)
                {
                    foreach (Soul targetSoul in targetContainer.SoulDictionary.Values)
                    {
                        peers.Add(targetSoul.SourceAnswer.Peer);
                    }
                }
                server.Broadcast(peers.ToArray(), BroadcastType.ContainerPositionUpdate, parameter);
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool SendMessage_and_Broadcast(int containerUniqueID,MessageLevel level, string message)
        {
            if (server.ContainerDictionary.ContainsKey(containerUniqueID))
            {
                Container container = server.ContainerDictionary[containerUniqueID];
                Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)SendMessageBroadcastItem.ContainerUniqueID,containerUniqueID },
                                            {(byte)SendMessageBroadcastItem.ContainerName,container.Name},
                                            {(byte)SendMessageBroadcastItem.MessageLevel,level},
                                            {(byte)SendMessageBroadcastItem.Message,message}
                                        };
                List<DSPeer> peers = GetPeerListByMessageLevel(container, level);
                server.Broadcast(peers.ToArray(), BroadcastType.SendMessage, parameter);
                return true;
            }
            else
            {
                return false;
            }
        }

        //Tool Function
        private List<DSPeer> GetPeerListByMessageLevel(Container container, MessageLevel level)
        {
            List<DSPeer> peers = new List<DSPeer>();
            switch(level)
            {
                case MessageLevel.Scene:
                    {
                        foreach(Container targetContainer in container.Location.ContainerDictionary.Values)
                        {
                            foreach (Soul targetSoul in targetContainer.SoulDictionary.Values)
                            {
                                peers.Add(targetSoul.SourceAnswer.Peer);
                            }
                        }
                    }
                    break;
            }
            return peers;
        }
    }
}
