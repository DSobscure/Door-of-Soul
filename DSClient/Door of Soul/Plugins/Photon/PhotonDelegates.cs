using DSSerializable.CharacterStructure;
using DSSerializable.WorldLevelStructure;

public partial class PhotonService
{
    public delegate void ConnectEventHandler(bool connectStatus);
    public event ConnectEventHandler ConnectEvent;

    public delegate void OpenDSEventHandler(bool openDSStatus, string debugMessage, SerializableAnswer answer);
    public event OpenDSEventHandler OpenDSEvent;

    public delegate void GetSoulListEventHandler(bool getSoulListStatus, string debugMessage, SerializableSoul[] soulList);
    public event GetSoulListEventHandler GetSoulListEvent;

    public delegate void GetContainerListEventHandler(bool getContainerListStatus, string debugMessage, SerializableContainer[] containerList);
    public event GetContainerListEventHandler GetContainerListEvent;

    public delegate void ProjectToSceneEventHandler(bool projectToSceneStatus, string debugMessage);
    public event ProjectToSceneEventHandler ProjectToSceneEvent;

    

    public delegate void GetSceneDataEventHandler(bool getSceneDataStatus, string debugMessage, SerializableScene scene, SerializableContainer[] containers);
    public event GetSceneDataEventHandler GetSceneDataEvent;

    //event
    public delegate void ProjectContainerToSceneEventHandler(int sceneUniqueID, SerializableContainer container);
    public event ProjectContainerToSceneEventHandler ProjectContainerToSceneEvent;

    public delegate void ActiveSoulEventHandler(int soulUniqueID);
    public event ActiveSoulEventHandler ActiveSoulEvent;

    public delegate void DisconnectEventHandler(int[] soulUniqueIDList, int[] sceneUniqueIDList, int[] containerUniqueIDList);
    public event DisconnectEventHandler DisconnectEvent;

    public delegate void UpdateContainerPositionEventHandler(int sceneUniqueID, int containerUniqueID, float positionX, float positionY, float positionZ);
    public event UpdateContainerPositionEventHandler UpdateContainerPositionEvent;

    public delegate void MoveTargetPositionEventHandler(int sceneUniqueID, int containerUniqueID, float positionX, float positionY, float positionZ);
    public event MoveTargetPositionEventHandler MoveTargetPositionEvent;
}
