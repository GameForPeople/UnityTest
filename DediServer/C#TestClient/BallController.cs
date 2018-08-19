using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {

    float newPosX;
    Vector3 newMove;
    float speed = 4.0f;

    //public static int PlayerCount = 0;

    public int dirLeftOrRight = 0;
    bool isOnLeft = false;
    bool isOnRight = false;
    public bool isOnControl = false;
    public bool isJumping;
    new Rigidbody rigidbody;

    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody>();
	}
	
    // Update is called once per frame
    void Update()
    {
        if (isOnControl)
        {
            InputLeftOrRight();
            InputJump();
        }
    }

    // Update to rigidbody
    void FixedUpdate () {
        //네트워크로 컨트롤 하려면 이 옵션 꺼야함
        //if (isOnControl) 
        //{
            MoveLeftOrRight();
            Jump();
        //}
    }


    void SetIsControl(bool InIsOnCOntrol)
    {
        isOnControl = InIsOnCOntrol;
    }


    void MoveLeftOrRight()
    {
        newPosX = dirLeftOrRight * speed * Time.deltaTime;
        newMove.Set(newPosX, 0, 0);
        //transform.position = new Vector3(newPosX, transform.position.y, transform.position.z);
        rigidbody.MovePosition(transform.position + newMove);
    }

    void InputLeftOrRight()
    {
        if(Input.GetButtonDown("Left"))
        {
            isOnLeft = true;
        }
        else if (Input.GetButtonUp("Left"))
        {
            isOnLeft = false;
        }

        if (Input.GetButtonDown("Right"))
        { 
            isOnRight = true;
        }
        else if (Input.GetButtonUp("Right")) 
        {
            isOnRight = false;
        }

        if (isOnLeft && isOnRight)
        {
            if(dirLeftOrRight == 1)
            {
                dirLeftOrRight = -1;
            }
            else
            {
                dirLeftOrRight = 1;
            }
        }
        else if (isOnLeft)
        {
            dirLeftOrRight = -1;
        }
        else if (isOnRight)
        {
            dirLeftOrRight = 1;
        }
        else
        {
            dirLeftOrRight = 0;
        }
    }

    void Jump()
    {
        if (!isJumping)
            return;

        rigidbody.AddForce(Vector3.up * 7.5f , ForceMode.Impulse);

        isJumping = false;
    }

    void InputJump()
    {
        if (Input.GetButtonDown("Jump"))
            isJumping = true;
    }

    void ProcessRecvData(int InMixedData)
    {
        Debug.Log("LogMessage : " + InMixedData);

        if(InMixedData / 10 == 0)
        {
            dirLeftOrRight = 0;
        }
        else if (InMixedData / 10 == 1)
        {
            dirLeftOrRight = -1;
        }
        else if (InMixedData / 10 == 2)
        {
            dirLeftOrRight = 1;
        }
        if (InMixedData % 10 == 1)
        {
            isJumping = true;
        }
    }
}
