using DSSerializable.CharacterStructure;

public partial class PhotonService
{
    public delegate void ConnectEventHandler(bool connectStatus);
    public event ConnectEventHandler ConnectEvent;

    public delegate void OpenDSEventHandler(bool openDSStatus, string debugMessage, SerializableAnswer answer);
    public event OpenDSEventHandler OpenDSEvent;

    public delegate void GetSceneDataEventHandler(bool success, SerializableContainer scene, SerializableContainer[] containers);
    public event GetSceneDataEventHandler GetSceneDataEvent;
}
