using UnityEngine;
using System.Collections;

public class PhotonConnect : MonoBehaviour {

    public string ServerIP = "114.36.107.240";
    public int ServerPort = 5055;
    public string ServerName = "DSServer";
    private bool ConnectStatus = true;

    // Use this for initialization
    void Start()
    {
        if (!PhotonGlobal.PS.ServerConnected)
        {
            PhotonGlobal.PS.ConnectEvent += ConnectEventAction;
            PhotonGlobal.PS.Connect(ServerIP, ServerPort, ServerName);
        }
    }

    private void ConnectEventAction(bool Status)
    {
        if (Status)
        {
            Debug.Log("Connecting . . . . .");
            ConnectStatus = true;
        }
        else
        {
            Debug.Log("Connect Fail");
            ConnectStatus = false;
        }
    }

    private void OnDestroy()
    {
        PhotonGlobal.PS.ConnectEvent -= ConnectEventAction;
    }

    void OnGUI()
    {
        if (ConnectStatus == false)
        {
            GUI.Label(new Rect((Screen.width / 2) - 200, (Screen.height / 2) - 10, 400, 20), "Connect fail");
        }
    }
}
