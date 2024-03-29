﻿using UnityEngine;
using System.Collections;
using System;

public class ContainerMoveController : MonoBehaviour {

    public Rigidbody rigidbody;
    public Vector3 targetPosition;
    public bool moving = false;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (moving)
        {
            Vector3 velocity = new Vector3(x: Convert.ToSingle(Math.Round(targetPosition.x - rigidbody.position.x, 1)),
                                           y: 0,
                                           z: Convert.ToSingle(Math.Round(targetPosition.z - rigidbody.position.z, 1)));
            rigidbody.velocity = velocity.normalized * 5;
            if(velocity.magnitude == 0)
            {
                moving = false;
                AnswerGlobal.MainContainer.TargetPostion = transform.position;
                AnswerGlobal.MainContainer.Moving = false;
            }
        }
    }
}
