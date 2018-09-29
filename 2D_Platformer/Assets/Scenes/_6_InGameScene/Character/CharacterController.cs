using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    // ---- please Release - Off! ----
    public bool ON_INSERT_FOR_TEST_PC_KEY = false; 
    // -----------------------------

    float newPosX;
    Vector3 newMove;

    public int hp = 1;

    float speed = 8.0f;
    float jumpPower = 800.0f;

    float deathRotateAngle = 0.0f;

    int moveDir = 0; // -1 left, +1 Right, 0 Stop
    public int dirLeftOrRightBuffer = 1; // -1 left, +1 Right

    public int realCharacterIndex; // 1 = hostChar, 2 = guestChar
    public bool isOnControl = false; // Super

    bool isOnLeft = false;
    bool isOnRight = false;

    public bool isOnJumping;
    public bool isRenewJump = true;
    int jumpTimer = 0;
    public int JUMP_MAX_TIMER = 12;

    bool isOnFire = false;

    bool isMobileOnJump = false;
    bool isMobileOnFire = false;
    bool isMobileOnLeft = false;
    bool isMobileOnRight = false;

    bool networkJumpBuffer = false;
    bool networkFireBuffer = false;

    new Rigidbody2D rigidbody;
    GameObject networkManager;
    public InGameSceneManager inGameSceneManager;
    ProjectileController projectileController;

    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody2D>();
        inGameSceneManager = GameObject.Find("InGameSceneManager").GetComponent<InGameSceneManager>();
        projectileController = GameObject.Find("ProjectileController").GetComponent<ProjectileController>();
    }

    // Update is called once per frame
    void Update () {
        if (isOnControl)
        {
            InputProcess();
        }
    }

    private void InputProcess()
    {
        InputLeftOrRight();
        InputLeftOrRightProcess();

        InputJump();
        InputFire();
    }

    void InputLeftOrRight()
    {
        if (ON_INSERT_FOR_TEST_PC_KEY)
        {
            isOnLeft = Input.GetButton("Left");
            isOnRight = Input.GetButton("Right");
        }
        else
        {
            isOnLeft = isMobileOnLeft;
            isOnRight = isMobileOnRight;
        }
    }

    void InputLeftOrRightProcess()
    {
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
        if (isRenewJump)
        {
            if (ON_INSERT_FOR_TEST_PC_KEY)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    isOnJumping = true;
                    isRenewJump = false;
                    jumpTimer = JUMP_MAX_TIMER;
                }
            }
            else
            {
                if (isMobileOnJump)
                {
                    isOnJumping = true;
                    isRenewJump = false;
                    jumpTimer = JUMP_MAX_TIMER;
                    networkJumpBuffer = true;
                }
            }
        }
    }

    void InputFire()
    {
        if (ON_INSERT_FOR_TEST_PC_KEY)
        {
            if (Input.GetButtonDown("Fire"))
            {
                if (!isOnFire)
                {
                    Fire();
                }
            }
        }
        else
        {
            if (isMobileOnFire)
            {
                if (!isOnFire)
                {
                    Fire();
                }
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



    // 함수쓰지 말고, 멤버 변수로 직접하자..
    //void SetIsControl(bool InIsOnCOntrol)
    //{
    //    isOnControl = InIsOnCOntrol;
    //}
    public void RecvDataProcess(float InPosX, float InPosY, bool InInputLeft, bool InInputRight, bool InIsJump, bool InIsFire)
    {
        isOnLeft = InInputLeft;
        isOnRight = InInputRight;
        InputLeftOrRightProcess();

        transform.position = new Vector2(InPosX, InPosY);

        if (InIsJump)
        {
            isRenewJump = false;
            jumpTimer = JUMP_MAX_TIMER;
        }

        if (InIsFire)
        {
            Fire();
        }
    }

    public void SendDataProcess(ref float OutPosX, ref float OutPosY, ref bool OutInputLeft, ref bool OutInputRight, ref bool OutIsJump, ref bool OutIsFire)
    {
        OutInputLeft = isOnLeft;
        OutInputRight = isOnRight;
        OutIsJump = networkJumpBuffer;
        OutIsFire = networkFireBuffer;
        OutPosX = transform.position.x;
        OutPosY = transform.position.y;

        networkJumpBuffer = false;
        networkFireBuffer = false;
    }

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
        // 점프 중일 때, 만약 떨어지는 중이면
        if (!isOnJumping)
        {
            //점프를 한번 갱신할 수 있습니다.
            isRenewJump = true;
        }
    }

    void Jump()
    {
        //----
        //isOnJumping = true;
        //isRenewJump = false;
        //jumpTimer = JUMP_MAX_TIMER;
        //----
        //StartCoroutine("JumpCoroutine");
        if (isRenewJump)
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

    //IEnumerator JumpCoroutune()
    //{
    //    while (jumpTimer > 0)
    //    {
    //        rigidbody.AddForce(Vector3.up * jumpPower, ForceMode2D.Force);
    //        --jumpTimer;
    //
    //        yield return new WaitForSeconds(1.0f / 60.0f);
    //    }
    //    isOnJumping = false;
    //}

    void Fire()
    {
        StartCoroutine("FireCoolTimeCoroutine");
    }

    IEnumerator FireCoolTimeCoroutine()
    {
        isOnFire = true;
        networkFireBuffer = true;

        //inGameSceneManager.PlayerAttackProcess(realCharacterIndex, dirLeftOrRightBuffer, posBuffer);
        
        // or 

        if(realCharacterIndex == 1)
        {
           projectileController.GetComponent<ProjectileController>().AttackFireBall(dirLeftOrRightBuffer, transform.position);
        }
        else
        {
           projectileController.GetComponent<ProjectileController>().AttackLightBall(dirLeftOrRightBuffer, transform.position);
        }

        yield return new WaitForSeconds(0.5f);

        isOnFire = false;
    }

    public void Death()
    {
        hp = 0;
        StartCoroutine("DeathAnimationCoroutine");
    }

    IEnumerator DeathAnimationCoroutine()
    {
        while(deathRotateAngle < 90.0f)
        {
            deathRotateAngle += 2.0f;

            transform.rotation = Quaternion.Euler(new Vector3(0, 0, deathRotateAngle));
            
            yield return new WaitForSeconds(1.0f / 60.0f);
        }
    }
}
