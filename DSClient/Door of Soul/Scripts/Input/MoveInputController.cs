using UnityEngine;
using System.Collections;

public class MoveInputController : MonoBehaviour {

    public LayerMask hitLayer;

	// Update is called once per frame
	void Update () {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, hitLayer.value))
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 targetPosition = hit.point;
                targetPosition.y = 0;
                PhotonGlobal.PS.SendMoveTargetPosition(AnswerGlobal.Scene.UniqueID, AnswerGlobal.MainContainer.UniqueID, targetPosition);
                Debug.Log("move!");
            }
        }
	}
}
