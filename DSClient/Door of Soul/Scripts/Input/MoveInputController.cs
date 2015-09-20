using UnityEngine;
using System.Collections;
using System;

public class MoveInputController : MonoBehaviour {

    public LayerMask hitLayer;

	// Update is called once per frame
	void Update () {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()&&Physics.Raycast(ray, out hit, 100, hitLayer.value))
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 targetPosition = hit.point;
                targetPosition.y = 0;
                PhotonGlobal.PS.SendMoveTargetPosition(AnswerGlobal.Scene.UniqueID, AnswerGlobal.MainContainer.UniqueID, targetPosition);
            }
        }
	}

    public Rigidbody rigidbody;
    public Vector3 targetPosition;
    public bool moving = false;

    IEnumerable Start()
    {
        yield return new WaitForSeconds(2);
        rigidbody = AnswerGlobal.MainContainer.GameObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (moving)
        {
            Vector3 velocity = new Vector3(x: Convert.ToSingle(Math.Round(targetPosition.x - rigidbody.position.x, 1)),
                                           y: 0,
                                           z: Convert.ToSingle(Math.Round(targetPosition.z - rigidbody.position.z, 1)));
            rigidbody.velocity = velocity.normalized * 5;
            if (velocity.magnitude == 0)
            {
                moving = false;
                AnswerGlobal.MainContainer.TargetPostion = transform.position;
                AnswerGlobal.MainContainer.Moving = false;
            }
        }
    }
}
