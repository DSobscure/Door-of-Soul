using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using DSProtocol;

public class MessagePanelController : MonoBehaviour
{
    public List<string> MessageContent = new List<string>();
    private StringBuilder showingContent = new StringBuilder();
    private int maxLineCount = 10;
    private int currentLineIndex = 0;

    [SerializeField]
    private Text showingText;
    [SerializeField]
    private Scrollbar scrollbar;
    [SerializeField]
    private InputField inputText;

    public void MessagePanelSwitch()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    public void UpdateMessageBox()
    {
        currentLineIndex = Mathf.Max((int)(MessageContent.Count * scrollbar.value)- maxLineCount,0);
        scrollbar.size = Mathf.Min((float)maxLineCount / Mathf.Max((MessageContent.Count),1),1f);

        showingContent.Remove(0, showingContent.Length);
        for (int i = 0, index = currentLineIndex; i < maxLineCount && index < MessageContent.Count; i++, index++)
        {
            showingContent.AppendLine(MessageContent[index]);
        }
        showingText.text = showingContent.ToString();
    }

    public void SendMessage()
    {
        PhotonGlobal.PS.SendMessage(AnswerGlobal.MainContainer.UniqueID,MessageLevel.Scene, inputText.text);
        inputText.text = "";
    }
}
