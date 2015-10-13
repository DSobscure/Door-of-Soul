using System;
using System.Collections.Generic;
using System.Linq;
using Photon.SocketServer;
using DSServerStructure;
using DSCommunicationProtocol;
using DSSerializable.CharacterStructure;
using DSSerializable.WorldLevelStructure;
using DSServerStructure.WorldLevelStructure;
using Newtonsoft.Json;

namespace DSServer
{
    public partial class DSPeer : PeerBase
    {
        private bool OpenDS(int answerUniqueID)
        {
            string[] requestItem = new string[2];
            requestItem[0] = "Name";
            requestItem[1] = "SoulLimit";

            TypeCode[] requestType = new TypeCode[2];
            requestType[0] = TypeCode.String;
            requestType[1] = TypeCode.Int32;

            object[] returnData = server.database.GetDataByUniqueID(answerUniqueID, requestItem, requestType, "answer");

            Answer = new Answer(answerUniqueID, (string)returnData[0], (int)returnData[1], this);

            if (server.WandererDictionary.ContainsKey(guid))
            {
                server.WandererDictionary.Remove(guid);
            }
            server.AnswerDictionary.Add(answerUniqueID, Answer);

            return true;
        }
        private List<SerializableSoul> RegisterSoulData(int answerUniqueID)
        {
            List<SerializableSoul> soulList = server.database.GetSoulList(answerUniqueID);
            foreach (SerializableSoul soul in soulList)
            {
                Answer.soulDictionary.Add(soul.UniqueID, new Soul(soul, Answer));
            }
            return soulList;
        }
        private List<SerializableContainer> RegisterContainerData(int soulUniqueID)
        {
            List<SerializableContainer> containerList = server.database.GetContainerList(soulUniqueID);
            foreach (SerializableContainer container in containerList)
            {
                Container soulContainer = new Container(container, server.SceneDictionary[container.LocationUniqueID]);
                soulContainer.soulDictionary.Add(soulUniqueID, Answer.soulDictionary[soulUniqueID]);
                Answer.soulDictionary[soulUniqueID].containerDictionary.Add(container.UniqueID, soulContainer);
                server.ContainerDictionary.Add(container.UniqueID, soulContainer);
            }
            return containerList;
        }
        private bool ActiveSoul_and_Broadcast(int soulUniqueID, Dictionary<byte, object> parameters)
        {
            if (Answer.soulDictionary[soulUniqueID].Active)
            {
                return false;
            }
            else
            {
                Answer.soulDictionary[soulUniqueID].Active = true;
                HashSet<DSPeer> peers = new HashSet<DSPeer>();
                foreach (Answer answer in server.AnswerDictionary.Values)
                {
                    peers.Add(answer.peer);
                }
                server.Broadcast(peers.ToArray(), BroadcastType.ActiveSoul, parameters);
                return true;
            }
        }
        private bool ProjectToScene(int sceneUniqueID, int containerUniqueID)
        {
            if (server.SceneDictionary[sceneUniqueID].containerDictionary.ContainsKey(containerUniqueID))
            {
                return false;
            }
            else
            {
                Container container = Answer.soulDictionary.First(pair => pair.Value.containerDictionary.ContainsKey(containerUniqueID)).Value.containerDictionary[containerUniqueID];
                return ProjectContainerToScene_and_Broadcast(container, server.SceneDictionary[sceneUniqueID]);
            }
        }
        private bool ProjectContainerToScene_and_Broadcast(Container container, Scene scene)
        {
            try
            {
                scene.containerDictionary.Add(container.uniqueID, container);
                Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)ProjectContainerBroadcastItem.SceneUniqueID,scene.uniqueID},
                                            {(byte)ProjectContainerBroadcastItem.ContainerDataString,JsonConvert.SerializeObject(container.Serialize(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })}
                                        };
                HashSet<DSPeer> peers = new HashSet<DSPeer>();
                foreach (Container targetContainer in scene.containerDictionary.Values)
                {
                    foreach (Soul targetSoul in targetContainer.soulDictionary.Values)
                    {
                        peers.Add(targetSoul.sourceAnswer.peer);
                    }
                }
                if (scene.administratorPeer != null)
                    peers.Add(scene.administratorPeer);
                server.Broadcast(peers.ToArray(), BroadcastType.ProjectContainer, parameter);
                return true;
            }
            catch (Exception ex)
            {
                DSServer.Log.Error(ex.Message);
                return false;
            }
        }
        private bool GetSceneData(int sceneUniqueID, out SerializableScene scene, out SerializableContainer[] containers)
        {
            if (server.SceneDictionary.ContainsKey(sceneUniqueID))
            {
                Scene targetScene = server.SceneDictionary[sceneUniqueID];
                List<SerializableContainer> containerList = new List<SerializableContainer>();
                foreach (Container container in targetScene.containerDictionary.Values)
                {
                    containerList.Add(container.Serialize());
                }
                scene = targetScene.Serialize();
                containers = containerList.ToArray();
                return true;
            }
            else
            {
                scene = null;
                containers = null;
                return false;
            }
        }
        private bool ControlTheScene(int administratorUniqueID, int sceneUniqueID, out  SerializableScene scene, out SerializableContainer[] containers)
        {
            if (server.SceneDictionary.ContainsKey(sceneUniqueID))
            {
                server.SceneDictionary[sceneUniqueID].SetAdministrator(administratorUniqueID, this);
                server.SceneAdministratorDictionary.Add(administratorUniqueID, server.SceneDictionary[sceneUniqueID]);

                List<SerializableContainer> containerList = new List<SerializableContainer>();
                foreach (Container container in server.SceneDictionary[sceneUniqueID].containerDictionary.Values)
                {
                    containerList.Add(container.Serialize());
                }
                scene = server.SceneDictionary[sceneUniqueID].Serialize();
                containers = containerList.ToArray();
                return true;
            }
            else
            {
                scene = null;
                containers = null;
                return false;
            }
        }
        private bool MoveTargetPosition_and_Broadcast(int sceneUniqueID, int containerUniqueID, float positionX, float positionY, float positionZ)
        {
            if (server.SceneDictionary.ContainsKey(sceneUniqueID) && server.SceneDictionary[sceneUniqueID].containerDictionary.ContainsKey(containerUniqueID))
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
                foreach (Container targetContainer in scene.containerDictionary.Values)
                {
                    foreach (Soul targetSoul in targetContainer.soulDictionary.Values)
                    {
                        peers.Add(targetSoul.sourceAnswer.peer);
                    }
                }
                if (scene.administratorPeer != null)
                    peers.Add(scene.administratorPeer);
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
            if (server.SceneDictionary.ContainsKey(sceneUniqueID) && server.SceneDictionary[sceneUniqueID].containerDictionary.ContainsKey(containerUniqueID))
            {
                Scene scene = server.SceneDictionary[sceneUniqueID];
                Container container = scene.containerDictionary[containerUniqueID];
                container.UpdatePosition(positionX,positionY,positionZ);
                container.UpdateEulerAngle(0, eulerAngleY,0);
                Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)ContainerPositionUpdateBroadcastItem.SceneUniqueID,scene.uniqueID},
                                            {(byte)ContainerPositionUpdateBroadcastItem.ContainerUniqueID,container.uniqueID},
                                            {(byte)ContainerPositionUpdateBroadcastItem.PositionX,positionX},
                                            {(byte)ContainerPositionUpdateBroadcastItem.PositionY,positionY},
                                            {(byte)ContainerPositionUpdateBroadcastItem.PositionZ,positionZ},
                                            {(byte)ContainerPositionUpdateBroadcastItem.EulerAngleY,eulerAngleY}
                                        };
                HashSet<DSPeer> peers = new HashSet<DSPeer>();
                foreach (Container targetContainer in scene.containerDictionary.Values)
                {
                    foreach (Soul targetSoul in targetContainer.soulDictionary.Values)
                    {
                        peers.Add(targetSoul.sourceAnswer.peer);
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
                                            {(byte)SendMessageBroadcastItem.ContainerName,container.name},
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
            if (Answer != null && server.AnswerDictionary.ContainsKey(Answer.uniqueID))
            {
                server.AnswerDictionary.Remove(Answer.uniqueID);
                HashSet<int> soulUniqueIDList = new HashSet<int>();
                HashSet<int> sceneUniqueIDList = new HashSet<int>();
                HashSet<int> containerUniqueIDList = new HashSet<int>();
                foreach (Soul soul in Answer.soulDictionary.Values)
                {
                    soulUniqueIDList.Add(soul.uniqueID);
                    foreach (Container container in soul.containerDictionary.Values)
                    {
                        sceneUniqueIDList.Add(container.location.uniqueID);
                        containerUniqueIDList.Add(container.uniqueID);
                    }
                }
                Dictionary<byte, object> parameter = new Dictionary<byte, object>
                                        {
                                            {(byte)DisconnectBroadcastItem.SoulUniqueIDListDataString, JsonConvert.SerializeObject(soulUniqueIDList.ToArray(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })},
                                            {(byte)DisconnectBroadcastItem.SceneUniqueIDListDataString, JsonConvert.SerializeObject(sceneUniqueIDList.ToArray(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })},
                                            {(byte)DisconnectBroadcastItem.ContainerUniqueIDListDataString, JsonConvert.SerializeObject(containerUniqueIDList.ToArray(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })}
                                        };
                HashSet<DSPeer> peers = new HashSet<DSPeer>();
                foreach (Answer answer in server.AnswerDictionary.Values)
                {
                    peers.Add(answer.peer);
                }
                foreach (Scene scene in server.SceneAdministratorDictionary.Values)
                {
                    peers.Add(scene.administratorPeer);
                }
                server.Broadcast(peers.ToArray(), BroadcastType.Disconnect, parameter);

                foreach (Soul soul in Answer.soulDictionary.Values)
                {
                    foreach (Container container in soul.containerDictionary.Values)
                    {
                        if (server.ContainerDictionary.ContainsKey(container.uniqueID))
                        {
                            server.ContainerDictionary.Remove(container.uniqueID);
                        }
                        if (container.location.containerDictionary.ContainsKey(container.uniqueID))
                        {
                            container.location.containerDictionary.Remove(container.uniqueID);
                            int containerUniqueID = container.uniqueID;
                            string[] updateItems = { "PositionX", "PositionY", "PositionZ", "EulerAngleY" };
                            object[] updateValues = { container.positionX, container.positionY, container.positionZ, container.eulerAngleY };
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
            var searchResult = server.SceneAdministratorDictionary.FirstOrDefault(pair => pair.Value.administratorPeer == this);
            int administratorUniqueID = (searchResult.Value is Scene) ? searchResult.Key : -1;
            if (server.SceneAdministratorDictionary.ContainsKey(administratorUniqueID))
            {
                server.SceneAdministratorDictionary[administratorUniqueID].SetAdministrator(administratorUniqueID, null);
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
