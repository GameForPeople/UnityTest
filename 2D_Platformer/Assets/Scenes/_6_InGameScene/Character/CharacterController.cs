using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    float newPosX;
    Vector3 newMove;
    float speed = 4.0f;

    int moveDir = 0; // -1 left, +1 Right, 0 Stop
    public int dirLeftOrRightBuffer = 1; // -1 left, +1 Right

    bool isOnLeft = false;
    bool isOnRight = false;
    public bool isOnControl = false;
    public bool isOnJumping;

    new Rigidbody2D rigidbody;

    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update () {
        if (isOnControl)
        {
            InputLeftOrRight();
            InputJump();
        }
    }

    // Update to rigidbody
    void FixedUpdate()
    {
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
        newPosX = moveDir * speed * Time.deltaTime;
        newMove.Set(newPosX, 0, 0);
        //transform.position = new Vector3(newPosX, transform.position.y, transform.position.z);
        rigidbody.MovePosition(transform.position + newMove);
    }

    void InputLeftOrRight()
    {
        if (Input.GetButtonDown("Left"))
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
            if (moveDir == 1)
            {
                moveDir = -1;
                dirLeftOrRightBuffer = -1;
            }
            else
            {
                moveDir = 1;
                dirLeftOrRightBuffer = 1;
            }
        }
        else if (isOnLeft)
        {
            moveDir = -1;
            dirLeftOrRightBuffer = -1;
        }
        else if (isOnRight)
        {
            moveDir = 1;
            dirLeftOrRightBuffer = 1;
        }
        else
        {
            moveDir = 0;
        }
    }

    void Jump()
    {
        if (!isOnJumping)
            return;

        rigidbody.AddForce(Vector3.up * 50f, ForceMode2D.Impulse);

        isOnJumping = false;
    }

    void InputJump()
    {
        if (Input.GetButtonDown("Jump"))
            isOnJumping = true;
    }
}
