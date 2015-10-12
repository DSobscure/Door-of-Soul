using System;
using System.Collections.Generic;
using System.Linq;
using Photon.SocketServer;
using DSDataStructure;
using DSProtocol;
using DSSerializable.CharacterStructure;
using DSSerializable.WorldLevelStructure;
using DSDataStructure.WorldLevelStructure;
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
                Answer.SoulDictionary.Add(soul.UniqueID, new Soul(soul, Answer));
            }
            return soulList;
        }
        private List<SerializableContainer> RegisterContainerData(int soulUniqueID)
        {
            List<SerializableContainer> containerList = server.database.GetContainerList(soulUniqueID);
            foreach (SerializableContainer container in containerList)
            {
                Container soulContainer = new Container(container, server.SceneDictionary[container.LocationUniqueID]);
                soulContainer.SoulDictionary.Add(soulUniqueID, Answer.SoulDictionary[soulUniqueID]);
                Answer.SoulDictionary[soulUniqueID].ContainerDictionary.Add(container.UniqueID, soulContainer);
                server.ContainerDictionary.Add(container.UniqueID, soulContainer);
            }
            return containerList;
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
                                            {(byte)ProjectContainerBroadcastItem.ContainerDataString,JsonConvert.SerializeObject(container.Serialize(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })}
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
        private bool GetSceneData(int sceneUniqueID, out SerializableScene scene, out SerializableContainer[] containers)
        {
            if (server.SceneDictionary.ContainsKey(sceneUniqueID))
            {
                Scene targetScene = server.SceneDictionary[sceneUniqueID];
                List<SerializableContainer> containerList = new List<SerializableContainer>();
                foreach (Container container in targetScene.ContainerDictionary.Values)
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
                server.SceneDictionary[sceneUniqueID].SceneAdministratorUniqueID = administratorUniqueID;
                server.SceneDictionary[sceneUniqueID].AdministratorPeer = this;
                server.SceneAdministratorDictionary.Add(administratorUniqueID, server.SceneDictionary[sceneUniqueID]);

                List<SerializableContainer> containerList = new List<SerializableContainer>();
                foreach (Container container in server.SceneDictionary[sceneUniqueID].ContainerDictionary.Values)
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
                                            {(byte)DisconnectBroadcastItem.SoulUniqueIDListDataString, JsonConvert.SerializeObject(soulUniqueIDList.ToArray(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })},
                                            {(byte)DisconnectBroadcastItem.SceneUniqueIDListDataString, JsonConvert.SerializeObject(sceneUniqueIDList.ToArray(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })},
                                            {(byte)DisconnectBroadcastItem.ContainerUniqueIDListDataString, JsonConvert.SerializeObject(containerUniqueIDList.ToArray(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })}
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
