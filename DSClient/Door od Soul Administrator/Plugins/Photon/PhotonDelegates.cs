using DSSerializable.CharacterStructure;
using DSSerializable.WorldLevelStructure;
using System.Collections.Generic;

public partial class PhotonService
{
    public delegate void ConnectEventHandler(bool connectStatus);
    public event ConnectEventHandler ConnectEvent;

    public delegate void ControlTheSceneEventHandler(bool controlTheSceneStatus, string debugMessage, SerializableScene scene, List<SerializableContainer> containers);
    public event ControlTheSceneEventHandler ControlTheSceneEvent;

    //event
    public delegate void ProjectContainerToSceneEventHandler(int sceneUniqueID, SerializableContainer container);
    public event ProjectContainerToSceneEventHandler ProjectContainerToSceneEvent;

    public delegate void DisconnectEventHandler(int[] soulUniqueIDList, int[] sceneUniqueIDList, int[] containerUniqueIDList);
    public event DisconnectEventHandler DisconnectEvent;

    public delegate void MoveContainerToTargetPositionEventHandler(int sceneUniqueID,int containerUniqueID,float positionX,float positionY,float positionZ);
    public event MoveContainerToTargetPositionEventHandler MoveContainerToTargetPositionEvent;
}
