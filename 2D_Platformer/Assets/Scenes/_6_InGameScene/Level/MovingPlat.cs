using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlat : MonoBehaviour {

    public Vector3 startLocation;
    public Vector3 endLocation;
    public bool moveDirection;
    public float directionChangeTime;
    public float directionSpeed;

    Vector3 forwardMoveDirection;
    Vector3 reverseMoveDirection;

    int moveDistance;
    //int reverseMoveDistance;

    new Rigidbody2D rigidbody;

    bool isObjectRun = true;

    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody2D>();

        forwardMoveDirection = endLocation - startLocation;
        reverseMoveDirection = startLocation - endLocation;

        moveDistance = (int)forwardMoveDirection.magnitude;
        //reverseMoveDistance = (int)reverseMoveDirection.magnitude;

        forwardMoveDirection = forwardMoveDirection / moveDistance;
        reverseMoveDirection = -1 * forwardMoveDirection;

        StartCoroutine("MoveDirectionCoroutine");
    }

    // Update is called once per frame
    //void Update ()
    //{
    //    StartCoroutine("MoveDirectionCoroutine");
	//}

    void FixedUpdate()
    {
        if (isObjectRun)
        {
            if (moveDirection)
            {
                rigidbody.MovePosition(transform.position + forwardMoveDirection * directionSpeed);
            }
            else //if(!moveDirection)
            {
                rigidbody.MovePosition(transform.position + reverseMoveDirection * directionSpeed);
            }
        }
    }

    IEnumerator MoveDirectionCoroutine()
    {
        while (isObjectRun)
        {
            yield return new WaitForSeconds(directionChangeTime);
            moveDirection = !moveDirection;
        }
    }

    public void ObjectRunSwitch(bool InBoolValue) // InGameScene Controll
    {
        isObjectRun = InBoolValue;

        if(isObjectRun) // fail -> true
        {
            StartCoroutine("MoveDirectionCoroutine");
        }
        // true -> fail 자동으로 코루틴 종료.
    }
}
