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
                    SerializableSoul[] soulList = server.database.GetSoulList(answerUniqueID);
                    FillSoulData(soulList);
                    Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
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
                    SerializableContainer[] containerList = server.database.GetContainerList(soulUniqueID);
                    FillContainerData(soulUniqueID,containerList);
                    Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)GetContainerListResponseItem.ContainerListDataString,SerializeFunction.SerializeObject(containerList)}
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
                if (!Answer.SoulDictionary[soulUniqueID].Active)
                {
                    Answer.SoulDictionary[soulUniqueID].Active = true;
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (short)ErrorType.Correct,
                        DebugMessage = ""
                    };
                    SendOperationResponse(response, new SendParameters());

                    HashSet<DSPeer> peers = new HashSet<DSPeer>();
                    foreach (Answer answer in server.AnswerDictionary.Values)
                    {
                        peers.Add(answer.Peer);
                    }
                    server.Broadcast(peers.ToArray(), BroadcastType.ActiveSoul, operationRequest.Parameters);
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
                    if (server.SceneDictionary[sceneUniqueID].ContainerDictionary.ContainsKey(containerUniqueID))
                    {
                        OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                        {
                            ReturnCode = (short)ErrorType.InvalidOperation,
                            DebugMessage = "場景已存在該容器"
                        };
                        this.SendOperationResponse(response, new SendParameters());
                    }
                    else
                    {
                        OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                        {
                            ReturnCode = (short)ErrorType.Correct,
                            DebugMessage = ""
                        };

                        SendOperationResponse(response, new SendParameters());
                        Container container = Answer.SoulDictionary.First(pair => pair.Value.ContainerDictionary.ContainsKey(containerUniqueID)).Value.ContainerDictionary[containerUniqueID];
                        ProjectContainerToScene(container, server.SceneDictionary[sceneUniqueID]);
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
                if (!server.SceneDictionary.ContainsKey(sceneUniqueID))
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (short)ErrorType.NotExist,
                        DebugMessage = "Scene not exist"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
                else
                {
                    List<SerializableContainer> containerList = new List<SerializableContainer>();
                    foreach(Container container in server.SceneDictionary[sceneUniqueID].ContainerDictionary.Values)
                    {
                        containerList.Add(container.Serialize());
                    }
                    Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)GetSceneDataResponseItem.SceneDataString,SerializeFunction.SerializeObject(server.SceneDictionary[sceneUniqueID].Serialize())},
                                            {(byte)GetSceneDataResponseItem.ContainersDataString,SerializeFunction.SerializeObject(containerList.ToArray())}
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
                if (!server.SceneDictionary.ContainsKey(sceneUniqueID))
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (short)ErrorType.NotExist,
                        DebugMessage = "Scene not exist"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
                else
                {
                    server.SceneDictionary[sceneUniqueID].SceneAdministratorUniqueID = administratorUniqueID;
                    server.SceneDictionary[sceneUniqueID].AdministratorPeer = this;
                    server.SceneAdministratorDictionary.Add(administratorUniqueID, server.SceneDictionary[sceneUniqueID]);

                    List<SerializableContainer> containerList = new List<SerializableContainer>();
                    foreach (Container container in server.SceneDictionary[sceneUniqueID].ContainerDictionary.Values)
                    {
                        containerList.Add(container.Serialize());
                    }
                    Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)ControlTheSceneResponseItem.SceneDataString,SerializeFunction.SerializeObject(server.SceneDictionary[sceneUniqueID].Serialize())},
                                            {(byte)ControlTheSceneResponseItem.ContainersDataString,SerializeFunction.SerializeObject(containerList.ToArray())}
                                        };

                    OperationResponse response = new OperationResponse(operationRequest.OperationCode,parameter)
                    {
                        ReturnCode = (short)ErrorType.Correct,
                        DebugMessage = ""
                    };

                    SendOperationResponse(response, new SendParameters());
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
                if (!server.SceneDictionary.ContainsKey(sceneUniqueID) || !server.SceneDictionary[sceneUniqueID].ContainerDictionary.ContainsKey(containerUniqueID))
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (short)ErrorType.NotExist,
                        DebugMessage = "Scene or Container not exist"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
                else
                {
                    float positionX = (float)operationRequest.Parameters[(byte)SendMoveTargetPositionParameterItem.PositionX];
                    float positionY = (float)operationRequest.Parameters[(byte)SendMoveTargetPositionParameterItem.PositionY];
                    float positionZ = (float)operationRequest.Parameters[(byte)SendMoveTargetPositionParameterItem.PositionZ];

                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (short)ErrorType.Correct,
                        DebugMessage = ""
                    };

                    SendOperationResponse(response, new SendParameters());

                    Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)SendMoveTargetPositionBroadcastItem.SceneUniqueID,sceneUniqueID},
                                            {(byte)SendMoveTargetPositionBroadcastItem.ContainerUniqueID,sceneUniqueID},
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
                    peers.Add(scene.AdministratorPeer);
                    server.Broadcast(peers.ToArray(),BroadcastType.SendMoveTargetPosition,parameter);
                }
            }
        }
        private void ContainerPositionUpdateTask(OperationRequest operationRequest)
        {
            if (operationRequest.Parameters.Count != 5)
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
                if (!server.SceneDictionary.ContainsKey(sceneUniqueID) || !server.SceneDictionary[sceneUniqueID].ContainerDictionary.ContainsKey(containerUniqueID))
                {
                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (short)ErrorType.NotExist,
                        DebugMessage = "Scene or Container not exist"
                    };
                    this.SendOperationResponse(response, new SendParameters());
                }
                else
                {
                    float positionX = (float)operationRequest.Parameters[(byte)ContainerPositionUpdateParameterItem.PositionX];
                    float positionY = (float)operationRequest.Parameters[(byte)ContainerPositionUpdateParameterItem.PositionY];
                    float positionZ = (float)operationRequest.Parameters[(byte)ContainerPositionUpdateParameterItem.PositionZ];

                    OperationResponse response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = (short)ErrorType.Correct,
                        DebugMessage = ""
                    };

                    SendOperationResponse(response, new SendParameters());

                    UpdateContainerPosition(server.SceneDictionary[sceneUniqueID], server.ContainerDictionary[containerUniqueID], positionX, positionY, positionZ);
                }
            }
        }

        //Action
        public bool OpenDS(int answerUniqueID)
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
        public void FillSoulData(SerializableSoul[] soulList)
        {
            foreach(SerializableSoul soul in soulList)
            {
                Answer.SoulDictionary.Add(soul.UniqueID,new Soul(soul, Answer));
            }
        }
        public void FillContainerData(int soulUniqueID, SerializableContainer[] containerList)
        {
            foreach(SerializableContainer container in containerList)
            {
                Container soulContainer = new Container(container, server.SceneDictionary[container.LocationUniqueID]);
                soulContainer.SoulDictionary.Add(soulUniqueID,Answer.SoulDictionary[soulUniqueID]);
                Answer.SoulDictionary[soulUniqueID].ContainerDictionary.Add(container.UniqueID, soulContainer);
                server.ContainerDictionary.Add(container.UniqueID, soulContainer);
            }
        }
        public void ProjectContainerToScene(Container container, Scene scene)
        {
            scene.ContainerDictionary.Add(container.UniqueID, container);
            Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)ProjectContainerBroadcastItem.SceneUniqueID,scene.UniqueID},
                                            {(byte)ProjectContainerBroadcastItem.ContainerDataString,SerializeFunction.SerializeObject(container.Serialize())}
                                        };
            HashSet<DSPeer> peers = new HashSet<DSPeer>();
            foreach(Container targetContainer in scene.ContainerDictionary.Values)
            {
                foreach(Soul targetSoul in targetContainer.SoulDictionary.Values)
                {
                    peers.Add(targetSoul.SourceAnswer.Peer);
                }
            }
            peers.Add(scene.AdministratorPeer);
            server.Broadcast(peers.ToArray(), BroadcastType.ProjectContainer, parameter);
        }
        public void UpdateContainerPosition(Scene scene,Container container,float positionX,float positionY,float positionZ)
        {
            container.PositionX = positionX;
            container.PositionY = positionY;
            container.PositionZ = positionZ;
            Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)ContainerPositionUpdateParameterItem.SceneUniqueID,scene.UniqueID},
                                            {(byte)ContainerPositionUpdateParameterItem.ContainerUniqueID,container.UniqueID},
                                            {(byte)ContainerPositionUpdateParameterItem.PositionX,positionX},
                                            {(byte)ContainerPositionUpdateParameterItem.PositionY,positionY},
                                            {(byte)ContainerPositionUpdateParameterItem.PositionZ,positionZ}
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
        }
    }
}
