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
        private bool OpenDS(int answerUniqueID)
        {
            string[] requestItem = new string[3];
            requestItem[0] = "Name";
            requestItem[1] = "SoulLimit";
            requestItem[2] = "MainSoulUniqueID";

            TypeCode[] requestType = new TypeCode[3];
            requestType[0] = TypeCode.String;
            requestType[1] = TypeCode.Int32;
            requestType[2] = TypeCode.Int32;

            object[] returnData = server.database.GetDataByUniqueID(answerUniqueID, requestItem, requestType, "answer");

            Answer = new Answer(answerUniqueID, (string)returnData[0], (int)returnData[1], (int)returnData[2], this);

            if (server.WandererDictionary.ContainsKey(guid))
            {
                server.WandererDictionary.Remove(guid);
            }
            server.AnswerDictionary.Add(answerUniqueID, Answer);

            return true;
        }
        private string RegisterSoulData(int answerUniqueID)
        {
            SerializableSoul[] soulList = server.database.GetSoulList(answerUniqueID);
            foreach (SerializableSoul soul in soulList)
            {
                Answer.SoulDictionary.Add(soul.UniqueID, new Soul(soul, Answer));
            }
            return SerializeFunction.SerializeObject(soulList);
        }
        private string RegisterContainerData(int soulUniqueID)
        {
            SerializableContainer[] containerList = server.database.GetContainerList(soulUniqueID);
            foreach (SerializableContainer container in containerList)
            {
                Container soulContainer = new Container(container, server.SceneDictionary[container.LocationUniqueID]);
                soulContainer.SoulDictionary.Add(soulUniqueID, Answer.SoulDictionary[soulUniqueID]);
                Answer.SoulDictionary[soulUniqueID].ContainerDictionary.Add(container.UniqueID, soulContainer);
                server.ContainerDictionary.Add(container.UniqueID, soulContainer);
            }
            return SerializeFunction.SerializeObject(containerList);
        }
        private bool ActiveSoul_and_Broadcast(int soulUniqueID, Dictionary<byte, object> parameters)
        {
            if (Answer.SoulDictionary[soulUniqueID].Active)
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
            if (server.SceneDictionary[sceneUniqueID].ContainerDictionary.ContainsKey(containerUniqueID))
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
            catch (Exception ex)
            {
                DSServer.Log.Error(ex.Message);
                return false;
            }
        }
        private bool GetSceneData(int sceneUniqueID, out string sceneDataString, out string containersDataString)
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
            if (server.SceneDictionary.ContainsKey(sceneUniqueID))
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
                if (scene.AdministratorPeer != null)
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
            if (server.SceneDictionary.ContainsKey(sceneUniqueID) && server.SceneDictionary[sceneUniqueID].ContainerDictionary.ContainsKey(containerUniqueID))
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
        private bool SendMessage_and_Broadcast(int containerUniqueID, MessageLevel level, string message)
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

        private bool DisconnectAsWanderer()
        {
            if(server.WandererDictionary.ContainsKey(guid))
            {
                return server.WandererDictionary.Remove(guid);
            }
            else
            {
                return false;
            }
        }
        private bool DisconnectAsPlayer()
        {
            if (Answer != null && server.AnswerDictionary.ContainsKey(Answer.UniqueID))
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
                foreach (Scene scene in server.SceneAdministratorDictionary.Values)
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
                            int containerUniqueID = container.UniqueID;
                            string[] updateItems = { "PositionX", "PositionY", "PositionZ", "EulerAngleY" };
                            object[] updateValues = { container.PositionX, container.PositionY, container.PositionZ, container.EulerAngleY };
                            string table = "container";
                            server.database.UpdateDataByUniqueID(containerUniqueID,updateItems,updateValues,table);
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool DisconnectAsSceneAdministrator()
        {
            var searchResult = server.SceneAdministratorDictionary.FirstOrDefault(pair => pair.Value.AdministratorPeer == this);
            int administratorUniqueID = (searchResult.Value is Scene) ? searchResult.Key : -1;
            if (server.SceneAdministratorDictionary.ContainsKey(administratorUniqueID))
            {
                server.SceneAdministratorDictionary[administratorUniqueID].SceneAdministratorUniqueID = 0;
                server.SceneAdministratorDictionary[administratorUniqueID].AdministratorPeer = null;
                server.SceneAdministratorDictionary.Remove(administratorUniqueID);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
