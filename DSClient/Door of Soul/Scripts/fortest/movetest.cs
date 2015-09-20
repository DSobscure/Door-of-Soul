using UnityEngine;
using System;

public class movetest : MonoBehaviour {

    public Rigidbody rigidbody;
    public LayerMask hitLayer;
    public Vector3 targetPosition;
    private bool moving = false;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 30, hitLayer.value))
        {
            if (Input.GetMouseButtonDown(0))
            {
                targetPosition = hit.point;
                moving = true;
            }
        }
    }

    void FixedUpdate()
    {
        if(moving)
        {
            Vector3 velocity = new Vector3(x: Convert.ToSingle(Math.Round(targetPosition.x - rigidbody.position.x, 1)),
                                           y: 0,
                                           z: Convert.ToSingle(Math.Round(targetPosition.z - rigidbody.position.z, 1)));
            rigidbody.velocity = velocity.normalized * 5;
            moving = (velocity.magnitude == 0) ? false : true;
        }
    }
}
