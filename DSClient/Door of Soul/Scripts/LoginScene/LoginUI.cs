using UnityEngine;
using System.Collections;
using System;
using DSSerializable.CharacterStructure;

public class LoginUI : MonoBehaviour {

    private string account = "";
    private string password = "";
    private string openDSResult = "";

    IEnumerator Start()
    {

        PhotonGlobal.PS.OpenDSEvent += OpenDSEventAction;
        yield return null;
    }

    // Update is called once per frame
    void OnGUI()
    {
        try
        {
            GUI.Label(new Rect(30, 10, 100, 20), "AK - ");

            if (PhotonGlobal.PS.ServerConnected)
            {
                GUI.Label(new Rect(130, 10, 100, 20), "Connecting . . .");

                if (!AnswerGlobal.LoginStatus)
                {
                    GUI.Label(new Rect(30, 40, 200, 20), "Please Login");

                    GUI.Label(new Rect(30, 70, 80, 20), "帳號:");
                    account = GUI.TextField(new Rect(110, 70, 100, 20), account, 17);

                    GUI.Label(new Rect(30, 100, 80, 20), "密碼:");
                    password = GUI.PasswordField(new Rect(110, 100, 100, 20), password, '*', 17);
                    if (GUI.Button(new Rect(30, 130, 100, 24), "登入"))
                    {
                        PhotonGlobal.PS.OpenDS(account, password);
                    }
                    GUI.Label(new Rect(30, 160, 600, 20), openDSResult);
                }
                else
                {
                    PhotonGlobal.PS.OpenDSEvent -= OpenDSEventAction;
                    GUI.Label(new Rect(30, 70, 80, 20), AnswerGlobal.Answer.Name);
                    //Application.LoadLevel("testScene");
                }
            }
            else
            {
                GUI.Label(new Rect(130, 10, 200, 20), "Disconnect");
            }
        }
        catch (Exception EX)
        {
            Debug.Log(EX.Message);
        }
    }

    private void OpenDSEventAction(bool openDSStatus, string debugMessage, SerializableAnswer answer)
    {
        if (openDSStatus)
        {
            AnswerGlobal.Answer = new Answer(answer);
            AnswerGlobal.LoginStatus = true;
        }
        else
        {
            AnswerGlobal.LoginStatus = false;
            openDSResult = debugMessage;
        }
    }
}
