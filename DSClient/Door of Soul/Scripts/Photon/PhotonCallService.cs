using UnityEngine;

public class PhotonCallService : MonoBehaviour
{

    void Update()
    {
        PhotonGlobal.PS.Service();
    }

    void OnApplicationQuit()
    {
        PhotonGlobal.PS.Disconnect();
    }
}
