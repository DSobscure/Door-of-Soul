using UnityEngine;

public class PhotonCallService : MonoBehaviour
{

    void FixedUpdate()
    {
        PhotonGlobal.PS.Service();
    }

    void OnApplicationQuit()
    {
        PhotonGlobal.PS.Disconnect();
    }
}
