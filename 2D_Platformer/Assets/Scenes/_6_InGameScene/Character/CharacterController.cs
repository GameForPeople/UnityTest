using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    float newPosX;
    Vector3 newMove;

    int hp = 1;

    float speed = 8.0f;
    float jumpPower = 800.0f;

    int moveDir = 0; // -1 left, +1 Right, 0 Stop
    public int dirLeftOrRightBuffer = 1; // -1 left, +1 Right

    public int realCharacterIndex; // 1 = hostChar, 2 = guestChar
    public bool isOnControl = false; // Super

    bool isOnLeft = false;
    bool isOnRight = false;

    public bool isOnJumping;
    public bool isjumpCount = false;
    int jumpTimer = 0;
    public int JUMP_MAX_TIMER = 12;

    bool isOnFire = false;

    bool isMobileOnJump = false;
    bool isMobileOnFire = false;
    bool isMobileOnLeft = false;
    bool isMobileOnRight = false;

    new Rigidbody2D rigidbody;
    GameObject networkManager;
    GameObject inGameSceneManager;

    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody2D>();
        inGameSceneManager = GameObject.Find("InGameSceneManager");
    }

    // Update is called once per frame
    void Update () {
        if (isOnControl)
        {
            InputProcess();
        }
    }

    void InputProcess()
    {
        InputLeftOrRight();
        InputJump();
        InputFire();
    }

    void InputLeftOrRight()
    {
        isOnLeft = isMobileOnLeft;
        isOnRight = isMobileOnRight;

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

        // 캐릭터 변환
        transform.localScale = new Vector3(dirLeftOrRightBuffer, 1, 1);
    }

    void InputJump()
    {
        //if (Input.GetButtonDown("Jump"))
        if (isMobileOnJump)
        {
            if (isjumpCount == false)
            {
                isjumpCount = true; // 더블점프 꺼놈
                isOnJumping = true;
                jumpTimer = JUMP_MAX_TIMER;
            }
        }
    }

    void InputFire()
    {
        if (isMobileOnFire)
        {
            if (!isOnFire)
            {
                Fire();
            }
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

    void MoveLeftOrRight()
    {
        newPosX = moveDir * speed * Time.deltaTime;
        newMove.Set(newPosX, 0, 0);
        //transform.position = new Vector3(newPosX, transform.position.y, transform.position.z);
        rigidbody.MovePosition(transform.position + newMove);
    }

    void Jump()
    {
        if (!isOnJumping)
            return;

        if (jumpTimer > 0)
        {
            rigidbody.AddForce(Vector3.up * jumpPower, ForceMode2D.Force);
            --jumpTimer;
        }
        else
        {
            isOnJumping = false;
        }
    }

    // 함수쓰지 말고, 멤버 변수로 직접하자..
    //void SetIsControl(bool InIsOnCOntrol)
    //{
    //    isOnControl = InIsOnCOntrol;
    //}

    public void MobileInputJump(bool InBoolValue)
    {
        if (isOnControl)
        {
            isMobileOnJump = InBoolValue;
        }
    }

    public void MobileInputFire(bool InBoolValue)
    {
        if (isOnControl)
        {
            isMobileOnFire = InBoolValue;
        }
    }

    public void MobileInputLeft(bool InBoolValue)
    {
        if (isOnControl)
        {
            isMobileOnLeft = InBoolValue;
        }
    }

    public void MobileInputRight(bool InBoolValue)
    {
        if (isOnControl)
        {
            isMobileOnRight = InBoolValue;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 이게 뭔 소리지...? 변수명이 쫌 이상해... isJumpCount...? --> 요고 코루틴으로 변경해야징
        if (!isOnJumping)
        {
            if (other.gameObject.layer == 8)
            {
                isjumpCount = false;
            }
        }
    }

    void Fire()
    {
        StartCoroutine("FireCoolTimeCoroutine");
    }

    IEnumerator FireCoolTimeCoroutine()
    {
        isOnFire = true;

        inGameSceneManager.GetComponent<InGameSceneManager>().PlayerAttack(realCharacterIndex, dirLeftOrRightBuffer, transform.position);

        yield return new WaitForSeconds(1.0f);

        isOnFire = false;
    }
}
